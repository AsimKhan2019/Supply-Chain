﻿using DMSApi.Models.IRepository;
using DMSApi.Models.StronglyType;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace DMSApi.Models.Repository
{
    public class PaymentRequestRepository : IPaymentRequestRepository
    {
        private DMSEntities _entities;

        public PaymentRequestRepository()
        {
            this._entities = new DMSEntities();
        }

        public object GetAllPaymentRequest(long party_id)
        {
            var payReq = (from p in _entities.payment_request
                          join par in _entities.parties
                         on p.party_id equals par.party_id
                          join pt in _entities.party_type
                          on par.party_type_id equals pt.party_type_id
                          join pm in _entities.payment_method
                          on p.payment_method_id equals pm.payment_method_id
                          join rcv in _entities.receives on p.payment_req_id equals rcv.payment_req_id into tempRcv
                          from rcv in tempRcv.DefaultIfEmpty()
                          join pty in _entities.parties on p.party_id equals pty.party_id into tempPty
                          from pty in tempPty.DefaultIfEmpty()
                          join teri in _entities.territories on pty.territory_id equals teri.territory_id into temTeri
                          from teri in temTeri.DefaultIfEmpty()
                          join app in _entities.users on rcv.approved_by equals app.user_id into tempApp
                          from app in tempApp.DefaultIfEmpty()
                          join ba in _entities.banks
                              on p.bank_id equals ba.bank_id into tempBank
                          from ba in tempBank.DefaultIfEmpty()
                          join bb in _entities.bank_branch on p.bank_branch_id equals bb.bank_branch_id into tempBb
                          from bb in tempBb.DefaultIfEmpty()
                          join acc in _entities.bank_account on p.bank_account_id equals acc.bank_account_id into tempAcc
                          from acc in tempAcc.DefaultIfEmpty()

                          where p.party_id == party_id

                          select new
                          {
                              payment_req_id = p.payment_req_id,
                              receipt_no = rcv.receipt_no,// new
                              bank_name = ba.bank_name,
                              bank_branch_name = bb.bank_branch_name,
                              amount = p.amount,
                              deposite_date = p.deposite_date,
                              payment_method_id = pm.payment_method_id,
                              payment_method_name = pm.payment_method_name,
                              cheque_no = p.cheque_no,
                              approved = p.approved,
                              document_attachment = p.document_attachment,
                              opening_balance = (from pj in _entities.party_journal
                                                 where pj.party_id == p.party_id
                                                 orderby pj.party_journal_id descending
                                                 select new { pj.closing_balance }).FirstOrDefault().closing_balance ?? 0,//new
                              party_id = par.party_id,
                              party_name = par.party_name,
                              bank_branch_id = p.bank_branch_id,
                              party_type_id = pt.party_type_id,
                              party_type_name = pt.party_type_name, // new
                              territory_name = teri.territory_name,//new
                              status = rcv.status,
                              approved_by = app.full_name ?? "Pending",
                              sales_representative = p.sales_representative

                          }).OrderByDescending(b => b.payment_req_id).ToList();

            return payReq;
        }

        public object GetAllPaymentRequestRawSql(long party_id)
        {
            string query = "select pr.payment_req_id, pr.amount,"
                //+" STUFF(STUFF(STUFF(pr.deposite_date,13,0,':'),11,0,':'),9,0,' ') as deposite_date, " 
                                + " FORMAT(pr.deposite_date , 'dd/MM/yyyy HH:mm:ss tt') as deposite_date, "
                                + " pm.payment_method_id,pm.payment_method_name, "
                                + " pr.cheque_no,pr.approved, pr.document_attachment, p.party_id, p.party_name, pt.party_type_id, "
                                + " pr.bank_branch_id, ba.bank_account_name, pt.party_type_name, t.territory_name,isnull(rcv.status,'Not Approved') as status, "
                                + " b.bank_name, bb.bank_branch_name, isnull(u.full_name,'Pending') as approved_by, "
                                + " (select top 1 CAST(isnull(party_journal.closing_balance,0) as decimal)  from party_journal where party_journal.party_id=p.party_id order by party_journal.party_journal_id desc) as opening_balance, "
                                + " isnull(rcv.receipt_no,'Processing') as receipt_no "
                                + " from payment_request pr "
                                + " left join bank b on pr.bank_id=b.bank_id "
                                + " left join bank_branch bb on pr.bank_branch_id=bb.bank_branch_id "
                                + " left join bank_account ba on pr.bank_account_id= ba.bank_account_id "
                                + " left join party p on  pr.party_id=p.party_id "
                                + " left join party_type pt on p.party_type_id=pt.party_type_id "
                                + " left join payment_method pm on pr.payment_method_id=pm.payment_method_id "
                                + " left join receive rcv on pr.payment_req_id=rcv.payment_req_id "
                                + " left join party p1 on pr.party_id=p1.party_id  "
                                + " left join territory t on p1.territory_id=t.territory_id "
                                + " left join users u on rcv.approved_by=u.user_id "
                                + " where pr.is_deleted=0 and pr.party_id = " + party_id + " "
                                + " order by pr.payment_req_id desc";

            var reData = _entities.Database.SqlQuery<AllPaymentRequest>(query).ToList();
            //var list1 = new List<AllPaymentRequest>(reData);

            return reData;
        }

        public object GetAllPaymentRequest()
        {
            var Payreq = (from p in _entities.payment_request
                          join ba in _entities.banks
                           on p.bank_id equals ba.bank_id into tempBank
                          join bb in _entities.bank_branch on p.bank_branch_id equals bb.bank_branch_id into tempBb
                          from bb in tempBb.DefaultIfEmpty()
                          join acc in _entities.bank_account on p.bank_account_id equals acc.bank_account_id into tempAcc
                          from acc in tempAcc.DefaultIfEmpty()
                          from ba in tempBank.DefaultIfEmpty()
                          join par in _entities.parties
                          on p.party_id equals par.party_id into tempPar
                          from par in tempPar.DefaultIfEmpty()
                          join pt in _entities.party_type
                          on par.party_type_id equals pt.party_type_id into TempPt
                          from pt in TempPt.DefaultIfEmpty()
                          join pm in _entities.payment_method
                          on p.payment_method_id equals pm.payment_method_id into TempPm
                          from pm in TempPm.DefaultIfEmpty()
                          join rcv in _entities.receives on p.payment_req_id equals rcv.payment_req_id into tempRcv
                          from rcv in tempRcv.DefaultIfEmpty()
                          join pty in _entities.parties on p.party_id equals pty.party_id into tempPty
                          from pty in tempPty.DefaultIfEmpty()
                          join teri in _entities.territories on pty.territory_id equals teri.territory_id into temTeri
                          from teri in temTeri.DefaultIfEmpty()
                          join app in _entities.users on rcv.approved_by equals app.user_id into tempApp
                          from app in tempApp.DefaultIfEmpty()
                          where p.is_deleted == false
                          select new
                          {
                              payment_req_id = p.payment_req_id,
                              amount = p.amount,
                              deposite_date = p.deposite_date,
                              payment_method_id = pm.payment_method_id,
                              payment_method_name = pm.payment_method_name,
                              cheque_no = p.cheque_no,
                              approved = p.approved,
                              document_attachment = p.document_attachment,
                              party_id = par.party_id,
                              party_name = par.party_name,
                              party_type_id = pt.party_type_id,
                              bank_branch_id = p.bank_branch_id,
                              bank_account_name = acc.bank_account_name,
                              party_type_name = pt.party_type_name,
                              territory_name = teri.territory_name,//new
                              status = rcv.status ?? "Not Approved",
                              bank_name = ba.bank_name,
                              bank_branch_name = bb.bank_branch_name,//new
                              approved_by = app.full_name ?? "Pending",//new
                              opening_balance = (from pj in _entities.party_journal
                                                 where pj.party_id == p.party_id
                                                 orderby pj.party_journal_id descending
                                                 select new { pj.closing_balance }).FirstOrDefault().closing_balance ?? 0,//new
                              receipt_no = rcv.receipt_no ?? "Processing"// new


                          }).OrderByDescending(b => b.payment_req_id).ToList();

            return Payreq;
        }

        public object GetAllPaymentRequestRawSql()
        {
           
                string query = "select pr.payment_req_id, pr.amount," 
                                //+" STUFF(STUFF(STUFF(pr.deposite_date,13,0,':'),11,0,':'),9,0,' ') as deposite_date, " 
                                + " FORMAT(pr.deposite_date , 'dd/MM/yyyy HH:mm:ss tt') as deposite_date, "
                                +" pm.payment_method_id,pm.payment_method_name, "
                                + " pr.cheque_no,pr.approved, pr.document_attachment, p.party_id, p.party_name, pt.party_type_id, "
                                + " pr.bank_branch_id, ba.bank_account_name, pt.party_type_name, t.territory_name,isnull(rcv.status,'Not Approved') as status, "
                                + " b.bank_name, bb.bank_branch_name, isnull(u.full_name,'Pending') as approved_by, "
                                + " (select top 1 CAST(isnull(party_journal.closing_balance,0) as decimal)  from party_journal where party_journal.party_id=p.party_id order by party_journal.party_journal_id desc) as opening_balance, "
                                + " isnull(rcv.receipt_no,'Processing') as receipt_no "
                                + " from payment_request pr "
                                + " left join bank b on pr.bank_id=b.bank_id "
                                + " left join bank_branch bb on pr.bank_branch_id=bb.bank_branch_id "
                                + " left join bank_account ba on pr.bank_account_id= ba.bank_account_id "
                                + " left join party p on  pr.party_id=p.party_id "
                                + " left join party_type pt on p.party_type_id=pt.party_type_id "
                                + " left join payment_method pm on pr.payment_method_id=pm.payment_method_id "
                                + " left join receive rcv on pr.payment_req_id=rcv.payment_req_id "
                                + " left join party p1 on pr.party_id=p1.party_id  "
                                + " left join territory t on p1.territory_id=t.territory_id "
                                + " left join users u on rcv.approved_by=u.user_id "
                                + " where pr.is_deleted=0  and pr.approved=0"
                                + " order by pr.payment_req_id desc";

                var reData = _entities.Database.SqlQuery<AllPaymentRequest>(query).ToList();
                //var list1 = new List<AllPaymentRequest>(reData);

                return reData;
           
        }

        public List<PaymentRequestModel> GetPaymentRequestByID(long payment_req_id)
        {
            List<PaymentRequestModel> payreq = (from p in _entities.payment_request
                                                join par in _entities.parties on p.party_id equals par.party_id
                                                join pt in _entities.party_type on par.party_type_id equals pt.party_type_id
                                                join pm in _entities.payment_method
                                                    on p.payment_method_id equals pm.payment_method_id
                                                where p.payment_req_id == payment_req_id
                                                select new PaymentRequestModel
                               {
                                   payment_req_id = p.payment_req_id,
                                   party_id = p.party_id,
                                   bank_id = p.bank_id,
                                   bank_branch_id = p.bank_branch_id,
                                   bank_account_id = p.bank_account_id,
                                   cheque_no = p.cheque_no,
                                   document_attachment = p.document_attachment,
                                   created_by = p.created_by,
                                   created_date = p.created_date,
                                   updated_by = p.updated_by,
                                   updated_date = p.updated_date,
                                   approved = p.approved,
                                   deposite_date = p.deposite_date,
                                   remarks = p.remarks,
                                   amount = p.amount,
                                   party_type_id = pt.party_type_id,
                                   payment_method_id = p.payment_method_id,
                                   payment_method_name = pm.payment_method_name,
                                   sales_representative = p.sales_representative,
                               }).ToList();
            return payreq;
        }

        public int InsertPaymentRequest()
        {
            // bank_branch_id ACT as Advance payment
            try
            {
                HttpRequest rsk = HttpContext.Current.Request;

                if (rsk.Form["payment_method_id"].ToString() == "Bank")
                {
                    if (rsk.Form["bank_id"].ToString() == null || rsk.Form["bank_id"].ToString() == "")
                    {
                        return 11;
                    }
                    else if (rsk.Form["cheque_no"].ToString() == null || rsk.Form["cheque_no"].ToString() == "")
                    {
                        return 14;
                    }

                }


                else if (rsk.Form["payment_method_id"].ToString() == "Cash")
                {
                    if (rsk.Form["sales_representative"].ToString() == null || rsk.Form["sales_representative"].ToString() == "")
                    {
                        return 16;
                    }

                }
                //for null or empty check
                if (rsk.Form["amount"].ToString() == null || rsk.Form["amount"].ToString() == "")
                {
                    return 17;
                }
                if (rsk.Form["deposite_date"].ToString() == null || rsk.Form["deposite_date"].ToString() == "")
                {
                    return 13;
                }


               /** call the Http request which is send by Web Form **/

                else
                {
                    var postedFile = rsk.Files["UploadedImage"];
                    long partyId = Convert.ToInt32(rsk.Form["party_id"]);
                    string bankId = rsk.Form["bank_id"];
                    string bankBranchId = rsk.Form["bank_branch_id"];
                    string bankAccountId = rsk.Form["bank_account_id"];
                    string paymentType = rsk.Form["payment_type"];
                    string chequeNo = rsk.Form["cheque_no"];
                    string salesRepresentative = rsk.Form["sales_representative"];
                    string remarksVari = rsk.Form["remarks"];

                    // Pass data from variable
                    long bank_id = Convert.ToInt64(string.IsNullOrEmpty(bankId) ? "0" : bankId);
                    long bank_branch_id = Convert.ToInt64(string.IsNullOrEmpty(bankBranchId) ? "0" : bankBranchId);
                    long bank_account_id = Convert.ToInt64(string.IsNullOrEmpty(bankAccountId) ? "0" : bankAccountId);
                    string payment_type = (string.IsNullOrEmpty(paymentType) ? "-" : paymentType);
                    string cheque_no = (string.IsNullOrEmpty(chequeNo) ? "0" : chequeNo);
                    string sales_representative = (string.IsNullOrEmpty(salesRepresentative) ? "-" : salesRepresentative);
                    string remarks = (string.IsNullOrEmpty(remarksVari) ? "-" : remarksVari);
                    decimal amount = Decimal.Parse(rsk.Form["amount"].ToString());
                    long requisition_master_id = Convert.ToInt64(string.IsNullOrEmpty(rsk.Form["requisition_master_id"]) ? "0" : rsk.Form["requisition_master_id"]);
                    DateTime deposite_date = DateTime.Now;//DateTime.ParseExact(rsk.Form["deposite_date"].ToString(), "dd/MM/yyyy", null);
                    long payment_method_id = int.Parse(rsk.Form["payment_method_id"].ToString());
                    long created_by = int.Parse(rsk.Form["created_by"].ToString());

                   

                    /** get the File Informaiton from http context **/
                    string actualFileName = "";
                    long userId = created_by;
                 

                    if (payment_method_id == 1 || payment_method_id == 3)
                    {
                        string filePathForDb = "";

                        if (postedFile == null)
                        {
                            filePathForDb = "";
                        }
                        else
                        {
                            //Start

                            actualFileName = postedFile.FileName;

                            var clientDir = HttpContext.Current.Server.MapPath("~/App_Data/Payment_request/" + "Party_" + partyId);

                            bool exists = System.IO.Directory.Exists(clientDir);
                            if (!exists)
                                System.IO.Directory.CreateDirectory(clientDir);


                            //var fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data/Payment_request"), actualFileName);
                            var fileSavePath = Path.Combine(clientDir, actualFileName);


                            bool checkFileSave = false;
                            try
                            {
                                postedFile.SaveAs(fileSavePath);
                                checkFileSave = true;
                            }
                            catch
                            {
                                checkFileSave = false;
                            }

                            filePathForDb = "Party_" + partyId + "/" + actualFileName;

                            //End
                        }

                        //if (checkFileSave)
                        //{



                        //}



                        /** insert record to database **/
                        payment_request paymentreq = new payment_request
                        {
                            party_id = partyId,
                            bank_id = bank_id,
                            bank_branch_id = bank_branch_id,
                            bank_account_id = bank_account_id,
                            cheque_no = cheque_no,
                            amount = amount,
                            payment_type = payment_type,
                            requisition_master_id = requisition_master_id,
                            document_attachment = filePathForDb,
                            deposite_date = DateTime.Now,//deposite_date,
                            remarks = remarks,
                            payment_method_id = payment_method_id,
                            sales_representative = sales_representative,
                            approved = false,
                            is_active = true,
                            is_deleted = false,
                            created_by = created_by,
                            created_date = DateTime.Now

                        };
                        var paymentReq = _entities.payment_request.Add(paymentreq);
                        _entities.SaveChanges();
                        if (paymentReq != null)
                        {
                            return 2;//Upload Success
                        }
                        //else
                        {
                            return 3;//Upload Save Failed
                        }
                    }
                    //If Payment type == CASH
                    else if (payment_method_id == 2)
                    {
                        payment_request paymentreq = new payment_request
                        {
                            party_id = partyId,
                            bank_id = 0,
                            bank_branch_id = bank_branch_id,
                            bank_account_id = bank_account_id,
                            payment_type = payment_type,
                            cheque_no = "0",
                            amount = amount,
                            requisition_master_id = requisition_master_id,
                            document_attachment = "No Attachment",
                            deposite_date = deposite_date,
                            remarks = remarks,
                            payment_method_id = payment_method_id,
                            sales_representative = sales_representative,
                            approved = false,
                            is_active = true,
                            is_deleted = false,
                            created_by = created_by,
                            created_date = DateTime.Now

                        };
                        var paymentReq = _entities.payment_request.Add(paymentreq);
                        _entities.SaveChanges();
                        if (paymentReq != null)
                        {
                            return 5;
                        }

                    }
                    //else
                    {
                        return 6;//Upload Failed 
                    }
                }

            }

            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdatePaymentRequest()
        {
            // bank_branch_id ACT as Advance payment
            try
            {
                HttpRequest rsk = HttpContext.Current.Request;

                if (rsk.Form["payment_method_id"].ToString() == "Bank")
                {
                    if (rsk.Form["bank_id"].ToString() == null || rsk.Form["bank_id"].ToString() == "")
                    {
                        return 11;
                    }
                    else if (rsk.Form["bank_branch_id "].ToString() == null || rsk.Form["bank_branch_id "].ToString() == "")
                    {
                        return 12;
                    }
                    //else if (rsk.Form["cheque_no"].ToString() == null || rsk.Form["cheque_no"].ToString() == "")
                    //{
                    //    return 14;
                    //}

                }
                //if (rsk.Form["account_no"].ToString() == null || rsk.Form["account_no"].ToString() == "")
                //{
                //    return 30;
                //}

                else if (rsk.Form["payment_method_id"].ToString() == "Cash")
                {
                    if (rsk.Form["sales_representative"].ToString() == null || rsk.Form["sales_representative"].ToString() == "")
                    {
                        return 16;
                    }
                }
                //for null or empty check
                if (rsk.Form["amount"].ToString() == null || rsk.Form["amount"].ToString() == "")
                {
                    return 17;
                }



               /** call the Http request which is send by Web Form **/

                else
                {
                    long payment_req_id = int.Parse(rsk.Form["payment_req_id"].ToString());
                    long partyId = Convert.ToInt32(rsk.Form["party_id"]);
                    var postedFile = rsk.Files["UploadedImage"];
                    long bank_id = Convert.ToInt64(rsk.Form["bank_id"]);
                    long bank_account_id = Convert.ToInt64(rsk.Form["bank_account_id"]);
                    long bank_branch_id = Convert.ToInt64(rsk.Form["bank_branch_id"]);
                    string cheque_no = rsk.Form["cheque_no"].ToString();
                    decimal amount = Decimal.Parse(rsk.Form["amount"].ToString());
                    string remarks = rsk.Form["remarks"].ToString();
                    long payment_method_id = int.Parse(rsk.Form["payment_method_id"].ToString());
                    string sales_representative = rsk.Form["sales_representative"].ToString();
                    int updated_by = int.Parse(rsk.Form["updated_by"].ToString());


                    if (sales_representative == null || sales_representative == "") { sales_representative = "0"; }
                    /** get the File Informaiton from http context **/
                    string actualFileName = "";
                    if (postedFile == null && (rsk.Form["payment_method_id"].ToString() == "Bank" || rsk.Form["payment_method_id"].ToString() == "Cheque"))
                    {
                        return 1;//no file upload

                    }
                    ///////////////getting party id////////////////
                    int userId = updated_by;
                    //var xxx = _entities.users.Find(userId);
                    //int partyId = (int)xxx.party_id;
                    //////////////////////////////////////////////
                    if (postedFile == null && payment_method_id == 1)
                    {
                        payment_request paymentReq = _entities.payment_request.Find(payment_req_id);
                        paymentReq.party_id = partyId;
                        paymentReq.bank_id = 0;
                        paymentReq.bank_branch_id = bank_branch_id;
                        paymentReq.bank_account_id = bank_account_id;
                        paymentReq.cheque_no = cheque_no;
                        paymentReq.amount = amount;
                        paymentReq.document_attachment = "No File Uploaded";
                        paymentReq.remarks = remarks;
                        paymentReq.payment_method_id = payment_method_id;
                        paymentReq.sales_representative = "";
                        paymentReq.approved = false;
                        paymentReq.updated_by = updated_by;
                        paymentReq.updated_date = DateTime.Now;

                        var paymentReqest = _entities.SaveChanges();
                        if (paymentReqest != null)
                        {
                            return 1;
                        }

                    }
                    /** save the File to Server Path **/
                    else if (payment_method_id == 1 || payment_method_id == 3 && postedFile != null)
                    {
                        actualFileName = postedFile.FileName;

                        var clientDir = HttpContext.Current.Server.MapPath("~/App_Data/Payment_request/" + "Party_" + partyId);

                        bool exists = System.IO.Directory.Exists(clientDir);
                        if (!exists)
                            System.IO.Directory.CreateDirectory(clientDir);


                        //var fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data/Payment_request"), actualFileName);
                        var fileSavePath = Path.Combine(clientDir, actualFileName);


                        bool checkFileSave = false;
                        try
                        {
                            postedFile.SaveAs(fileSavePath);
                            checkFileSave = true;
                        }
                        catch
                        {
                            checkFileSave = false;
                        }
                        payment_request paymentReq = _entities.payment_request.Find(payment_req_id);
                        paymentReq.party_id = partyId;
                        paymentReq.bank_id = bank_id;
                        paymentReq.bank_branch_id = bank_branch_id;
                        paymentReq.bank_account_id = bank_account_id;
                        paymentReq.cheque_no = cheque_no;
                        paymentReq.amount = amount;
                        paymentReq.document_attachment = fileSavePath;
                        paymentReq.remarks = remarks;
                        paymentReq.payment_method_id = payment_method_id;
                        paymentReq.sales_representative = sales_representative;
                        paymentReq.approved = false;
                        paymentReq.updated_by = updated_by;
                        paymentReq.updated_date = DateTime.Now;
                        _entities.SaveChanges();
                        if (paymentReq != null)
                        {
                            return 1;
                        }
                    }

                    else if (payment_method_id == 1 || payment_method_id == 3 && postedFile == null)
                    {



                        payment_request paymentReq = _entities.payment_request.Find(payment_req_id);
                        paymentReq.party_id = partyId;
                        paymentReq.bank_id = bank_id;
                        paymentReq.bank_branch_id = bank_branch_id;
                        paymentReq.bank_account_id = bank_account_id;
                        paymentReq.cheque_no = cheque_no;
                        paymentReq.amount = amount;
                        paymentReq.document_attachment = "No File Uploaded";
                        paymentReq.remarks = remarks;
                        paymentReq.payment_method_id = payment_method_id;
                        paymentReq.sales_representative = sales_representative;
                        paymentReq.approved = false;
                        paymentReq.updated_by = updated_by;
                        paymentReq.updated_date = DateTime.Now;
                        _entities.SaveChanges();
                        if (paymentReq != null)
                        {
                            return 1;
                        }
                    }



                   //If Payment type == CASH
                    else if (payment_method_id == 2)
                    {

                        payment_request paymentReq = _entities.payment_request.Find(payment_req_id);
                        paymentReq.party_id = partyId;
                        paymentReq.bank_id = 0;
                        paymentReq.bank_branch_id = bank_branch_id;
                        paymentReq.bank_account_id = bank_account_id;
                        paymentReq.amount = amount;
                        paymentReq.cheque_no = cheque_no;
                        paymentReq.document_attachment = "No Attachment";
                        paymentReq.remarks = remarks;
                        paymentReq.payment_method_id = payment_method_id;
                        paymentReq.sales_representative = sales_representative;
                        paymentReq.approved = false;
                        paymentReq.updated_by = updated_by;
                        paymentReq.updated_date = DateTime.Now;
                        var paymentReqest =
                        _entities.SaveChanges();
                        if (paymentReqest != null)
                        {
                            return 1;
                        }

                    }
                    //else
                    {
                        return 6;//Upload Failed 
                    }
                }

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public bool DeletePaymentRequest(long payment_req_id, long? updated_by)
        {
            try
            {
                payment_request oPayment = _entities.payment_request.FirstOrDefault(p => p.payment_req_id == payment_req_id);
                oPayment.is_deleted = true;
                oPayment.updated_by = updated_by;
                oPayment.updated_date = DateTime.Now;
                _entities.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //For Approved Button
        public bool UpdateStatus(long? payment_req_id, long? user_id)
        {
            try
            {
                var paymentreq = _entities.payment_request.Find(payment_req_id);
                paymentreq.approved = true;
                paymentreq.updated_by = user_id;
                paymentreq.updated_date = DateTime.Now;
                int saved = _entities.SaveChanges();
                //if (saved > 0)
                //{
                //    partyJournal.PartyJournalEntry("RECEIVE", paymentreq.party_id ?? 0, paymentreq.amount ?? 0, "Payment Received", paymentreq.updated_by ?? 0);
                //}
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //To Show image
        public HttpResponseMessage GetImage(long payment_req_id)
        {
            // GET FILE NAME
            var document = GetPaymentRequestByID(payment_req_id);

            if (document[0].document_attachment == "")
            {
                return null;
            }

            else
            {
                var fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data/Payment_Request"), document[0].document_attachment);
                //string fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images"),"Koala.jpg");

                try
                {
                    HttpResponseMessage result = null;
                    var image = System.Drawing.Image.FromFile(fileSavePath);

                    if (ImageFormat.Jpeg.Equals(image.RawFormat))
                    {
                        // JPEG
                        using (var memoryStream = new MemoryStream())
                        {
                            image.Save(memoryStream, ImageFormat.Jpeg);
                            result = new HttpResponseMessage(HttpStatusCode.OK)
                           {
                               Content = new ByteArrayContent(memoryStream.ToArray())
                           };
                            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                            result.Content.Headers.ContentLength = memoryStream.Length;
                            image.Dispose();
                            return result;
                        }
                    }
                    else if (ImageFormat.Png.Equals(image.RawFormat))
                    {
                        // PNG
                        using (var memoryStream = new MemoryStream())
                        {
                            image.Save(memoryStream, ImageFormat.Png);
                            result = new HttpResponseMessage(HttpStatusCode.OK)
                           {
                               Content = new ByteArrayContent(memoryStream.ToArray())
                           };
                            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                            result.Content.Headers.ContentLength = memoryStream.Length;
                            image.Dispose();
                            return result;
                        }
                    }
                    else if (ImageFormat.Gif.Equals(image.RawFormat))
                    {
                        // GIF
                        using (var memoryStream = new MemoryStream())
                        {
                            image.Save(memoryStream, ImageFormat.Gif);
                            result = new HttpResponseMessage(HttpStatusCode.OK)
                           {
                               Content = new ByteArrayContent(memoryStream.ToArray())
                           };
                            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/gif");
                            result.Content.Headers.ContentLength = memoryStream.Length;
                            image.Dispose();
                            return result;
                        }

                    }
                    return result;
                }

                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        //To get party type name
        public object GetPartyTypeName(long party_id)
        {
            var partType = (from p in _entities.parties
                            join pt in _entities.party_type
                                on p.party_type_id equals pt.party_type_id

                            where p.party_id == party_id
                            select new
                            {
                                party_id = p.party_id,
                                party_name = p.party_name,
                                party_type_id = pt.party_type_id,
                                party_type_name = pt.party_type_name
                            });
            return partType;
        }


        public object GetPartyPaymentAndRequisitionInfo(long party_id)
        {
            try
            {
                if (party_id == 10)
                {

                    var data = (from w in _entities.warehouses
                                join p in _entities.parties on w.party_id equals p.party_id

                                join wa in _entities.warehouses on p.party_id equals wa.party_id
                                join dm in _entities.delivery_master on p.party_id equals dm.party_id
                                join rm in _entities.internal_requisition_master
                                   on dm.requisition_master_id equals rm.internal_requisition_master_id into temRm
                                from rm in temRm.DefaultIfEmpty()
                                where wa.party_id == party_id
                                select new
                                {
                                    credit_limit = p.credit_limit ?? 0,
                                    requisition_code = rm.internal_requisition_no ?? "First Requisition",
                                    amount = rm.total_amount ?? 0,
                                    requisition_master_id = rm.internal_requisition_master_id
                                }).OrderByDescending(rm => rm.requisition_master_id).FirstOrDefault();
                    return data;
                }
                else
                {
                    var data = (from p in _entities.parties
                                join rm in _entities.requisition_master
                                    on p.party_id equals rm.party_id into temRm
                                from rm in temRm.DefaultIfEmpty()

                                where p.party_id == party_id
                                select new
                                {
                                    credit_limit = p.credit_limit ?? 0,
                                    requisition_code = rm.requisition_code ?? "First Requisition",
                                    amount = rm.amount ?? 0,
                                    requisition_master_id = rm.requisition_master_id
                                }).OrderByDescending(rm => rm.requisition_master_id).FirstOrDefault();
                    return data;
                }
            }

            catch (Exception ex)
            {
                return null;
            }
        }


        public object GetPartyAccountNumber(long party_id)
        {
            try
            {
                var accountNo = (from p in _entities.payment_request
                                 join acc in _entities.bank_account on p.bank_account_id equals acc.bank_account_id into tempAcc
                                 from acc in tempAcc.DefaultIfEmpty()
                                 where p.party_id == party_id
                                 select new
                             {
                                 account_no = acc.bank_account_name
                             }).FirstOrDefault();
                return accountNo;
            }

            catch (Exception ex)
            {
                return null;
            }
        }


        public object GetAllPaymentRequest(DateTime fromDate, DateTime toDate, long partyId)
        {
            try
            {
                var toDayes = toDate.AddDays(1);
                if (partyId != 0)
                {
                    var payReq = (from p in _entities.payment_request
                                  join par in _entities.parties
                                      on p.party_id equals par.party_id into temPar
                                  from par in temPar.DefaultIfEmpty()
                                  join pt in _entities.party_type
                                       on par.party_type_id equals pt.party_type_id into TempPt
                                  from pt in TempPt.DefaultIfEmpty()
                                  join pm in _entities.payment_method
                                      on p.payment_method_id equals pm.payment_method_id
                                  join rcv in _entities.receives on p.payment_req_id equals rcv.payment_req_id into tempRcv
                                  from rcv in tempRcv.DefaultIfEmpty()
                                  join pty in _entities.parties on p.party_id equals pty.party_id into tempPty
                                  from pty in tempPty.DefaultIfEmpty()
                                  join teri in _entities.territories on pty.territory_id equals teri.territory_id into temTeri
                                  from teri in temTeri.DefaultIfEmpty()
                                  join app in _entities.users on rcv.approved_by equals app.user_id into tempApp
                                  from app in tempApp.DefaultIfEmpty()
                                  join ba in _entities.banks
                                      on p.bank_id equals ba.bank_id into tempBank
                                  from ba in tempBank.DefaultIfEmpty()
                                  join bb in _entities.bank_branch on p.bank_branch_id equals bb.bank_branch_id into tempBb
                                  from bb in tempBb.DefaultIfEmpty()
                                  join acc in _entities.bank_account on p.bank_account_id equals acc.bank_account_id into tempAcc
                                  from acc in tempAcc.DefaultIfEmpty()


                                  where p.party_id == partyId && (p.deposite_date >= fromDate && p.deposite_date <= toDayes)

                                  select new
                                  {
                                      payment_req_id = p.payment_req_id,
                                      receipt_no = rcv.receipt_no ?? "Processing", // new
                                      bank_name = ba.bank_name,
                                      bank_branch_name = bb.bank_branch_name,
                                      bank_account_name = acc.bank_account_name,
                                      amount = p.amount,
                                      deposite_date = p.deposite_date,
                                      payment_method_id = pm.payment_method_id,
                                      payment_method_name = pm.payment_method_name,
                                      cheque_no = p.cheque_no,
                                      approved = p.approved,
                                      document_attachment = p.document_attachment,
                                      opening_balance = (from pj in _entities.party_journal
                                                         where pj.party_id == p.party_id
                                                         orderby pj.party_journal_id descending
                                                         select new { pj.closing_balance }).FirstOrDefault().closing_balance ?? 0, //new
                                      party_id = par.party_id,
                                      party_name = par.party_name,
                                      bank_branch_id = p.bank_branch_id,
                                      party_type_id = pt.party_type_id,
                                      party_type_name = pt.party_type_name, // new
                                      territory_name = teri.territory_name, //new
                                      status = rcv.status,
                                      approved_by = app.full_name ?? "Pending",
                                      sales_representative = p.sales_representative

                                  }).OrderByDescending(b => b.payment_req_id).ToList();
                    return payReq;
                }
                else
                {
                    var payReq = (from p in _entities.payment_request
                                  join par in _entities.parties
                                      on p.party_id equals par.party_id
                                  join pt in _entities.party_type
                                      on par.party_type_id equals pt.party_type_id
                                  join pm in _entities.payment_method
                                      on p.payment_method_id equals pm.payment_method_id
                                  join rcv in _entities.receives on p.payment_req_id equals rcv.payment_req_id into tempRcv
                                  from rcv in tempRcv.DefaultIfEmpty()
                                  join pty in _entities.parties on p.party_id equals pty.party_id into tempPty
                                  from pty in tempPty.DefaultIfEmpty()
                                  join teri in _entities.territories on pty.territory_id equals teri.territory_id into temTeri
                                  from teri in temTeri.DefaultIfEmpty()
                                  join app in _entities.users on rcv.approved_by equals app.user_id into tempApp
                                  from app in tempApp.DefaultIfEmpty()
                                  join ba in _entities.banks
                                      on p.bank_id equals ba.bank_id into tempBank
                                  from ba in tempBank.DefaultIfEmpty()
                                  join bb in _entities.bank_branch on p.bank_branch_id equals bb.bank_branch_id into tempBb
                                  from bb in tempBb.DefaultIfEmpty()
                                  join acc in _entities.bank_account on p.bank_account_id equals acc.bank_account_id into tempAcc
                                  from acc in tempAcc.DefaultIfEmpty()


                                  where p.deposite_date >= fromDate && p.deposite_date <= toDayes

                                  select new
                                  {
                                      payment_req_id = p.payment_req_id,
                                      receipt_no = rcv.receipt_no ?? "Processing", // new
                                      bank_name = ba.bank_name,
                                      bank_branch_name = bb.bank_branch_name,
                                      bank_account_name = acc.bank_account_name,
                                      amount = p.amount,
                                      deposite_date = p.deposite_date,
                                      payment_method_id = pm.payment_method_id,
                                      payment_method_name = pm.payment_method_name,
                                      cheque_no = p.cheque_no,
                                      approved = p.approved,
                                      document_attachment = p.document_attachment,
                                      opening_balance = (from pj in _entities.party_journal
                                                         where pj.party_id == p.party_id
                                                         orderby pj.party_journal_id descending
                                                         select new { pj.closing_balance }).FirstOrDefault().closing_balance ?? 0, //new
                                      party_id = par.party_id,
                                      party_name = par.party_name,
                                      bank_branch_id = p.bank_branch_id,
                                      party_type_id = pt.party_type_id,
                                      party_type_name = pt.party_type_name, // new
                                      territory_name = teri.territory_name, //new
                                      status = rcv.status,
                                      approved_by = app.full_name ?? "Pending",
                                      sales_representative = p.sales_representative

                                  }).OrderByDescending(b => b.payment_req_id).ToList();
                    return payReq;
                }
            }
            catch (Exception)
            {

                return null;
            }


        }


        public object GetAllUnProcessedPaymentList()
        {
            string query = "select pr.payment_req_id , pr.amount , FORMAT(pr.deposite_date , 'dd/MM/yyyy HH:mm:ss tt') as deposite_date , pm.payment_method_id , pm.payment_method_name , pr.cheque_no , pr.approved , pr.document_attachment , p.party_id, p.party_name , pt.party_type_id , pr.bank_branch_id , ba.bank_account_name , pt.party_type_name , t.territory_name , isnull(rcv.status,'Not Approved') as status , b.bank_name , bb.bank_branch_name , isnull(u.full_name,'Pending') as approved_by , (select top 1 CAST(isnull(party_journal.closing_balance,0) as decimal) from party_journal where party_journal.party_id=p.party_id order by party_journal.party_journal_id desc) as opening_balance , isnull(rcv.receipt_no,'Processing') as receipt_no from payment_request pr left join bank b on pr.bank_id=b.bank_id left join bank_branch bb on pr.bank_branch_id=bb.bank_branch_id left join bank_account ba on pr.bank_account_id= ba.bank_account_id left join party p on pr.party_id=p.party_id left join party_type pt on p.party_type_id=pt.party_type_id left join payment_method pm on pr.payment_method_id=pm.payment_method_id left join receive rcv on pr.payment_req_id=rcv.payment_req_id left join party p1 on pr.party_id=p1.party_id left join territory t on p1.territory_id=t.territory_id left join users u on rcv.approved_by=u.user_id where pr.is_deleted=0 and pr.approved=0 order by pr.payment_req_id desc";

            var reData = _entities.Database.SqlQuery<AllPaymentRequest>(query).ToList();
           
            return reData;
           
        }
    }
}
