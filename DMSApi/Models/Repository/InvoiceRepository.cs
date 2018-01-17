﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DMSApi.Models.crystal_models;
using DMSApi.Models.IRepository;
using DMSApi.Models.StronglyType;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.ComponentModel;
using System.Text;
using Newtonsoft.Json;

namespace DMSApi.Models.Repository
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private DMSEntities _entities;
        private PartyJournalRepository partyJournalRepository;
        private IMailRepository mailRepository;
        private INotifierMailAccountRepository notifierMailAccountRepository;



        public InvoiceRepository()
        {
            this._entities = new DMSEntities();
            this.partyJournalRepository = new PartyJournalRepository();
            this.mailRepository = new MailRepository();
            this.notifierMailAccountRepository = new NotifierMailAccountRepository();

        }
        public int AddInvoice(RequisitionModel rm)
        {
            var RequisitionMaster = rm.RequisitionMasterData;
            var ProductDetailsList = rm.RequisitionDetailsList;


            //Get Party type Prefix by party Id :Kiron:27/10/2016

            var partyTypePrefix = (from ptype in _entities.party_type
                                   join par in _entities.parties on ptype.party_type_id equals par.party_type_id
                                   where par.party_id == RequisitionMaster.party_id
                                   select new
                                   {
                                       party_prefix = ptype.party_prefix

                                   }).FirstOrDefault();
            //invoice master
            // generate invoice No
            int InvoiceSerial = _entities.invoice_master.Max(inv => (int?)inv.invoice_master_id) ?? 0;

            if (InvoiceSerial != 0)
            {
                InvoiceSerial++;

            }
            else
            {
                InvoiceSerial++;
            }
            var invStr = InvoiceSerial.ToString().PadLeft(7, '0');
            string invoiceNo = "INV-" + partyTypePrefix.party_prefix + "-" + invStr;
            invoice_master insert_invoice = new invoice_master
            {
                invoice_no = invoiceNo,
                invoice_date = System.DateTime.Now,
                party_id = RequisitionMaster.party_id,
                requisition_master_id = RequisitionMaster.requisition_master_id,
                company_id = RequisitionMaster.company_id,
                remarks = RequisitionMaster.remarks,
                created_by = RequisitionMaster.created_by,
                item_total = RequisitionMaster.amount,
                rebate_total = 0,
                invoice_total = 0,
                account_balance = 0,
            };

            ////GET REBATE TOTAL
            //decimal rebateTotal =
            //    _entities.requisition_rebate.Where(
            //        rb => rb.requisition_master_id == RequisitionMaster.requisition_master_id).Sum(s => s.rebate_amount)??0;

            //GET ACCOUNT BALANCE FROM PARTY JOURNAL
            var partyJournal =
                _entities.party_journal.Where(pj => pj.party_id == RequisitionMaster.party_id)
                    .OrderByDescending(p => p.party_journal_id)
                    .FirstOrDefault();
            decimal partyAccountBalance = 0;
            if (partyJournal != null)
            {
                partyAccountBalance = partyJournal.closing_balance ?? 0;
            }

            //CALCULATING INVOICE TOTAL
            decimal invoiceTotal = 0;
            invoiceTotal = RequisitionMaster.amount ?? 0; //insert in both invoice master and party journal table

            //ACCOUNT BALANCE
            decimal accountBalance = 0;
            accountBalance = invoiceTotal + partyAccountBalance;


            insert_invoice.invoice_total = invoiceTotal;
            //insert_invoice.rebate_total = rebateTotal;
            insert_invoice.account_balance = accountBalance;

            _entities.invoice_master.Add(insert_invoice);
            _entities.SaveChanges();
            long InvoiceMasterId = insert_invoice.invoice_master_id;

            //INVOICE DETALS
            foreach (var item in ProductDetailsList)
            {
                var invoiceDetails_insert = new invoice_details
                {
                    invoice_master_id = InvoiceMasterId,
                    product_category_id = item.product_category_id,
                    product_id = item.product_id,
                    color_id = item.color_id,
                    brand_id = item.brand_id,
                    unit_id = item.unit_id,
                    price = item.price,
                    quantity = item.quantity,
                    line_total = item.line_total,
                    is_gift = item.is_gift,
                    is_live_dummy = item.is_live_dummy

                };

                _entities.invoice_details.Add(invoiceDetails_insert);
                _entities.SaveChanges();
            }
            partyJournalRepository.PartyJournalEntry("INVOICE", RequisitionMaster.party_id ?? 0, invoiceTotal, "Invoice", RequisitionMaster.created_by ?? 0, invoiceNo);



            return 1;
        }
        public int AccountsForward(RequisitionModel RequisitionModel)
        {
            try
            {
                var RequisitionMaster = RequisitionModel.RequisitionMasterData;
                var pStatus = "";
                var paymentStatus = _entities.payment_request.FirstOrDefault(w => w.requisition_master_id == RequisitionMaster.requisition_master_id);
                if (paymentStatus != null)
                {
                    pStatus = "Forward to HOS with payment check";
                }
                else
                {
                    pStatus = "Forward to HOS";
                }
                requisition_master reqMaster = _entities.requisition_master.Find(RequisitionMaster.requisition_master_id);
                //06.03.2017
                //reqMaster.status = "Fwd to HOS";//status means Accounts forward to Head of sales
                reqMaster.status = pStatus;
                reqMaster.verified_by = RequisitionMaster.updated_by;
                reqMaster.verified_date = DateTime.Now;
                reqMaster.accounts_remarks = RequisitionMaster.accounts_remarks;
                _entities.SaveChanges();

                return 1;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public object GetInvoices(string fromDate, string toDate)
        {
            DateTime fDate = Convert.ToDateTime(fromDate);
            DateTime tDate = Convert.ToDateTime(toDate);
            //var getInvoice = _entities.invoice_master.ToList();
            //var getInvoice = _entities.invoice_master.Where(w => w.invoice_date > Convert.ToDateTime(fromDate).AddDays(-1) && w.invoice_date < Convert.ToDateTime(toDate).AddDays(1)).ToList();


            var getInvoice = _entities.invoice_master.Where(w => w.invoice_date >= fDate && w.invoice_date <= tDate).ToList();
            return getInvoice;


        }


        public object GetInvoicesByFromDate(string fromDate)
        {
            DateTime fDate = Convert.ToDateTime(fromDate);

            var getInvoice = _entities.invoice_master.Where(w => w.invoice_date == fDate).ToList();
            return getInvoice;
        }

        public object GetInvoice()
        {
            var getInvoice = _entities.invoice_master.ToList();
            return getInvoice;
        }

        //Accounts head approval --update the requisition table
        public int AddInvoiceNew(InvoiceModel invoiceModel)
        {
            var InvoiceMaster = invoiceModel.InvoiceMasterData;
            var InvoiceDetails = invoiceModel.InvoiceDetailsList;
            //var RequisitionRebate = invoiceModel.InvoiceRebateData;
            var RequisitionDetails = invoiceModel.RequisitionDetailsData;

            decimal InvoiceTotal = 0;// (invoiceModel.InvoiceMasterData.item_total - rebateTotal)??0;

            //GET ACCOUNT BALANCE FROM PARTY JOURNAL
            var partyJournal =
                _entities.party_journal.Where(pj => pj.party_id == invoiceModel.InvoiceMasterData.party_id)
                    .OrderByDescending(p => p.party_journal_id)
                    .FirstOrDefault();
            decimal partyAccountBalance = 0;
            if (partyJournal != null)
            {
                partyAccountBalance = partyJournal.closing_balance ?? 0;
            }

            //update requisition master table status column
            //29.04.2017(mohi uddin) start if payment verified or not
            var pStatus = "";
            var paymentStatus = _entities.payment_request.FirstOrDefault(w => w.requisition_master_id == invoiceModel.InvoiceMasterData.requisition_master_id);
            if (paymentStatus != null)
            {
                pStatus = "Forward to HOS with payment check";
            }
            else
            {
                pStatus = "Forward to HOS";
            }
            //29.04.2017 end
            requisition_master reqMaster = _entities.requisition_master.Find(invoiceModel.InvoiceMasterData.requisition_master_id);
            //06.03.2017
            //reqMaster.status = "Fwd to HOS";//status means Accounts forward to Head of sales
            reqMaster.status = pStatus;
            reqMaster.verified_by = InvoiceMaster.updated_by;
            reqMaster.verified_date = DateTime.Now;
            //reqMaster.accounts_remarks=InvoiceMaster.a
            _entities.SaveChanges();

            return 1;
        }
        //head of oeration forward to sales head
        public int HOOpsApproval(InvoiceModel invoiceModel)
        {
            var InvoiceMaster = invoiceModel.InvoiceMasterData;
            var InvoiceDetails = invoiceModel.InvoiceDetailsList;
            var RequisitionDetails = invoiceModel.RequisitionDetailsData;


            decimal InvoiceTotal = 0;

            //GET ACCOUNT BALANCE FROM PARTY JOURNAL
            //var partyJournal =
            //    _entities.party_journal.Where(pj => pj.party_id == invoiceModel.InvoiceMasterData.party_id)
            //        .OrderByDescending(p => p.party_journal_id)
            //        .FirstOrDefault();
            //decimal partyAccountBalance = 0;
            //if (partyJournal != null)
            //{
            //    partyAccountBalance = partyJournal.closing_balance ?? 0;
            //}

            //update requisition master table
            requisition_master reqMaster = _entities.requisition_master.Find(invoiceModel.InvoiceMasterData.requisition_master_id);
            //reqMaster.forward_2_status = "Forwarded";
            //06.03.2017
            reqMaster.forward_2_status = "Fwd to HOS";//forward_2_status means HOOps forward status
            reqMaster.operation_remarks = InvoiceMaster.operation_remarks;
            reqMaster.forward_2_by = InvoiceMaster.updated_by;
            reqMaster.forward_2_date = DateTime.Now;


            _entities.SaveChanges();

            return 1;
        }

        public int HOSpsApprovalByShafiq(InvoiceModel invoiceModel)
        {

            //12.06.2017 start(first requisition will edit)
            var RequisitionMaster = invoiceModel.InvoiceMasterData;
            var RequisitionDetailsList = invoiceModel.InvoiceDetailsList;


            requisition_master masterData = _entities.requisition_master.Find(RequisitionMaster.requisition_master_id);

            masterData.party_type_id = masterData.party_type_id;
            masterData.party_id = masterData.party_id;
            masterData.remarks = RequisitionMaster.remarks;
            masterData.is_invoice_created = masterData.is_invoice_created;
            masterData.delivery_status = masterData.delivery_status;

            masterData.created_by = masterData.created_by;
            masterData.created_date = masterData.created_date;
            masterData.updated_by = RequisitionMaster.updated_by;
            masterData.updated_date = DateTime.Now;

            masterData.region_id = masterData.region_id;
            masterData.area_id = masterData.area_id;
            masterData.company_id = masterData.company_id;
            masterData.credit_term = masterData.credit_term;
            masterData.contact_person_mobile = masterData.contact_person_mobile;
            masterData.address = masterData.address;
            masterData.territory_id = masterData.territory_id;

            masterData.reference_no = masterData.reference_no;
            masterData.price_type = masterData.price_type;

            _entities.SaveChanges();


            //requisition details delete and update
            var rdetails =
                _entities.requisition_details.Where(
                    w => w.requisition_master_id == RequisitionMaster.requisition_master_id).ToList();
            foreach (var list in rdetails)
            {
                _entities.requisition_details.Remove(list);
                _entities.SaveChanges();
            }

            foreach (var item in RequisitionDetailsList)
            {
                var requisitionDetails = new requisition_details
                {
                    requisition_master_id = RequisitionMaster.requisition_master_id,
                    product_id = item.product_id,
                    unit_id = item.unit_id,
                    brand_id = item.brand_id,
                    product_category_id = item.product_category_id,
                    price = item.price,
                    quantity = item.quantity,
                    color_id = item.color_id,
                    delivered_quantity = 0,
                    line_total = item.line_total,
                    product_version_id = item.product_version_id,
                    //is_gift = false,
                    is_gift = item.is_gift,
                    gift_type = item.gift_type,
                    is_live_dummy = item.is_live_dummy,
                    discount_amount = item.discount_amount,
                    promotion_master_id = item.promotion_master_id


                };
                _entities.requisition_details.Add(requisitionDetails);
                _entities.SaveChanges();
            }

            //12.06.2017 end

            requisition_master reqMaster = _entities.requisition_master.Find(invoiceModel.InvoiceMasterData.requisition_master_id);
            //23.04.2017-mohi uddin
            reqMaster.finance_status = "Approved";
            reqMaster.verified_date = DateTime.Now;
            reqMaster.verified_by = invoiceModel.InvoiceMasterData.updated_by;
            reqMaster.sales_head_recommendation = invoiceModel.InvoiceMasterData.sales_head_recommendation;



            _entities.SaveChanges();
            return 1;
        }



        //dealer head of dales approval(finance_status means sales status)
        public int DHOSpsApproval(InvoiceModel invoiceModel)
        {
            try
            {
                //11.06.2017 start(first requisition will edit)
                var RequisitionMaster = invoiceModel.InvoiceMasterData;//RequisitionModel.RequisitionMasterData;
                var RequisitionDetailsList = invoiceModel.InvoiceDetailsList;//RequisitionModel.RequisitionDetailsList;
                bool isRequisitionUpdated = false;


                requisition_master masterData = _entities.requisition_master.Find(RequisitionMaster.requisition_master_id);

                masterData.party_type_id = masterData.party_type_id;
                masterData.party_id = masterData.party_id;
                masterData.remarks = RequisitionMaster.remarks;
                masterData.is_invoice_created = masterData.is_invoice_created;
                masterData.delivery_status = masterData.delivery_status;

                masterData.created_by = masterData.created_by;
                masterData.created_date = masterData.created_date;
                masterData.updated_by = RequisitionMaster.updated_by;
                masterData.updated_date = DateTime.Now;

                masterData.region_id = masterData.region_id;
                masterData.area_id = masterData.area_id;
                masterData.company_id = masterData.company_id;
                masterData.credit_term = masterData.credit_term;
                masterData.contact_person_mobile = masterData.contact_person_mobile;
                masterData.address = masterData.address;
                masterData.territory_id = masterData.territory_id;

                masterData.reference_no = masterData.reference_no;
                masterData.price_type = masterData.price_type;

                _entities.SaveChanges();


                //requisition details delete and update
                var rdetails =
                    _entities.requisition_details.Where(
                        w => w.requisition_master_id == RequisitionMaster.requisition_master_id).ToList();
                foreach (var list in rdetails)
                {
                    _entities.requisition_details.Remove(list);
                    _entities.SaveChanges();
                }

                foreach (var item in RequisitionDetailsList)
                {
                    var requisitionDetails = new requisition_details
                    {
                        requisition_master_id = RequisitionMaster.requisition_master_id,
                        product_id = item.product_id,
                        unit_id = item.unit_id,
                        brand_id = item.brand_id,
                        product_category_id = item.product_category_id,
                        price = item.price,
                        quantity = item.quantity,
                        color_id = item.color_id,
                        delivered_quantity = 0,
                        line_total = item.line_total,
                        product_version_id = item.product_version_id,
                        //is_gift = false,
                        is_gift = item.is_gift,
                        gift_type = item.gift_type,
                        is_live_dummy = item.is_live_dummy,
                        discount_amount = item.discount_amount,
                        promotion_master_id = item.promotion_master_id


                    };
                    _entities.requisition_details.Add(requisitionDetails);
                    _entities.SaveChanges();
                }

                //11.06.2017 end


                requisition_master reqMaster = _entities.requisition_master.Find(invoiceModel.InvoiceMasterData.requisition_master_id);
                reqMaster.finance_status = "Approved";
                reqMaster.approved_by = invoiceModel.InvoiceMasterData.updated_by;
                reqMaster.approved_date = DateTime.Now;
                reqMaster.sales_head_recommendation = invoiceModel.InvoiceMasterData.sales_head_recommendation;


                //Check IsUpdate from Client Site
                isRequisitionUpdated = true;
                // Send Email Nofication When Updated
                //Get Mail Data
                if (isRequisitionUpdated == true)
                {
                    int counter = 0;
                    var requisitionEmail = "";


                    var dataSmtp = _entities.notifier_mail_account.FirstOrDefault(s => s.is_active == true);

                    var dataReceiver = (from mrs in _entities.mail_receiver_setting
                                        join spm in _entities.software_process_module on mrs.process_code_id equals spm.process_code_id
                                        select new
                                        {
                                            mail_receiver_setting_id = mrs.mail_receiver_setting_id,
                                            process_code_name = spm.process_code_name,
                                            process_code_id = spm.process_code_id,
                                            receiver_name = mrs.receiver_name,
                                            receiver_email = mrs.receiver_email,
                                            is_active = mrs.is_active,
                                            is_deleted = mrs.is_deleted,
                                            created_by = mrs.created_by,
                                            created_date = mrs.created_date,
                                            updated_by = mrs.updated_by,
                                            updated_date = mrs.updated_date
                                        }).Where(c => c.is_deleted != true && c.process_code_name == "REQUISITION")
                        .OrderByDescending(c => c.mail_receiver_setting_id)
                        .ToList();

                    var dataProcess = (from mrs in _entities.process_wise_mail_setting
                                       join spm in _entities.software_process_module on mrs.process_code_id equals spm.process_code_id
                                       select new
                                       {
                                           process_wise_mail_setting_id = mrs.process_wise_mail_setting_id,
                                           process_code_name = spm.process_code_name,
                                           process_code_id = spm.process_code_id,
                                           email_body = mrs.email_body,
                                           email_subject = mrs.email_subject,
                                           is_active = mrs.is_active,
                                           is_deleted = mrs.is_deleted,
                                           created_by = mrs.created_by,
                                           created_date = mrs.created_date,
                                           updated_by = mrs.updated_by,
                                           updated_date = mrs.updated_date
                                       }).FirstOrDefault(c => c.is_deleted != true && c.process_code_name == "REQUISITION");



                    //GET REQUISITION INFO
                    var requsitionInfo =
                        GetRequsitionInformatioForEmailNotificationById(RequisitionMaster.requisition_master_id ?? 0);
                    //GET WHO IS UPDATING
                    var approvedBy =
                        _entities.users.Where(d => d.user_id == RequisitionMaster.updated_by)
                            .Select(d => d.full_name)
                            .FirstOrDefault();

                    //PROCESS MAIL OBJECT
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<h4 style='color: red'>" + dataProcess.email_body + "</h4>");
                    sb.Append("<table>");
                    sb.Append("<tr>");
                    sb.Append("<td><b>REQUISITION NO</b></td>");
                    sb.Append("<td><b>: <span style='color: red'>" + requsitionInfo[0].requisition_code +
                              "</span></b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td><b>REQUISITION DATE</b></td>");
                    sb.Append("<td><b>: " + requsitionInfo[0].requisition_date + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td><b>REQUISITION TYPE</b></td>");
                    sb.Append("<td><b>: " + requsitionInfo[0].requisition_type + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td><b>CHANNEL NAME</b></td>");
                    sb.Append("<td><b>: " + requsitionInfo[0].party_name + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td><b>CHANNEL TYPE</b></td>");
                    sb.Append("<td><b>: " + requsitionInfo[0].party_type + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td><b>APPROVED BY By</b></td>");
                    sb.Append("<td><b>: " + approvedBy + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td><b>APPROVED DATE</b></td>");
                    sb.Append("<td><b>: " + DateTime.Now + "</b></td>");
                    sb.Append("</tr>");
                    sb.Append("</table>");
                    sb.Append("<br/>");
                    sb.Append("<table border='1px' cellpadding='7'>");
                    sb.Append("<tr>");
                    sb.Append("<th>Product Category</th>");
                    sb.Append("<th>Product Name</th>");
                    sb.Append("<th>Color</th>");
                    sb.Append("<th>Version</th>");
                    sb.Append("<th>Requisition Qty</th>");
                    sb.Append("<th>Discount Amount</th>");
                    sb.Append("<th>Line Total</th>");

                    sb.Append("</tr>");
                    foreach (var item in requsitionInfo)
                    {
                        sb.Append("<tr align='center'>");
                        sb.Append("<td>" + item.product_category_name + "</td>");
                        sb.Append("<td>" + item.product_name + "</td>");
                        sb.Append("<td>" + item.color_name + "</td>");
                        sb.Append("<td>" + item.product_version_name + "</td>");
                        sb.Append("<td>" + item.quantity + "</td>");
                        sb.Append("<td>" + item.discount_amount + "</td>");
                        sb.Append("<td>" + item.line_total + "</td>");
                        sb.Append("</tr>");
                    }
                    sb.Append("</table>");



                    string requisitionNumberEmailBody = sb.ToString();

                    foreach (var item in dataReceiver)
                    {
                        if (counter == 0)
                        {
                            requisitionEmail = item.receiver_email;
                        }
                        requisitionEmail += "," + item.receiver_email;
                        counter++;
                    }




                    //Send Confirmation Mail
                    mailRepository.SendMail(dataSmtp.account_email, requisitionEmail, dataProcess.email_subject,
                        requisitionNumberEmailBody, dataSmtp.account_email, dataSmtp.accoutn_password, "");


                }
                else
                {

                }



                _entities.SaveChanges();

                return 1;


            }
            catch (Exception)
            {
                throw;
            }
        }



        //mohi uddin sales approve(when sales approve then invoice will be created)
        public int CreateInvoice(InvoiceModel invoiceModel)
        {
            var InvoiceMaster = invoiceModel.InvoiceMasterData;
            var InvoiceDetails = invoiceModel.InvoiceDetailsList;

            //15.05.2017 start(first requisition will edit)
            var RequisitionMaster = invoiceModel.InvoiceMasterData;//RequisitionModel.RequisitionMasterData;
            var RequisitionDetailsList = invoiceModel.InvoiceDetailsList;//RequisitionModel.RequisitionDetailsList;
            bool isRequisitionUpdated = false;


            requisition_master masterData = _entities.requisition_master.Find(RequisitionMaster.requisition_master_id);

            masterData.party_type_id = masterData.party_type_id;
            masterData.party_id = masterData.party_id;
            masterData.remarks = RequisitionMaster.remarks;
            masterData.is_invoice_created = masterData.is_invoice_created;
            masterData.delivery_status = masterData.delivery_status;

            masterData.created_by = masterData.created_by;
            masterData.created_date = masterData.created_date;
            masterData.updated_by = RequisitionMaster.updated_by;
            masterData.updated_date = DateTime.Now;

            masterData.region_id = masterData.region_id;
            masterData.area_id = masterData.area_id;
            masterData.company_id = masterData.company_id;
            masterData.credit_term = masterData.credit_term;
            masterData.contact_person_mobile = masterData.contact_person_mobile;
            masterData.address = masterData.address;
            masterData.territory_id = masterData.territory_id;

            masterData.reference_no = masterData.reference_no;
            masterData.price_type = masterData.price_type;

            _entities.SaveChanges();

            //requisition details delete and update
            var rdetails =
                _entities.requisition_details.Where(
                    w => w.requisition_master_id == RequisitionMaster.requisition_master_id).ToList();
            foreach (var list in rdetails)
            {
                _entities.requisition_details.Remove(list);
                _entities.SaveChanges();
            }

            foreach (var item in RequisitionDetailsList)
            {
                var requisitionDetails = new requisition_details
                {
                    requisition_master_id = RequisitionMaster.requisition_master_id,
                    product_id = item.product_id,
                    unit_id = item.unit_id,
                    brand_id = item.brand_id,
                    product_category_id = item.product_category_id,
                    price = item.price,
                    quantity = item.quantity,
                    color_id = item.color_id,
                    delivered_quantity = 0,
                    line_total = item.line_total,
                    product_version_id = item.product_version_id,
                    //is_gift = false,
                    is_gift = item.is_gift,
                    gift_type = item.gift_type,
                    is_live_dummy = item.is_live_dummy,
                    discount_amount = item.discount_amount,
                    promotion_master_id = item.promotion_master_id


                };
                _entities.requisition_details.Add(requisitionDetails);
                _entities.SaveChanges();
            }

            //15.05.2017 end


            var partyTypePrefix = (from ptype in _entities.party_type
                                   join par in _entities.parties on ptype.party_type_id equals par.party_type_id
                                   where par.party_id == InvoiceMaster.party_id
                                   select new
                                   {
                                       party_prefix = ptype.party_prefix

                                   }).FirstOrDefault();

            DateTime TodayDate = System.DateTime.Today;
            DateTime ReceivingDate = DateTime.Today.AddDays(-27);

            //calculating rebate total and invoice total
            //decimal rebateTotal =
            //    _entities.requisition_rebate.Where(
            //        rb => rb.requisition_master_id == invoiceModel.InvoiceMasterData.requisition_master_id).Sum(s => s.rebate_amount) ?? 0;

            //decimal InvoiceTotal = (invoiceModel.InvoiceMasterData.item_total - rebateTotal) ?? 0;
            decimal InvoiceTotal = (invoiceModel.InvoiceMasterData.item_total) ?? 0;

            //GET ACCOUNT BALANCE FROM PARTY JOURNAL
            var partyJournal =
                _entities.party_journal.Where(pj => pj.party_id == invoiceModel.InvoiceMasterData.party_id)
                    .OrderByDescending(p => p.party_journal_id)
                    .FirstOrDefault();
            decimal partyAccountBalance = 0;
            if (partyJournal != null)
            {
                partyAccountBalance = partyJournal.closing_balance ?? 0;
            }



            //invoice master
            int invoiceserial = _entities.invoice_master.Max(inv => (int?)inv.invoice_master_id) ?? 0;

            if (invoiceserial != 0)
            {
                invoiceserial++;
            }
            else
            {
                invoiceserial++;
            }
            var invstr = invoiceserial.ToString().PadLeft(7, '0');
            string invoiceno = "INV-" + partyTypePrefix.party_prefix + "-" + invstr;

            InvoiceMaster.invoice_no = invoiceno;
            InvoiceMaster.invoice_date = DateTime.Now;
            InvoiceMaster.party_id = invoiceModel.InvoiceMasterData.party_id;
            InvoiceMaster.requisition_master_id = invoiceModel.InvoiceMasterData.requisition_master_id;
            InvoiceMaster.item_total = invoiceModel.InvoiceMasterData.item_total;
            InvoiceMaster.company_id = invoiceModel.InvoiceMasterData.company_id;
            InvoiceMaster.invoice_total = InvoiceTotal;
            InvoiceMaster.account_balance = partyAccountBalance;
            InvoiceMaster.remarks = invoiceModel.InvoiceMasterData.remarks;
            InvoiceMaster.created_by = invoiceModel.InvoiceMasterData.created_by;
            InvoiceMaster.credit_limit = invoiceModel.InvoiceMasterData.credit_limit;
            InvoiceMaster.party_type_id = invoiceModel.InvoiceMasterData.party_type_id;

            InvoiceMaster.credit_term = invoiceModel.InvoiceMasterData.credit_term;
            InvoiceMaster.contact_person_mobile = invoiceModel.InvoiceMasterData.contact_person_mobile;
            InvoiceMaster.address = invoiceModel.InvoiceMasterData.address;
            InvoiceMaster.reference_no = invoiceModel.InvoiceMasterData.reference_no;
            InvoiceMaster.price_type = invoiceModel.InvoiceMasterData.price_type;
            InvoiceMaster.requisition_amount = invoiceModel.InvoiceMasterData.requisition_amount;//15.05.2017


            _entities.invoice_master.Add(InvoiceMaster);
            _entities.SaveChanges();
            long invoicemasterid = InvoiceMaster.invoice_master_id;

            //get party type(MD/DIBS)
            // var partytypeId = (from rm in _entities.requisition_master
            //                    join p in _entities.parties on rm.party_id equals p.party_id
            //                    join pt in _entities.party_type on p.party_type_id equals pt.party_type_id
            //                    where (rm.party_id == invoiceModel.InvoiceMasterData.party_id)
            //                    select new
            //                    {
            //                        party_prefix = pt.party_prefix
            //                    }).ToList().FirstOrDefault();
            //var ptTypeName = partytypeId.party_prefix;

            //invoice details table
            foreach (var item in InvoiceDetails)
            {
                var invoiceDetails = new invoice_details
                {
                    invoice_master_id = invoicemasterid,
                    product_category_id = item.product_category_id,
                    product_id = item.product_id,
                    quantity = item.quantity,
                    price = item.price,
                    unit_id = item.unit_id,
                    color_id = item.color_id,
                    line_total = item.line_total,
                    brand_id = item.brand_id,
                    is_gift = item.is_gift,
                    gift_type = item.gift_type,
                    is_live_dummy = item.is_live_dummy,
                    product_version_id = item.product_version_id,
                    promotion_master_id = item.promotion_master_id,
                    discount_amount = item.discount_amount

                };
                _entities.invoice_details.Add(invoiceDetails);
                _entities.SaveChanges();
            }

            //update invoice_master table invoice_total column(19.11.2016)
            decimal incentive_amount = 0;
            incentive_amount = invoiceModel.InvoiceMasterData.incentive_amount ?? 0;
            InvoiceTotal = invoiceModel.InvoiceMasterData.item_total ?? 0;//(invoiceModel.InvoiceMasterData.item_total - (rebateTotal + incentive_amount + priceProtectionAmount)) ?? 0;
            invoice_master invmaster = _entities.invoice_master.Find(invoicemasterid);
            invmaster.invoice_total = InvoiceTotal;

            //update requisition master table status column
            requisition_master reqMaster = _entities.requisition_master.Find(invoiceModel.InvoiceMasterData.requisition_master_id);
            reqMaster.is_invoice_created = true;
            reqMaster.forward_2_status = "Approved";//for aamra it is HOOps approval
            reqMaster.forward_2_date = DateTime.Now;
            reqMaster.forward_2_by = invoiceModel.InvoiceMasterData.created_by;
            reqMaster.sales_head_recommendation = invoiceModel.InvoiceMasterData.sales_head_recommendation;
            reqMaster.operation_remarks = invoiceModel.InvoiceMasterData.operation_remarks;
            reqMaster.amount = invoiceModel.InvoiceMasterData.requisition_amount;
            _entities.SaveChanges();

            //insert data into party journal table
            partyJournalRepository.PartyJournalEntry("INVOICE", invoiceModel.InvoiceMasterData.party_id ?? 0, InvoiceTotal, "Invoice", invoiceModel.InvoiceMasterData.created_by ?? 0, invoiceno);

            //update receive table field received_invoice_no
            //var receiveList = _entities.receives.Where(r => r.party_id == invoiceModel.InvoiceMasterData.party_id && (r.received_invoice_no == null || r.received_invoice_no == "")).ToList();
            //update on 18.05.2017
            var receiveList = _entities.receives.Where(r => r.party_id == invoiceModel.InvoiceMasterData.party_id && (r.received_invoice_no == null || r.received_invoice_no == "") && r.requisition_master_id == invoiceModel.InvoiceMasterData.requisition_master_id).ToList();
            foreach (var r in receiveList)
            {
                r.received_invoice_no = invoiceno;
                _entities.SaveChanges();
            }

            //insert invoice_log table
            decimal receivedAmount = 0;
            decimal balanceAmount = 0;
            decimal previousInvoiceAdvance = 0;
            decimal previousInvoiceDue = 0;
            decimal priceProtectedAmount = 0;

            receivedAmount = _entities.receives.Where(w => w.received_invoice_no == invoiceno).Sum(s => s.amount) ?? 0;
            balanceAmount = InvoiceTotal - receivedAmount;

            var previousinvoicebalance = _entities.receives.Where(w => w.received_invoice_no == invoiceno).OrderByDescending(o => o.receive_id).FirstOrDefault();
            if (previousinvoicebalance != null)
            {
                decimal previousInvoiceBalance = 0;

                previousInvoiceBalance = Convert.ToDecimal(previousinvoicebalance.last_invoice_balance);
                if (previousInvoiceBalance < 0)
                {
                    previousInvoiceDue = previousInvoiceBalance;
                    previousInvoiceAdvance = 0;
                }
                else
                {
                    previousInvoiceDue = 0;
                    previousInvoiceAdvance = previousInvoiceBalance;
                }
            }

            //priceProtectedAmount = _entities.price_protection.Where(w => w.invoice_master_id == invoicemasterid).Sum(s => s.rebate) ?? 0;

            invoice_master invoiceDataForDump = _entities.invoice_master.Where(w => w.invoice_master_id == invoicemasterid).FirstOrDefault();
            if (invoiceDataForDump != null)
            {
                invoice_log invoiceLogInsert = new invoice_log
                {
                    invoice_master_id = invoiceDataForDump.invoice_master_id,
                    invoice_no = invoiceDataForDump.invoice_no,
                    invoice_date = invoiceDataForDump.invoice_date,
                    party_id = invoiceDataForDump.party_id,
                    requisition_master_id = invoiceDataForDump.requisition_master_id,
                    item_total = invoiceDataForDump.item_total,
                    rebate_total = invoiceDataForDump.rebate_total,
                    invoice_total = invoiceDataForDump.invoice_total,
                    account_balance = invoiceDataForDump.account_balance,
                    remarks = invoiceDataForDump.remarks,
                    created_by = invoiceDataForDump.created_by,
                    discount_amount = invoiceDataForDump.discount_amount,
                    incentive_amount = invoiceDataForDump.incentive_amount,
                    incentive_percentage = invoiceDataForDump.incentive_percentage,
                    discount_percentage = invoiceDataForDump.discount_percentage,
                    pos_master_id = invoiceDataForDump.pos_master_id,
                    owner_party_id = invoiceDataForDump.owner_party_id,
                    received_amount = receivedAmount,
                    balance_amount = balanceAmount,
                    previous_invoice_advance = previousInvoiceAdvance,
                    previous_invoice_due = previousInvoiceDue,
                    price_protected_amount = priceProtectedAmount,
                    lifting_quantity = 0,
                    returned_qty = 0,
                    returned_amount = 0,
                    credit_limit = invoiceDataForDump.credit_limit,//invoiceModel.InvoiceMasterData.credit_limit,
                    party_type_id = invoiceDataForDump.party_type_id,//invoiceModel.InvoiceMasterData.party_type_id,

                    credit_term = invoiceDataForDump.credit_term,
                    contact_person_mobile = invoiceDataForDump.contact_person_mobile,
                    address = invoiceDataForDump.address
                };
                _entities.invoice_log.Add(invoiceLogInsert);
                _entities.SaveChanges();
            }

            //Check IsUpdate from Client Site SENDING EMAIL NOTIFICATION
            isRequisitionUpdated = true;
            // Send Email Nofication When Updated
            //Get Mail Data
            if (isRequisitionUpdated == true)
            {
                int counter = 0;
                var requisitionEmail = "";


                var dataSmtp = _entities.notifier_mail_account.FirstOrDefault(s => s.is_active == true);

                var dataReceiver = (from mrs in _entities.mail_receiver_setting
                                    join spm in _entities.software_process_module on mrs.process_code_id equals spm.process_code_id
                                    select new
                                    {
                                        mail_receiver_setting_id = mrs.mail_receiver_setting_id,
                                        process_code_name = spm.process_code_name,
                                        process_code_id = spm.process_code_id,
                                        receiver_name = mrs.receiver_name,
                                        receiver_email = mrs.receiver_email,
                                        is_active = mrs.is_active,
                                        is_deleted = mrs.is_deleted,
                                        created_by = mrs.created_by,
                                        created_date = mrs.created_date,
                                        updated_by = mrs.updated_by,
                                        updated_date = mrs.updated_date
                                    }).Where(c => c.is_deleted != true && c.process_code_name == "REQUISITION")
                    .OrderByDescending(c => c.mail_receiver_setting_id)
                    .ToList();

                var dataProcess = (from mrs in _entities.process_wise_mail_setting
                                   join spm in _entities.software_process_module on mrs.process_code_id equals spm.process_code_id
                                   select new
                                   {
                                       process_wise_mail_setting_id = mrs.process_wise_mail_setting_id,
                                       process_code_name = spm.process_code_name,
                                       process_code_id = spm.process_code_id,
                                       email_body = mrs.email_body,
                                       email_subject = mrs.email_subject,
                                       is_active = mrs.is_active,
                                       is_deleted = mrs.is_deleted,
                                       created_by = mrs.created_by,
                                       created_date = mrs.created_date,
                                       updated_by = mrs.updated_by,
                                       updated_date = mrs.updated_date
                                   }).FirstOrDefault(c => c.is_deleted != true && c.process_code_name == "REQUISITION");



                //GET REQUISITION INFO
                var requsitionInfo =
                    GetRequsitionInformatioForEmailNotificationById(RequisitionMaster.requisition_master_id ?? 0);
                //GET WHO IS UPDATING
                var approvedBy =
                    _entities.users.Where(d => d.user_id == RequisitionMaster.updated_by)
                        .Select(d => d.full_name)
                        .FirstOrDefault();

                //PROCESS MAIL OBJECT
                StringBuilder sb = new StringBuilder();
                sb.Append("<h4 style='color: red'>" + dataProcess.email_body + "</h4>");
                sb.Append("<table>");
                sb.Append("<tr>");
                sb.Append("<td><b>REQUISITION NO</b></td>");
                sb.Append("<td><b>: <span style='color: red'>" + requsitionInfo[0].requisition_code +
                          "</span></b></td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td><b>REQUISITION DATE</b></td>");
                sb.Append("<td><b>: " + requsitionInfo[0].requisition_date + "</b></td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td><b>REQUISITION TYPE</b></td>");
                sb.Append("<td><b>: " + requsitionInfo[0].requisition_type + "</b></td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td><b>CHANNEL NAME</b></td>");
                sb.Append("<td><b>: " + requsitionInfo[0].party_name + "</b></td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td><b>CHANNEL TYPE</b></td>");
                sb.Append("<td><b>: " + requsitionInfo[0].party_type + "</b></td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td><b>APPROVED BY By</b></td>");
                sb.Append("<td><b>: " + approvedBy + "</b></td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td><b>APPROVED DATE</b></td>");
                sb.Append("<td><b>: " + DateTime.Now + "</b></td>");
                sb.Append("</tr>");
                sb.Append("</table>");
                sb.Append("<br/>");
                sb.Append("<table border='1px' cellpadding='7'>");
                sb.Append("<tr>");
                sb.Append("<th>Product Category</th>");
                sb.Append("<th>Product Name</th>");
                sb.Append("<th>Color</th>");
                sb.Append("<th>Version</th>");
                sb.Append("<th>Requisition Qty</th>");
                sb.Append("<th>Discount Amount</th>");
                sb.Append("<th>Line Total</th>");

                sb.Append("</tr>");
                foreach (var item in requsitionInfo)
                {
                    sb.Append("<tr align='center'>");
                    sb.Append("<td>" + item.product_category_name + "</td>");
                    sb.Append("<td>" + item.product_name + "</td>");
                    sb.Append("<td>" + item.color_name + "</td>");
                    sb.Append("<td>" + item.product_version_name + "</td>");
                    sb.Append("<td>" + item.quantity + "</td>");
                    sb.Append("<td>" + item.discount_amount + "</td>");
                    sb.Append("<td>" + item.line_total + "</td>");
                    sb.Append("</tr>");
                }
                sb.Append("</table>");



                string requisitionNumberEmailBody = sb.ToString();

                foreach (var item in dataReceiver)
                {
                    if (counter == 0)
                    {
                        requisitionEmail = item.receiver_email;
                    }
                    requisitionEmail += "," + item.receiver_email;
                    counter++;
                }




                //Send Confirmation Mail
                mailRepository.SendMail(dataSmtp.account_email, requisitionEmail, dataProcess.email_subject,
                    requisitionNumberEmailBody, dataSmtp.account_email, dataSmtp.accoutn_password, "");


            }
            else
            {
                // nothing Happend
            }



            _entities.SaveChanges();

            return 1;
        }

        public int CreateInvoiceByShafiq(InvoiceModel invoiceModel)
        {
            var InvoiceMaster = invoiceModel.InvoiceMasterData;
            var InvoiceDetails = invoiceModel.InvoiceDetailsList;
            //var PriceProtection = invoiceModel.PriceProtectionList;

            var partyTypePrefix = (from ptype in _entities.party_type
                                   join par in _entities.parties on ptype.party_type_id equals par.party_type_id
                                   where par.party_id == InvoiceMaster.party_id
                                   select new
                                   {
                                       party_prefix = ptype.party_prefix

                                   }).FirstOrDefault();

            DateTime TodayDate = System.DateTime.Today;
            DateTime ReceivingDate = DateTime.Today.AddDays(-27);

            //calculating rebate total and invoice total
            //decimal rebateTotal =
            //    _entities.requisition_rebate.Where(
            //        rb => rb.requisition_master_id == invoiceModel.InvoiceMasterData.requisition_master_id).Sum(s => s.rebate_amount) ?? 0;

            //decimal InvoiceTotal = (invoiceModel.InvoiceMasterData.item_total - rebateTotal) ?? 0;
            decimal InvoiceTotal = (invoiceModel.InvoiceMasterData.item_total) ?? 0;

            //GET ACCOUNT BALANCE FROM PARTY JOURNAL
            var partyJournal =
                _entities.party_journal.Where(pj => pj.party_id == invoiceModel.InvoiceMasterData.party_id)
                    .OrderByDescending(p => p.party_journal_id)
                    .FirstOrDefault();
            decimal partyAccountBalance = 0;
            if (partyJournal != null)
            {
                partyAccountBalance = partyJournal.closing_balance ?? 0;
            }



            //invoice master
            int invoiceserial = _entities.invoice_master.Max(inv => (int?)inv.invoice_master_id) ?? 0;

            if (invoiceserial != 0)
            {
                invoiceserial++;
            }
            else
            {
                invoiceserial++;
            }
            var invstr = invoiceserial.ToString().PadLeft(7, '0');
            string invoiceno = "INV-" + partyTypePrefix.party_prefix + "-" + invstr;

            InvoiceMaster.invoice_no = invoiceno;
            InvoiceMaster.invoice_date = DateTime.Now;
            InvoiceMaster.party_id = invoiceModel.InvoiceMasterData.party_id;
            InvoiceMaster.requisition_master_id = invoiceModel.InvoiceMasterData.requisition_master_id;
            InvoiceMaster.item_total = invoiceModel.InvoiceMasterData.item_total;
            InvoiceMaster.company_id = invoiceModel.InvoiceMasterData.company_id;
            InvoiceMaster.invoice_total = InvoiceTotal;
            InvoiceMaster.account_balance = partyAccountBalance;
            InvoiceMaster.remarks = invoiceModel.InvoiceMasterData.remarks;
            InvoiceMaster.created_by = invoiceModel.InvoiceMasterData.created_by;
            InvoiceMaster.credit_limit = invoiceModel.InvoiceMasterData.credit_limit;
            InvoiceMaster.party_type_id = invoiceModel.InvoiceMasterData.party_type_id;

            InvoiceMaster.credit_term = invoiceModel.InvoiceMasterData.credit_term;
            InvoiceMaster.contact_person_mobile = invoiceModel.InvoiceMasterData.contact_person_mobile;
            InvoiceMaster.address = invoiceModel.InvoiceMasterData.address;
            InvoiceMaster.reference_no = invoiceModel.InvoiceMasterData.reference_no;
            InvoiceMaster.price_type = invoiceModel.InvoiceMasterData.price_type;

            //InvoiceMaster.discount_amount = invoiceModel.InvoiceMasterData.discount_amount;
            //InvoiceMaster.discount_percentage = invoiceModel.InvoiceMasterData.discount_percentage;
            //InvoiceMaster.incentive_amount = invoiceModel.InvoiceMasterData.incentive_amount;
            //InvoiceMaster.incentive_percentage = invoiceModel.InvoiceMasterData.incentive_percentage;

            _entities.invoice_master.Add(InvoiceMaster);
            _entities.SaveChanges();
            long invoicemasterid = InvoiceMaster.invoice_master_id;

            //get party type(MD/DIBS)
            // var partytypeId = (from rm in _entities.requisition_master
            //                    join p in _entities.parties on rm.party_id equals p.party_id
            //                    join pt in _entities.party_type on p.party_type_id equals pt.party_type_id
            //                    where (rm.party_id == invoiceModel.InvoiceMasterData.party_id)
            //                    select new
            //                    {
            //                        party_prefix = pt.party_prefix
            //                    }).ToList().FirstOrDefault();
            //var ptTypeName = partytypeId.party_prefix;

            //invoice details table
            foreach (var item in InvoiceDetails)
            {
                var invoiceDetails = new invoice_details
                {
                    invoice_master_id = invoicemasterid,
                    product_category_id = item.product_category_id,
                    product_id = item.product_id,
                    quantity = item.quantity,
                    price = item.price,
                    unit_id = item.unit_id,
                    color_id = item.color_id,
                    line_total = item.line_total,
                    brand_id = item.brand_id,
                    is_gift = item.is_gift,
                    gift_type = item.gift_type,
                    is_live_dummy = item.is_live_dummy,
                    product_version_id = item.product_version_id,
                    promotion_master_id = item.promotion_master_id,
                    discount_amount = item.discount_amount

                };
                _entities.invoice_details.Add(invoiceDetails);
                _entities.SaveChanges();
            }

            //update invoice_master table invoice_total column(19.11.2016)
            decimal incentive_amount = 0;
            incentive_amount = invoiceModel.InvoiceMasterData.incentive_amount ?? 0;
            InvoiceTotal = invoiceModel.InvoiceMasterData.item_total ?? 0;//(invoiceModel.InvoiceMasterData.item_total - (rebateTotal + incentive_amount + priceProtectionAmount)) ?? 0;
            invoice_master invmaster = _entities.invoice_master.Find(invoicemasterid);
            invmaster.invoice_total = InvoiceTotal;

            //update requisition master table status column
            requisition_master reqMaster = _entities.requisition_master.Find(invoiceModel.InvoiceMasterData.requisition_master_id);
            reqMaster.is_invoice_created = true;

            reqMaster.finance_status = "Approved";//for aamra it is sales head approval
            reqMaster.sales_head_recommendation = invoiceModel.InvoiceMasterData.sales_head_recommendation;
            reqMaster.forward_2_status = "Fwd to HOS";//forward_2_status means HOOps forward status
            reqMaster.operation_remarks = InvoiceMaster.operation_remarks;
            reqMaster.forward_2_by = InvoiceMaster.updated_by;
            reqMaster.forward_2_date = DateTime.Now;
            _entities.SaveChanges();

            //insert data into party journal table
            partyJournalRepository.PartyJournalEntry("INVOICE", invoiceModel.InvoiceMasterData.party_id ?? 0, InvoiceTotal, "Invoice", invoiceModel.InvoiceMasterData.created_by ?? 0, invoiceno);

            //update receive table field received_invoice_no
            var receiveList = _entities.receives.Where(r => r.party_id == invoiceModel.InvoiceMasterData.party_id && (r.received_invoice_no == null || r.received_invoice_no == "")).ToList();
            foreach (var r in receiveList)
            {
                r.received_invoice_no = invoiceno;
                _entities.SaveChanges();
            }

            //insert invoice_log table
            decimal receivedAmount = 0;
            decimal balanceAmount = 0;
            decimal previousInvoiceAdvance = 0;
            decimal previousInvoiceDue = 0;
            decimal priceProtectedAmount = 0;

            receivedAmount = _entities.receives.Where(w => w.received_invoice_no == invoiceno).Sum(s => s.amount) ?? 0;
            balanceAmount = InvoiceTotal - receivedAmount;

            var previousinvoicebalance = _entities.receives.Where(w => w.received_invoice_no == invoiceno).OrderByDescending(o => o.receive_id).FirstOrDefault();
            if (previousinvoicebalance != null)
            {
                decimal previousInvoiceBalance = 0;

                previousInvoiceBalance = Convert.ToDecimal(previousinvoicebalance.last_invoice_balance);
                if (previousInvoiceBalance < 0)
                {
                    previousInvoiceDue = previousInvoiceBalance;
                    previousInvoiceAdvance = 0;
                }
                else
                {
                    previousInvoiceDue = 0;
                    previousInvoiceAdvance = previousInvoiceBalance;
                }
            }

            //priceProtectedAmount = _entities.price_protection.Where(w => w.invoice_master_id == invoicemasterid).Sum(s => s.rebate) ?? 0;

            invoice_master invoiceDataForDump = _entities.invoice_master.Where(w => w.invoice_master_id == invoicemasterid).FirstOrDefault();
            if (invoiceDataForDump != null)
            {
                invoice_log invoiceLogInsert = new invoice_log
                {
                    invoice_master_id = invoiceDataForDump.invoice_master_id,
                    invoice_no = invoiceDataForDump.invoice_no,
                    invoice_date = invoiceDataForDump.invoice_date,
                    party_id = invoiceDataForDump.party_id,
                    requisition_master_id = invoiceDataForDump.requisition_master_id,
                    item_total = invoiceDataForDump.item_total,
                    rebate_total = invoiceDataForDump.rebate_total,
                    invoice_total = invoiceDataForDump.invoice_total,
                    account_balance = invoiceDataForDump.account_balance,
                    remarks = invoiceDataForDump.remarks,
                    created_by = invoiceDataForDump.created_by,
                    discount_amount = invoiceDataForDump.discount_amount,
                    incentive_amount = invoiceDataForDump.incentive_amount,
                    incentive_percentage = invoiceDataForDump.incentive_percentage,
                    discount_percentage = invoiceDataForDump.discount_percentage,
                    pos_master_id = invoiceDataForDump.pos_master_id,
                    owner_party_id = invoiceDataForDump.owner_party_id,
                    received_amount = receivedAmount,
                    balance_amount = balanceAmount,
                    previous_invoice_advance = previousInvoiceAdvance,
                    previous_invoice_due = previousInvoiceDue,
                    price_protected_amount = priceProtectedAmount,
                    lifting_quantity = 0,
                    returned_qty = 0,
                    returned_amount = 0,
                    credit_limit = invoiceDataForDump.credit_limit,//invoiceModel.InvoiceMasterData.credit_limit,
                    party_type_id = invoiceDataForDump.party_type_id,//invoiceModel.InvoiceMasterData.party_type_id,

                    credit_term = invoiceDataForDump.credit_term,
                    contact_person_mobile = invoiceDataForDump.contact_person_mobile,
                    address = invoiceDataForDump.address
                };
                _entities.invoice_log.Add(invoiceLogInsert);
                _entities.SaveChanges();
            }


            return 1;
        }

        //For invoice List
        public object GetAllInvoice()
        {
            try
            {
                var invoice = (from inv in _entities.invoice_master
                               join p in _entities.parties on inv.party_id equals p.party_id
                               join pt in _entities.party_type on p.party_type_id equals pt.party_type_id
                               select new
                               {
                                   invoice_master_id = inv.invoice_master_id,
                                   invoice_no = inv.invoice_no,
                                   invoice_date = inv.invoice_date,
                                   invoice_total = inv.invoice_total,
                                   party_name = p.party_name,
                                   party_code = p.party_code,
                                   party_type_name = pt.party_type_name,
                                   party_type_id = inv.party_type_id,
                                   party_prefix = pt.party_prefix

                               }).OrderByDescending(invc => invc.invoice_master_id).Where(w => w.party_type_id != null && w.party_prefix == "DEL" || w.party_prefix == "B2B").ToList();

                return invoice;

                //linq left join
                //var invoice = (from inv in _entities.invoice_master
                //               join p in _entities.parties on inv.party_id equals p.party_id into join_inv_p
                //               from j in join_inv_p.DefaultIfEmpty()
                //               join pt in _entities.party_type on j.party_type_id equals pt.party_type_id

                //               select new
                //               {
                //                   invoice_master_id = inv.invoice_master_id,
                //                   invoice_no = inv.invoice_no,
                //                   invoice_date = inv.invoice_date,
                //                   invoice_total = inv.invoice_total,
                //                   party_name = j.party_name,
                //                   party_code = j.party_code,
                //                   party_type_name = pt.party_type_name

                //               }).OrderByDescending(invc => invc.invoice_master_id).ToList();

                //return invoice;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public static DataTable ToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }
        //For invoice Report
        public object GetInvoiceReportById(int invoice_master_id)
        {
            try
            {
                string invoice_No = string.Empty;
                int partyid = 0;
                var this_party_id = _entities.invoice_master.Where(w => w.invoice_master_id == invoice_master_id).OrderBy(o => o.invoice_master_id).FirstOrDefault();
                partyid = (int)this_party_id.party_id;

                var invoiceNo = _entities.invoice_master.Where(w => w.invoice_master_id == invoice_master_id).FirstOrDefault();

                if (invoiceNo != null)
                {
                    invoice_No = invoiceNo.invoice_no;
                }


                //check first invoice of this party or not
                //int invoice_masterId = 0;
                var checkFirstInvoice = _entities.invoice_master.Where(w => w.party_id == partyid).OrderBy(o => o.invoice_master_id).FirstOrDefault();
                long invoice_masterId = checkFirstInvoice.invoice_master_id;
                int first_invoice = 0;
                if (invoice_masterId == invoice_master_id)
                {
                    first_invoice = 1;
                }
                else
                {
                    first_invoice = 2;
                }
                //////////////first invoice checking end//////////////

                //previous balance getting
                decimal previousBalance = 0;
                var firstOrDefault = _entities.invoice_master.FirstOrDefault(w => w.invoice_master_id == invoice_master_id);


                //19.12.2016
                //if any party have TransactionType=INITIAL_BALANCE and this is the first invoice then previousBalance comes from that row 
                var initial_Balance = "";
                var tranType = "";
                var intialBalance = _entities.party_journal.FirstOrDefault(w => w.transaction_type == "INITIAL_BALANCE" && w.party_id == partyid);
                if (intialBalance != null)
                {
                    initial_Balance = intialBalance.transaction_type;
                    tranType = intialBalance.transaction_type;
                }
                var transactionType = "";

                if (initial_Balance == "INITIAL_BALANCE" && first_invoice == 1)
                {
                    previousBalance = Convert.ToDecimal(intialBalance.closing_balance);
                }

                else
                {
                    if (firstOrDefault != null)
                    {
                        var documentCode = firstOrDefault.invoice_no;
                        var partyJournalCurrentId = _entities.party_journal.FirstOrDefault(w => w.document_code == documentCode).party_journal_id;

                        var previousRows =
                            _entities.party_journal.Where(w => w.party_id == partyid && w.transaction_type == "INVOICE" && w.party_journal_id < partyJournalCurrentId).OrderByDescending(o => o.party_journal_id).FirstOrDefault();

                        if (previousRows != null)
                        {
                            //previousBalance = -Convert.ToDecimal(previousRows.closing_balance);
                            previousBalance = Convert.ToDecimal(previousRows.closing_balance);
                        }
                        else
                        {
                            previousBalance = 0;
                        }
                    }
                }


                //getting good returned amount between current invoice and previous invoice(28.11.2016)
                if (first_invoice != 1)//when first invoice created there is no return
                {
                    var currentInvoice = _entities.invoice_master.FirstOrDefault(w => w.invoice_master_id == invoice_master_id);
                    var currentInvoiceNo = currentInvoice.invoice_no;
                    var ThisPartyJournalId = _entities.party_journal.FirstOrDefault(w => w.document_code == currentInvoiceNo).party_journal_id;
                    var PreviousPartyJournalId = _entities.party_journal.Where(w => w.party_id == partyid && w.transaction_type == "INVOICE" && w.party_journal_id < ThisPartyJournalId).OrderByDescending(o => o.party_journal_id).FirstOrDefault().party_journal_id;

                    var ReturnedAmount = _entities.party_journal.Where(w => w.party_journal_id > PreviousPartyJournalId && w.party_journal_id < ThisPartyJournalId && w.transaction_type == "RETURN" && w.party_id == partyid).Sum(s => s.cr_amount) ?? 0;
                    var ReturnedInvoceCode = _entities.party_journal.Where(w => w.party_journal_id >= PreviousPartyJournalId && w.party_journal_id < ThisPartyJournalId && w.transaction_type == "RETURN" && w.party_id == partyid).ToList();
                    if (ReturnedInvoceCode.Count > 1)
                    {
                        var returncodes = "";
                        foreach (var rcode in ReturnedInvoceCode)
                        {
                            returncodes = returncodes + rcode.document_code + ',';
                        }
                    }
                }


                DataSet ds = new DataSet();


                string query = " select im.invoice_no, im.invoice_date,company.company_name, p.product_name, p.product_code, id.quantity, id.price, "
                    //+" c.color_name, v.product_version_name, " 
                                + " isnull(c.color_name,'') as color_name, isnull(v.product_version_name,'') as product_version_name, "
                                + " pt.party_name, pt.party_code, pt.address, pt.mobile, "
                                + " pty.party_prefix, pty.party_type_name , (select full_name from users where user_id=im.created_by) as prepared_by, im.party_id, im.party_type_id, "
                                + " (select requisition_code from requisition_master where requisition_master_id=im.requisition_master_id) AS requisition_no, "
                                + " id.is_gift, id.gift_type, rg.region_name, "
                                + " -(select top 1 opening_balance from party_journal where party_id=im.party_id and document_code='" + invoice_No + "' order by party_journal_id desc) as previous_balance, "
                                + " -(select top 1 closing_balance from party_journal where party_id=im.party_id and document_code='" + invoice_No + "' order by party_journal_id desc) as closing_balance, "
                                + " (select isnull(sum(amount),0) from receive where party_id=im.party_id and received_invoice_no='" + invoice_No + "')  as receivedAmount, "
                                + " isnull(im.discount_amount,0) as discount_amount "
                                + " from invoice_master im "
                                + " inner join invoice_details id on im.invoice_master_id=id.invoice_master_id "
                                + " inner join product p on id.product_id=p.product_id "
                                + " left join color c on id.color_id=c.color_id "
                                + " left join product_version v on id.product_version_id=v.product_version_id "
                                + " inner join party pt on im.party_id=pt.party_id "
                                + " inner join party_type pty on pt.party_type_id=pty.party_type_id "
                                + " left join region rg on pt.region_id=rg.region_id "
                                + "left join company  on im.company_id=company.company_id "
                                + " where im.invoice_master_id = " + invoice_master_id + " ";


                var reData = _entities.Database.SqlQuery<InvoiceReportModel>(query).ToList();
                var list1 = new List<InvoiceReportModel>(reData);

                DataTable dt = new DataTable();
                dt = ToDataTable(list1);
                var invcNo = dt.Rows[0]["invoice_no"].ToString();
                var partyId = dt.Rows[0]["party_id"].ToString();
                //int invoiceMasterId = Convert.ToInt32(dt.Rows[0]["invoice_master_id"].ToString());


                //for receive breakdown subreport
                //string queryreceivebreakdown = " select payment_method.payment_method_name,  (CASE WHEN payment_method.payment_method_name='Cash' THEN 'Cash' when payment_method.payment_method_name='Cheque' then 'Cheque' else bank.bank_name END) as bank_name, receive.receive_date, receive.amount as receivedAmount, (CASE WHEN receive.cheque_no='' THEN 'DC:' ELSE 'DC:' || receive.cheque_no  END) as cheque_no from receive left join payment_method on receive.payment_method_id=payment_method.payment_method_id left join bank on receive.bank_id=bank.bank_id where received_invoice_no ='" + invcNo + "' and party_id='" + partyId + "'";
                string queryreceivebreakdown = " select payment_method.payment_method_name,  (CASE WHEN payment_method.payment_method_name='Cash' THEN 'Cash' when payment_method.payment_method_name='Cheque' then 'Cheque' else bank.bank_name END) as bank_name, receive.receive_date, receive.amount as receivedAmount, (CASE WHEN receive.cheque_no='' THEN 'DC:' ELSE 'DC:'  END) as cheque_no from receive left join payment_method on receive.payment_method_id=payment_method.payment_method_id left join bank on receive.bank_id=bank.bank_id where received_invoice_no ='" + invcNo + "' and party_id='" + partyId + "'";
                var receivebreakdowndata = _entities.Database.SqlQuery<ReceivedBreakdownModel>(queryreceivebreakdown);
                var list3 = new List<ReceivedBreakdownModel>(receivebreakdowndata);

                var invoiceCombined = new InvoiceCombinedModelcs();
                invoiceCombined.InvoiceReportModels = list1;
                invoiceCombined.ReceivedBreakdownModels = list3;

                return invoiceCombined;

            }
            catch (Exception)
            {

                throw;
            }

        }



        //Daily Sales Report
        //public object GetDailySalesReport(int party_id, string from_dt, string to_dt)
        //{
        //    try
        //    {
        //        if (party_id != null && from_dt != null && to_dt != null)
        //        {
        //            //string query = " select party_journal.party_journal_id, party_journal.transaction_date, party_journal.transaction_type, party_journal.party_id, party_journal.opening_balance, "
        //            //               + " party_journal.dr_amount, party_journal.cr_amount, party_journal.closing_balance, party_journal.remarks, party_journal.created_by, party_journal.created_date, "
        //            //               + " party_journal.updated_by, party_journal.updated_date, party.party_name, users.full_name, party.address,party.proprietor_name,party.phone, party.mobile, party.email, location.location_name, "
        //            //               + " party_journal.document_code, payment_method.payment_method_name "
        //            //               + " from party_journal "
        //            //               + " inner join party on party_journal.party_id=party.party_id inner join users on party_journal.created_by=users.user_id inner join party_type on party.party_type_id=party_type.party_type_id "
        //            //               + " left join location on party.location_id=location.location_id "
        //            //               + " left join receive on party_journal.document_code=receive.receipt_no "
        //            //               + " left join payment_method on receive.payment_method_id=payment_method.payment_method_id "
        //            //               + " where to_date(party_journal.transaction_date,'DD/MM/YYYY') between to_date('" + from_dt + "', 'DD/MM/YYYY') and to_date('" + to_dt + "', 'DD/MM/YYYY') and party_journal.party_id = " + party_id + " order by party_journal.transaction_date asc ";


        //            string query = " select party.party_name as CustomerName, party_type.party_prefix as CustType, party.party_code as CustID, zone.zone_name, area.area_name, invoice_log.invoice_no, "
        //                           +
        //                           " to_date(invoice_log.invoice_date,'DD/MM/YYYY') as InvoiceDate, product.product_name, color.color_name, invoice_details.quantity as Qty, invoice_details.price, "
        //                           + " invoice_log.item_total as BillingAmt,  "
        //                           +
        //                           " (case when product.product_name = requisition_rebate.rebate_for then requisition_rebate.rebate_amount else 0 end) as Rebate, "
        //                           +
        //                           " (case when invoice_details.is_live_dummy=true then invoice_details.quantity else 0 end) as LiveDummy, "
        //                           +
        //                           " (case when price_protection.product_id=invoice_details.product_id then price_protection.rebate else 0 end) as Adjust, "
        //                           +
        //                           " (select sum(invoice_details.line_total) from invoice_details where invoice_details.product_id=product.product_id and invoice_details.is_live_dummy=false and invoice_details.invoice_master_id = invoice_log.invoice_master_id) as NetAmount, "
        //                           +
        //                           " (select sum(invoice_details.line_total) from invoice_details where invoice_details.is_live_dummy=false and invoice_details.invoice_master_id=invoice_log.invoice_master_id) as GrandTotal, "
        //                           +
        //                           " bank.bank_name, payment_request.deposite_date, payment_request.cheque_no as DcorChqNo, receive.amount as DepositAmt, "
        //                           +
        //                           " invoice_log.received_amount as TotalReceived, invoice_log.balance_amount as BalanceAmt, "
        //                           +
        //                           " (case when invoice_log.balance_amount < 0 then 'Due' when invoice_log.balance_amount > 0 then 'Advance' else 'Fully Received' end) as BalanceStatus, "
        //                           +
        //                           " (select sum(return_details.returned_qty) from return_details where return_details.invoice_master_id=invoice_log.invoice_master_id and return_details.product_id = invoice_details.product_id) as ReturnQty, "
        //                           +
        //                           " (select return_master.return_date from return_details inner join return_master on return_details.return_master_id=return_master.return_master_id where return_details.invoice_master_id=invoice_log.invoice_master_id and return_details.product_id = invoice_details.product_id) as ReturnDate, "
        //                           +
        //                           " ((select sum(return_details.returned_qty) from return_details where return_details.invoice_master_id=invoice_log.invoice_master_id and return_details.product_id = invoice_details.product_id)*(select return_details.price from return_details where return_details.invoice_master_id=invoice_log.invoice_master_id and return_details.product_id = invoice_details.product_id)) as ReturnValue, "
        //                           +
        //                           " invoice_log.returned_amount as ReturnTotal, (invoice_log.balance_amount-invoice_log.returned_amount) as BalanceAfterReturn "
        //                           + " from invoice_log "
        //                           +
        //                           " inner join invoice_details on invoice_log.invoice_master_id=invoice_details.invoice_master_id "
        //                           +
        //                           " left join requisition_rebate on invoice_log.requisition_master_id = requisition_rebate.requisition_master_id "
        //                           +
        //                           " left join price_protection on invoice_details.invoice_master_id = price_protection.invoice_master_id "
        //                           + " inner join party on invoice_log.party_id=party.party_id "
        //                           + " inner join party_type on party.party_type_id=party_type.party_type_id "
        //                           + " inner join product on invoice_details.product_id=product.product_id "
        //                           + " inner join color on invoice_details.color_id=color.color_id "
        //                           + " left join zone on party.zone_id=zone.zone_id "
        //                           + " left join area on party.area_id=area.area_id "
        //                           + " eive.received_invoice_no "
        //                           + " left join bank on receive.bank_id=bank.bank_id "
        //                           +
        //                           " left join payment_request on receive.payment_method_id=payment_request.payment_method_id "
        //                            + " where invoice_log.party_id=" + party_id + " and invoice_details.is_live_dummy=false ";
        //                           //+ " where invoice_log.invoice_master_id=1 and invoice_details.is_live_dummy=false ";

        //            var reData = _entities.Database.SqlQuery<PartyJournalReportModel>(query).ToList();
        //            return reData;
        //        }

        //        return null;
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }

        //}


        public List<RequisitionUpdateEmailNotificationModel> GetRequsitionInformatioForEmailNotificationById(long masterId)
        {

            string query = "select rm.requisition_code ,rm.requisition_date ,rm.requisition_type ,rm.requisition_master_id ,pro.product_name ,col.color_name ,pv.product_version_name ,pc.product_category_name ,p.party_name ,p.address ,pt.party_type_name as party_type,rd.quantity ,rd.discount_amount ,rd.line_total ,us.full_name as approved_by ,rm.approved_date from requisition_master rm left join requisition_details rd on rm.requisition_master_id=rd.requisition_master_id left join product pro on rd.product_id=pro.product_id left join color col on rd.color_id = col.color_id left join product_version pv on rd.product_version_id=pv.product_version_id left join product_category pc on pro.product_category_id = pc.product_category_id left join party p on rm.party_id =p.party_id left join party_type pt on p.party_type_id =pt.party_type_id left join users us on rm.approved_by =us.user_id where rm.requisition_master_id='" + masterId + "'";
            var requsitionUpdateEmailData = _entities.Database.SqlQuery<RequisitionUpdateEmailNotificationModel>(query).ToList();

            return requsitionUpdateEmailData;
        }
    }
}