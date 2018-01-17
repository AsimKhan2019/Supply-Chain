﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using DMSApi.Models.IRepository;
using DMSApi.Models.StronglyType;
using DMSApi.Models.crystal_models;

namespace DMSApi.Models.Repository
{
    public class RequisitionRepository : IRequisitionRepository
    {
        private DMSEntities _entities;
        private IMailRepository mailRepository;
        private INotifierMailAccountRepository notifierMailAccountRepository;


        public RequisitionRepository()
        {
            this._entities = new DMSEntities();
            this.mailRepository = new MailRepository();
            this.notifierMailAccountRepository = new NotifierMailAccountRepository();
        }

        public object GetAllRequisitions()
        {
            var requisition = (from rm in _entities.requisition_master
                               join rd in _entities.requisition_details on rm.requisition_master_id equals rd.requisition_master_id
                               join pro in _entities.products on rd.product_id equals pro.product_id
                               join c in _entities.colors on rd.color_id equals c.color_id
                               join pt in _entities.parties on rm.party_id equals pt.party_id
                               join u in _entities.units on rd.unit_id equals u.unit_id
                               select new
                               {
                                   requisition_master_id = rm.requisition_master_id,
                                   requisition_code = rm.requisition_code,
                                   created_by = rm.created_by,
                                   created_date = rm.created_date,
                                   party_id = rm.party_id,
                                   requisition_date = rm.requisition_date,
                                   delivery_status = rm.delivery_status,
                                   party_name = pt.party_name,
                                   finance_status = rm.finance_status

                               });
            return requisition;
        }

        public object GetAllForwardedRequisitionListbyUser(long user_id)
        {
            var requisition = (from rm in _entities.requisition_master
                               join rd in _entities.requisition_details on rm.requisition_master_id equals rd.requisition_master_id
                               join pro in _entities.products on rd.product_id equals pro.product_id
                               join c in _entities.colors on rd.color_id equals c.color_id
                               join pt in _entities.parties on rm.party_id equals pt.party_id
                               join u in _entities.units on rd.unit_id equals u.unit_id
                               select new
                               {
                                   requisition_master_id = rm.requisition_master_id,
                                   status = rm.status,
                                   amount = rm.amount,
                                   verified_by = rm.verified_by,
                                   requisition_code = rm.requisition_code,
                                   requisition_type = rm.requisition_type,
                                   created_by = rm.created_by,
                                   created_date = rm.created_date,
                                   party_id = rm.party_id,
                                   requisition_date = rm.requisition_date,
                                   delivery_status = rm.delivery_status,
                                   party_name = pt.party_name,
                                   finance_status = rm.finance_status

                               }).Where(r => (r.status == "Forward to HOS" || r.status == "Forward to HOS with payment check") && r.verified_by == user_id).ToList().Select(o => new
                              {
                                  requisition_master_id = o.requisition_master_id,
                                  status = o.status,
                                  amount = o.amount,
                                  verified_by = o.verified_by,
                                  requisition_code = o.requisition_code,
                                  requisition_type = o.requisition_type,
                                  created_by = o.created_by,
                                  created_date = o.created_date,
                                  party_id = o.party_id,
                                  requisition_date = o.requisition_date.ToString(),
                                  delivery_status = o.delivery_status,
                                  party_name = o.party_name,
                                  finance_status = o.finance_status
                              });
            return requisition;
        }

        public int AddRequisition(RequisitionModel rm)
        {
            var RequisitionMaster = rm.RequisitionMasterData;
            var ProductDetailsList = rm.RequisitionDetailsList;
            
            var partyTypePrefix = (from ptype in _entities.party_type
                                   join par in _entities.parties on ptype.party_type_id equals par.party_type_id
                                   where par.party_id == RequisitionMaster.party_id
                                   select new
                                {
                                    party_prefix = ptype.party_prefix

                                }).FirstOrDefault();
            // generate Requisition No
            int RequisitionSerial = _entities.requisition_master.Max(rq => (int?)rq.requisition_master_id) ?? 0;
            RequisitionSerial++;

            var rqStr = RequisitionSerial.ToString().PadLeft(7, '0');
            string requisitionNo = "REQ-" + partyTypePrefix.party_prefix + "-" + rqStr;
            RequisitionMaster.requisition_code = requisitionNo;
            RequisitionMaster.requisition_date = rm.RequisitionMasterData.requisition_date;
            RequisitionMaster.expected_receiving_date = rm.RequisitionMasterData.expected_receiving_date;
            RequisitionMaster.warehouse_from = rm.RequisitionMasterData.warehouse_from;
            RequisitionMaster.remarks = rm.RequisitionMasterData.remarks;
            RequisitionMaster.amount = rm.RequisitionMasterData.amount;
            RequisitionMaster.party_type_id = rm.RequisitionMasterData.party_type_id;
            RequisitionMaster.party_id = rm.RequisitionMasterData.party_id;
            RequisitionMaster.created_by = rm.RequisitionMasterData.created_by;
            RequisitionMaster.created_date = DateTime.Now;
            RequisitionMaster.updated_by = rm.RequisitionMasterData.created_by;
            RequisitionMaster.updated_date = DateTime.Now;
            RequisitionMaster.is_invoice_created = false;
            RequisitionMaster.status = "Not Forwarded"; //this is forwarded to finance by accounts
            RequisitionMaster.delivery_status = "Not Delivered";
            RequisitionMaster.edit_count = 0;
            RequisitionMaster.region_id = rm.RequisitionMasterData.region_id;
            RequisitionMaster.company_id = rm.RequisitionMasterData.company_id; // New Feild As per Maruf vai
            RequisitionMaster.area_id = rm.RequisitionMasterData.area_id;
            RequisitionMaster.credit_limit = rm.RequisitionMasterData.credit_limit;
            RequisitionMaster.credit_term = rm.RequisitionMasterData.credit_term;
            RequisitionMaster.contact_person_mobile = rm.RequisitionMasterData.contact_person_mobile;
            RequisitionMaster.address = rm.RequisitionMasterData.address;

            //RequisitionMaster.is_demo = rm.RequisitionMasterData.is_demo;
            RequisitionMaster.requisition_type = rm.RequisitionMasterData.requisition_type;
            RequisitionMaster.discount_percentage = 0;
            RequisitionMaster.discount_amount = 0;
            RequisitionMaster.incentive_percentage = 0;
            RequisitionMaster.incentive_amount = 0;
            RequisitionMaster.finance_status = "Not Approved";
            RequisitionMaster.is_deleted = false;
            RequisitionMaster.territory_id = rm.RequisitionMasterData.territory_id;

            ////////////////////22.02.2017///////////////////////////
            RequisitionMaster.reference_no = rm.RequisitionMasterData.reference_no;
            RequisitionMaster.price_type = rm.RequisitionMasterData.price_type;
            /// ////////////////////22.02.2017///////////////////////////

            _entities.requisition_master.Add(RequisitionMaster);
            _entities.SaveChanges();
            Int64 RequisitionMasterId = RequisitionMaster.requisition_master_id;


            //requisition details table
            foreach (var item in ProductDetailsList)
            {
                var requisitionDetails = new requisition_details
                {
                    requisition_master_id = RequisitionMasterId,
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
                    is_gift = item.is_gift,//04.04.2017
                    gift_type = item.gift_type,
                    is_live_dummy = item.is_live_dummy,
                    discount_amount = item.discount_amount,
                    promotion_master_id = item.promotion_master_id
                   

                };
                _entities.requisition_details.Add(requisitionDetails);
                _entities.SaveChanges();
            }
            return 1;
        }

        //Meraj-18-04-2017
        //public RequisitionModel GetRequisitionById(long requisition_master_id)
        //{
        //    try
        //    {
        //        RequisitionModel requisitionModel = new RequisitionModel();
        //        requisitionModel.RequisitionMasterData = _entities.requisition_master.Find(requisition_master_id);
        //        requisitionModel.RequisitionDetailsList = _entities.requisition_details
        //            .Join(_entities.products, jp => jp.product_id, p => p.product_id, (jp, p) => new { jp, p })
        //            //.Join(_entities.product_category, jcat => jcat.jp.product_category_id,
        //             //cat => cat.product_category_id, (jcat, cat) => new { jcat, cat })
        //            //.Join(_entities.brands, jb => jb.jcat.jp.brand_id, b => b.brand_id, (jb, b) => new { jb, b })
        //            .GroupJoin(_entities.colors, jc => jc.jp.color_id, c => c.color_id, (jc, c) => new { jc, nc = c.FirstOrDefault() })
        //            //.Join(_entities.units, ju => ju.jc.jb.jcat.jp.unit_id, u => u.unit_id, (ju, u) => new { ju, u })
        //            .GroupJoin(_entities.product_version, jv => jv.jc.jp.product_version_id, v => v.product_version_id, (jv, v) => new { jv, nv = v.FirstOrDefault() })
        //            //06.04.2017
        //            .GroupJoin(_entities.promotion_master, jpm => jpm.jv.jc.jp.promotion_master_id, pm => pm.promotion_master_id, (jpm, pm) => new { jpm, npm = pm.FirstOrDefault() })
        //            //.Where(w => w.jv.ju.jc.jb.jcat.jp.requisition_master_id == requisition_master_id && string.IsNullOrEmpty(w.jv.ju.jc.jb.jcat.jp.gift_type))
        //            .Where(w => w.jpm.jv.jc.jp.requisition_master_id == requisition_master_id )
        //            .Select(i => new RequisitionDetailsModel
        //            {
        //                requisition_master_id = i.jpm.jv.jc.jp.requisition_master_id,
        //                requisition_details_id = i.jpm.jv.jc.jp.requisition_details_id,
        //                is_gift = i.jpm.jv.jc.jp.is_gift,
        //                gift_type = i.jpm.jv.jc.jp.gift_type,
        //                product_id = i.jpm.jv.jc.jp.product_id,
        //                product_name = i.jpm.jv.jc.p.product_name,
        //                brand_id = i.jpm.jv.jc.jp.brand_id,
        //                color_id = i.jpm.jv.nc.color_id,
        //                color_name = string.IsNullOrEmpty(i.jpm.jv.nc.color_name) ? "" : i.jpm.jv.nc.color_name,
        //                quantity = i.jpm.jv.jc.jp.quantity,
        //                price = i.jpm.jv.jc.jp.price,
        //                delivered_quantity = i.jpm.jv.jc.jp.delivered_quantity,
        //                is_live_dummy = i.jpm.jv.jc.jp.is_live_dummy,
        //                line_total = i.jpm.jv.jc.jp.line_total,
        //                product_version_id = i.jpm.nv.product_version_id,
        //                product_version_name = string.IsNullOrEmpty(i.jpm.nv.product_version_name) ? "" : i.jpm.nv.product_version_name,
        //                has_serial = i.jpm.jv.jc.p.has_serial ?? false,
        //                discount_amount = i.jpm.jv.jc.jp.discount_amount,
        //                promotion_master_id = i.jpm.jv.jc.jp.promotion_master_id,
        //                promotion_name = i.npm.promotion_name,
        //                promotion_type = i.npm.promotion_type
        //            }).OrderBy(p => p.requisition_details_id).ToList();
        //        return requisitionModel;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }


        //}

        public RequisitionModel GetRequisitionById(long requisition_master_id)
        {
            try
            {
                RequisitionModel requisitionModel = new RequisitionModel();
                requisitionModel.RequisitionMasterData = _entities.requisition_master.Find(requisition_master_id);
                requisitionModel.RequisitionDetailsList = _entities.requisition_details
                    .Join(_entities.products, jp => jp.product_id, p => p.product_id, (jp, p) => new { jp, p })
                    .Join(_entities.product_category, jcat => jcat.jp.product_category_id,
                     cat => cat.product_category_id, (jcat, cat) => new { jcat, cat })
                    .Join(_entities.brands, jb => jb.jcat.jp.brand_id, b => b.brand_id, (jb, b) => new { jb, b })
                    .GroupJoin(_entities.colors, jc => jc.jb.jcat.jp.color_id, c => c.color_id,
                        (jc, c) => new { jc, nc = c.FirstOrDefault() })
                    .Join(_entities.units, ju => ju.jc.jb.jcat.jp.unit_id, u => u.unit_id, (ju, u) => new { ju, u })
                    .GroupJoin(_entities.product_version, jv => jv.ju.jc.jb.jcat.jp.product_version_id,
                        v => v.product_version_id, (jv, v) => new { jv, nv = v.FirstOrDefault() })
                    //06.04.2017
                    .GroupJoin(_entities.promotion_master, jpm => jpm.jv.ju.jc.jb.jcat.jp.promotion_master_id,
                        pm => pm.promotion_master_id, (jpm, pm) => new { jpm, npm = pm.FirstOrDefault() })
                    //.Where(w => w.jv.ju.jc.jb.jcat.jp.requisition_master_id == requisition_master_id && string.IsNullOrEmpty(w.jv.ju.jc.jb.jcat.jp.gift_type))
                    .Where(
                        w =>
                            w.jpm.jv.ju.jc.jb.jcat.jp.requisition_master_id == requisition_master_id &&
                            string.IsNullOrEmpty(w.jpm.jv.ju.jc.jb.jcat.jp.gift_type))
                    .Select(i => new RequisitionDetailsModel
                    {
                        requisition_master_id = i.jpm.jv.ju.jc.jb.jcat.jp.requisition_master_id,
                        requisition_details_id = i.jpm.jv.ju.jc.jb.jcat.jp.requisition_details_id,
                        is_gift = i.jpm.jv.ju.jc.jb.jcat.jp.is_gift,
                        product_id = i.jpm.jv.ju.jc.jb.jcat.jp.product_id,
                        product_name = i.jpm.jv.ju.jc.jb.jcat.p.product_name,
                        brand_id = i.jpm.jv.ju.jc.jb.jcat.jp.brand_id,
                        product_category_id = i.jpm.jv.ju.jc.jb.cat.product_category_id,
                        product_category_name = i.jpm.jv.ju.jc.jb.cat.product_category_name,
                        color_id = i.jpm.jv.ju.nc.color_id,
                        color_name = string.IsNullOrEmpty(i.jpm.jv.ju.nc.color_name) ? "N/A" : i.jpm.jv.ju.nc.color_name,
                        unit_id = i.jpm.jv.ju.jc.jb.jcat.jp.unit_id,
                        unit_name = i.jpm.jv.u.unit_name,
                        quantity = i.jpm.jv.ju.jc.jb.jcat.jp.quantity,
                        price = i.jpm.jv.ju.jc.jb.jcat.jp.price,
                        delivered_quantity = i.jpm.jv.ju.jc.jb.jcat.jp.delivered_quantity,
                        is_live_dummy = i.jpm.jv.ju.jc.jb.jcat.jp.is_live_dummy,
                        line_total = i.jpm.jv.ju.jc.jb.jcat.jp.line_total,
                        product_version_id = i.jpm.nv.product_version_id,
                        product_version_name =
                            string.IsNullOrEmpty(i.jpm.nv.product_version_name) ? "N/A" : i.jpm.nv.product_version_name,
                        has_serial = i.jpm.jv.ju.jc.jb.jcat.p.has_serial ?? false,
                        discount_amount = i.jpm.jv.ju.jc.jb.jcat.jp.discount_amount,
                        promotion_master_id = i.jpm.jv.ju.jc.jb.jcat.jp.promotion_master_id,
                        promotion_name = i.npm.promotion_name,
                        promotion_type = i.npm.promotion_type
                    }).OrderBy(p => p.requisition_details_id).ToList();
                return requisitionModel;
            }
            catch (Exception)
            {

                throw;
            }


        }




        //Meraj-17-04-2017
        //public object GetRequisitionByIdForFinanceApprove(long requisition_master_id)
        //{
        //    try
        //    {
        //        var req = (from rqd in _entities.requisition_details
        //                   join p in _entities.products on rqd.product_id equals p.product_id
        //                   //join pc in _entities.product_category on rqd.product_category_id equals pc.product_category_id
        //                   join c in _entities.colors on rqd.color_id equals c.color_id into Tempc
        //                   from c in Tempc.DefaultIfEmpty()
        //                   //join b in _entities.brands on rqd.brand_id equals b.brand_id
        //                   //join u in _entities.units on rqd.unit_id equals u.unit_id
        //                   //added on 04.04.2017(mohi uddin)
        //                   join v in _entities.product_version on rqd.product_version_id equals v.product_version_id into Tempv
        //                   from v in Tempv.DefaultIfEmpty()

        //                   select new
        //                   {
        //                       requisition_master_id = rqd.requisition_master_id,
        //                       requisition_details_id = rqd.requisition_details_id,
        //                       color_id = rqd.color_id,
        //                       color_name = c.color_name,
        //                       product_id = rqd.product_id,
        //                       product_name = p.product_name,
        //                       product_category_id = rqd.product_category_id,
        //                       unit_id = rqd.unit_id,
        //                       //unit_name = u.unit_name,
        //                       quantity = rqd.quantity,
        //                       price = rqd.price,
        //                       delivered_quantity = rqd.delivered_quantity,
        //                       is_gift = rqd.is_gift,
        //                       gift_type = rqd.gift_type,
        //                       is_live_dummy = rqd.is_live_dummy,
        //                       brand_id = rqd.brand_id,

        //                       //added on 04.04.2017(mohi uddin)
        //                       product_version_id = rqd.product_version_id,
        //                       product_version_name = v.product_version_name

        //                   }).Where(r => r.requisition_master_id == requisition_master_id && r.gift_type == "Promotional Gift").ToList();

        //        return req;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}


        ////for requisition approve by finance//Sales gift
        public List<RequisitionDetailsModel> GetRequisitionByIdForFinanceApprove(long requisition_master_id)
        {
            try
            {
                //List<RequisitionDetailsModel> requisitionDetails = new List<RequisitionDetailsModel>();
                RequisitionDetailsModel requisitionDetailsModel = new RequisitionDetailsModel();

                var req = (from rqd in _entities.requisition_details
                           join p in _entities.products on rqd.product_id equals p.product_id
                           join pc in _entities.product_category on rqd.product_category_id equals pc.product_category_id into
                               Tempcat
                           from pc in Tempcat.DefaultIfEmpty()
                           join c in _entities.colors on rqd.color_id equals c.color_id into Tempc
                           from c in Tempc.DefaultIfEmpty()
                           join b in _entities.brands on rqd.brand_id equals b.brand_id into Tempb
                           from b in Tempb.DefaultIfEmpty()
                           join u in _entities.units on rqd.unit_id equals u.unit_id into TempU
                           from u in TempU.DefaultIfEmpty()
                           //added on 04.04.2017(mohi uddin)
                           join v in _entities.product_version on rqd.product_version_id equals v.product_version_id into Tempv
                           from v in Tempv.DefaultIfEmpty()

                           select new RequisitionDetailsModel
                           {
                               requisition_master_id = rqd.requisition_master_id,
                               requisition_details_id = rqd.requisition_details_id,
                               color_id = rqd.color_id,
                               //color_name = c.color_name,
                               color_name = string.IsNullOrEmpty(c.color_name) ? "N/A" : c.color_name,
                               product_id = rqd.product_id,
                               product_name = p.product_name,
                               product_category_id = rqd.product_category_id,
                               unit_id = rqd.unit_id,
                               unit_name = u.unit_name,
                               quantity = rqd.quantity,
                               price = rqd.price,
                               delivered_quantity = rqd.delivered_quantity,
                               is_gift = rqd.is_gift,
                               gift_type = rqd.gift_type,
                               is_live_dummy = rqd.is_live_dummy,
                               brand_id = rqd.brand_id,

                               //added on 04.04.2017(mohi uddin)
                               product_version_id = rqd.product_version_id,
                               product_version_name =
                                   string.IsNullOrEmpty(v.product_version_name) ? "N/A" : v.product_version_name,
                               promotion_master_id = rqd.promotion_master_id

                           }).Where(r => r.requisition_master_id == requisition_master_id && r.gift_type == "Promotional Gift")
                    .ToList();

               
                return req;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool EditRequisition(RequisitionModel RequisitionModel)
        {
            try
            {
                var RequisitionMaster = RequisitionModel.RequisitionMasterData;
                var RequisitionDetailsList = RequisitionModel.RequisitionDetailsList;

                requisition_master masterData =
                    _entities.requisition_master.Find(RequisitionMaster.requisition_master_id);
                
                masterData.party_type_id = masterData.party_type_id;
                masterData.party_id = masterData.party_id;
                masterData.requisition_code = RequisitionMaster.requisition_code;
                masterData.requisition_date = RequisitionMaster.requisition_date;
                masterData.status = "Not Forwarded";
                masterData.remarks = RequisitionMaster.remarks;
                masterData.is_invoice_created = masterData.is_invoice_created;
                masterData.delivery_status = masterData.delivery_status;
                masterData.payment_method_id = RequisitionMaster.payment_method_id;

                masterData.created_by = masterData.created_by;
                masterData.created_date = masterData.created_date;
                masterData.updated_by = RequisitionMaster.updated_by;
                masterData.updated_date = DateTime.Now;

                masterData.expected_receiving_date = RequisitionMaster.expected_receiving_date;
                masterData.warehouse_from = RequisitionMaster.warehouse_from;
                masterData.payment_method_id = RequisitionMaster.payment_method_id;
                masterData.amount = RequisitionMaster.amount;
                masterData.edit_count = masterData.edit_count + 1;
                masterData.region_id = masterData.region_id;
                masterData.area_id = masterData.area_id;
                masterData.company_id = masterData.company_id;
                masterData.credit_term = masterData.credit_term;
                masterData.contact_person_mobile = masterData.contact_person_mobile;
                masterData.address = masterData.address;
                masterData.territory_id = masterData.territory_id;
                //////////////23.02.2017///////////
                masterData.reference_no = masterData.reference_no;
                masterData.price_type = masterData.price_type;
                /////////////23.02.2017///////////
                
                _entities.SaveChanges();

                //update details table
                //foreach (var item in RequisitionDetailsList)
                //{
                //    requisition_details detailsData =
                //        _entities.requisition_details.FirstOrDefault(rd => rd.requisition_master_id == RequisitionMaster.requisition_master_id && rd.requisition_details_id == item.requisition_details_id);
                //    if (detailsData != null)
                //    {
                //        detailsData.requisition_master_id = RequisitionMaster.requisition_master_id;
                //        detailsData.brand_id = item.brand_id;
                //        detailsData.product_category_id = item.product_category_id;
                //        detailsData.product_id = item.product_id;
                //        detailsData.color_id = item.color_id;
                //        detailsData.quantity = item.quantity;
                //        //detailsData.delivered_quantity = detailsData.delivered_quantity;
                //        detailsData.price = item.price;
                //        detailsData.is_live_dummy = item.is_live_dummy;
                //        detailsData.unit_id = item.unit_id;
                //        detailsData.line_total = item.line_total;

                //        detailsData.product_version_id = item.product_version_id;
                //        detailsData.gift_type = item.gift_type;
                //        detailsData.discount_amount = item.discount_amount;
                //        detailsData.promotion_master_id = item.promotion_master_id;
                        
                //        _entities.SaveChanges();
                //    }
                //    else
                //    {
                //        var requisitionDetails = new requisition_details
                //        {
                //            requisition_master_id = RequisitionMaster.requisition_master_id,
                //            brand_id = item.brand_id,
                //            product_category_id = item.product_category_id,
                //            product_id = item.product_id,
                //            color_id = item.color_id,
                //            quantity = item.quantity,
                //            price = item.price,
                //            delivered_quantity = 0,
                //            //is_gift = false,
                //            is_gift = item.is_gift,
                //            //is_live_dummy = item.is_live_dummy,
                //            //is_live_dummy = false,
                //            is_live_dummy = item.is_live_dummy,
                //            unit_id = item.unit_id,
                //            line_total = item.line_total,
                //            product_version_id = item.product_version_id,

                //            gift_type = item.gift_type,
                //            discount_amount = item.discount_amount,
                //            promotion_master_id = item.promotion_master_id

                //        };
                //        _entities.requisition_details.Add(requisitionDetails);
                //        _entities.SaveChanges();
                //    }
                //}
                //09.04.2017
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
                



                return true;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public bool DeleteRequisition(long requisition_details_id)
        {
            throw new NotImplementedException();
        }

        public bool DeleteRequisitionDetailsById(long requisition_details_id)
        {
            try
            {
                requisition_details oRequisitionDetails = _entities.requisition_details.Find(requisition_details_id);

                int masterId = (int)oRequisitionDetails.requisition_master_id;
                var xxx = _entities.requisition_master.Find(masterId);
                xxx.amount = xxx.amount - oRequisitionDetails.line_total;
                _entities.SaveChanges();

                _entities.requisition_details.Attach(oRequisitionDetails);
                _entities.requisition_details.Remove(oRequisitionDetails);
                _entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateApproveStatus(long requisition_master_id)
        {
            try
            {
                requisition_master reqMaster = _entities.requisition_master.Find(requisition_master_id);
                reqMaster.status = "Approved";
                _entities.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                throw;
            }

        }


        public object GetPartywiseRequisitionsForDelivery(long partyId)
        {
            var reqMasters =
                _entities.requisition_master.Where(
                    r => r.party_id == partyId && r.finance_status == "Approved" && r.delivery_status != "Delivered")
                    .ToList();
            return reqMasters;
        }
       
        public object GetAAllRequisition()
        {
            var requisition = (from rm in _entities.requisition_master
                               join pt in _entities.parties on rm.party_id equals pt.party_id
                               join w in _entities.warehouses on rm.warehouse_from equals w.warehouse_id
                               join com in _entities.companies on rm.company_id equals com.company_id into tempCom
                               from com in tempCom.DefaultIfEmpty()
                               select new
                               {
                                   requisition_master_id = rm.requisition_master_id,
                                   requisition_code = rm.requisition_code,
                                   created_by = rm.created_by,
                                   created_date = rm.created_date,
                                   party_id = rm.party_id,
                                   requisition_date = rm.requisition_date,
                                   delivery_status = rm.delivery_status,
                                   party_name = pt.party_name,
                                   amount = rm.amount,
                                   expected_receiving_date = rm.expected_receiving_date,
                                   warehouse_from = rm.warehouse_from,
                                   warehouse_name = w.warehouse_name,
                                   status = rm.status,
                                   finance_status = rm.finance_status,
                                   company_name = com.company_name
                               }

                ).OrderByDescending(e => e.requisition_master_id).ToList();
            ;
            return requisition;
        }

        //load dealer requisition
        public object GetAllRequisitionByPartyId(long party_id)
        {
            if (party_id == 1)
            {
                var requisition = (from rm in _entities.requisition_master
                                   join pt in _entities.parties on rm.party_id equals pt.party_id
                                   join w in _entities.warehouses on rm.warehouse_from equals w.warehouse_id
                                   //where (rm.is_deleted == false || rm.is_deleted == null)
                                   join com in _entities.companies on rm.company_id equals com.company_id into temCom
                                   from com in temCom.DefaultIfEmpty()
                                   where
                                       ((rm.is_deleted == false || rm.is_deleted == null) && (rm.is_demo == null || rm.is_demo == ""))
                                   select new
                                   {
                                       requisition_master_id = rm.requisition_master_id,
                                       requisition_code = rm.requisition_code,
                                       created_by = rm.created_by,
                                       created_date = rm.created_date,
                                       party_id = rm.party_id,
                                       requisition_date = rm.requisition_date,
                                       delivery_status = rm.delivery_status,
                                       party_name = pt.party_name,
                                       amount = rm.amount,
                                       expected_receiving_date = rm.expected_receiving_date,
                                       warehouse_from = rm.warehouse_from,
                                       warehouse_name = w.warehouse_name,
                                       status = rm.status,
                                       finance_status = rm.finance_status,
                                       requisition_type = rm.requisition_type,
                                       company_name = com.company_name,
                                       forward_2_status = rm.forward_2_status,
                                       deposite_date=_entities.payment_request.FirstOrDefault(pr=>pr.requisition_master_id==rm.requisition_master_id).deposite_date,
                                       verified_date = rm.verified_date,
                                       approved_date=rm.approved_date,
                                       forward_2_date=rm.forward_2_date
                                   }

              ).OrderByDescending(e => e.requisition_master_id).ToList()
                    .Select(o => new
              {
                  requisition_master_id = o.requisition_master_id,
                  requisition_code = o.requisition_code,
                  created_by = o.created_by,
                  created_date = o.created_date,
                  party_id = o.party_id,
                  requisition_date = o.requisition_date.ToString(),
                  delivery_status = o.delivery_status,
                  party_name = o.party_name,
                  amount = o.amount,
                  expected_receiving_date = o.expected_receiving_date,
                  warehouse_from = o.warehouse_from,
                  warehouse_name = o.warehouse_name,
                  status = o.status,
                  finance_status = o.finance_status,
                  requisition_type = o.requisition_type,
                  company_name = o.company_name,
                  forward_2_status = o.forward_2_status,

                  deposite_date = o.deposite_date.ToString() ?? "",
                  verified_date = o.verified_date.ToString() ?? "",
                  approved_date = o.approved_date.ToString() ?? "",
                  forward_2_date=o.forward_2_date.ToString() ?? ""
              });
              
              
                return requisition;
            }
            else
            {
                var requisition = (from rm in _entities.requisition_master
                    join pt in _entities.parties on rm.party_id equals pt.party_id
                    join w in _entities.warehouses on rm.warehouse_from equals w.warehouse_id
                                   join com in _entities.companies on rm.company_id equals com.company_id into tempCom
                                   from com in tempCom.DefaultIfEmpty()
                                   
                                   //where (rm.party_id == party_id && (rm.is_deleted == false || rm.is_deleted == null))
                                   where
                                       (rm.party_id == party_id && (rm.is_deleted == false || rm.is_deleted == null) &&
                                        (rm.is_demo == null || rm.is_demo == ""))
                                   select new
                                   {
                                       requisition_master_id = rm.requisition_master_id,
                                       requisition_code = rm.requisition_code,
                                       created_by = rm.created_by,
                                       created_date = rm.created_date,
                                       party_id = rm.party_id,
                                       requisition_date = rm.requisition_date,
                                       delivery_status = rm.delivery_status,
                                       party_name = pt.party_name,
                                       amount = rm.amount,
                                       expected_receiving_date = rm.expected_receiving_date,
                                       warehouse_from = rm.warehouse_from,
                                       warehouse_name = w.warehouse_name,
                                       status = rm.status,
                                       finance_status = rm.finance_status,
                                       company_name = com.company_name
                                   }

              ).OrderByDescending(e => e.requisition_master_id).ToList()
                    .Select(o => new
              {
                  requisition_master_id = o.requisition_master_id,
                  requisition_code = o.requisition_code,
                  created_by = o.created_by,
                  created_date = o.created_date,
                  party_id = o.party_id,
                  requisition_date = o.requisition_date.ToString(),
                  delivery_status = o.delivery_status,
                  party_name = o.party_name,
                  amount = o.amount,
                  expected_receiving_date = o.expected_receiving_date,
                  warehouse_from = o.warehouse_from,
                  warehouse_name = o.warehouse_name,
                  status = o.status,
                  finance_status = o.finance_status,
                  company_name = o.company_name
              });
              
               
                return requisition;
            }

        }

        //to show all demo requisiton in list(04.02.2017)
        public object GetAllDealerDemoRequisitionByPartyId(long party_id)
        {
            if (party_id == 1)
            {
                //requisition list view for admin ----------------------------
                var requisition = (from rm in _entities.requisition_master
                                   join pt in _entities.parties on rm.party_id equals pt.party_id
                                   join w in _entities.warehouses on rm.warehouse_from equals w.warehouse_id
                                   join com in _entities.companies on rm.company_id equals com.company_id into tempCom
                                   from com in tempCom.DefaultIfEmpty()
                                   where ((rm.is_deleted == false || rm.is_deleted == null) && (rm.is_demo == "Dealer Demo"))
                                   select new
                                   {
                                       requisition_master_id = rm.requisition_master_id,
                                       requisition_code = rm.requisition_code,
                                       created_by = rm.created_by,
                                       created_date = rm.created_date,
                                       party_id = rm.party_id,
                                       requisition_date = rm.requisition_date,
                                       delivery_status = rm.delivery_status,
                                       party_name = pt.party_name,
                                       amount = rm.amount,
                                       expected_receiving_date = rm.expected_receiving_date,
                                       warehouse_from = rm.warehouse_from,
                                       warehouse_name = w.warehouse_name,
                                       status = rm.status,
                                       finance_status = rm.finance_status,
                                       company_name = com.company_name
                                   }

                    ).OrderByDescending(e => e.requisition_master_id).ToList();
                ;
                return requisition;
            }
            else
            {
                //requisition list view for MD/DBIS/Retailer ----------------------------
                var requisition = (from rm in _entities.requisition_master
                                   join pt in _entities.parties on rm.party_id equals pt.party_id
                                   join w in _entities.warehouses on rm.warehouse_from equals w.warehouse_id
                                   join com in _entities.companies on rm.company_id equals com.company_id into tempCom
                                   from com in tempCom.DefaultIfEmpty()
                                   where
                                       (rm.party_id == party_id && (rm.is_deleted == false || rm.is_deleted == null) &&
                                        (rm.is_demo == "Dealer Demo"))
                                   select new
                                   {
                                       requisition_master_id = rm.requisition_master_id,
                                       requisition_code = rm.requisition_code,
                                       created_by = rm.created_by,
                                       created_date = rm.created_date,
                                       party_id = rm.party_id,
                                       requisition_date = rm.requisition_date,
                                       delivery_status = rm.delivery_status,
                                       party_name = pt.party_name,
                                       amount = rm.amount,
                                       expected_receiving_date = rm.expected_receiving_date,
                                       warehouse_from = rm.warehouse_from,
                                       warehouse_name = w.warehouse_name,
                                       status = rm.status,
                                       finance_status = rm.finance_status,
                                       company_name = com.company_name
                                   }

                    ).OrderByDescending(e => e.requisition_master_id).ToList();
                ;
                return requisition;
            }
        }

        //Get demo requisition for accounts head approval(06.02.2017)
        public object GetDemoRequisitionForVerifyByPartyId(long party_id)
        {
            try
            {
                var requisition = (from rm in _entities.requisition_master
                                   join pt in _entities.parties on rm.party_id equals pt.party_id
                                   join w in _entities.warehouses on rm.warehouse_from equals w.warehouse_id
                                   join com in _entities.companies on rm.company_id equals com.company_id into tempCom
                                   from com in tempCom.DefaultIfEmpty()
                                   where
                                       (rm.is_deleted == false && rm.status == "Not Approved" && rm.is_demo == "Dealer Demo" &&
                                        (rm.forwarded_status != "Forwarded" || rm.forwarded_status == null))
                                   //&& rm.forwarded_status!="Forwarded"
                                   select new
                                   {
                                       requisition_master_id = rm.requisition_master_id,
                                       requisition_code = rm.requisition_code,
                                       created_by = rm.created_by,
                                       created_date = rm.created_date,
                                       party_id = rm.party_id,
                                       requisition_date = rm.requisition_date,
                                       delivery_status = rm.delivery_status,
                                       party_name = pt.party_name,
                                       amount = rm.amount,
                                       expected_receiving_date = rm.expected_receiving_date,
                                       warehouse_from = rm.warehouse_from,
                                       warehouse_name = w.warehouse_name,
                                       status = rm.status,
                                       finance_status = rm.finance_status,
                                       forwarded_status = rm.forwarded_status,
                                       forwarded_by = rm.forwarded_by,
                                       forwarded_date = rm.forwarded_date,
                                       company_name = com.company_name
                                   }

                    ).OrderByDescending(e => e.requisition_master_id).ToList();
                ;
                return requisition;

            }
            catch (Exception)
            {

                throw;
            }
        }

        //load dealer demo requisition for Sales head approval(after accounts approval)
        public object GetDemoRequisitionForApprovalByPartyId(long party_id)
        {
            try
            {
                var requisition = (from rm in _entities.requisition_master
                                   join pt in _entities.parties on rm.party_id equals pt.party_id
                                   join w in _entities.warehouses on rm.warehouse_from equals w.warehouse_id
                                   join com in _entities.companies on rm.company_id equals com.company_id into tempCom
                                   from com in tempCom.DefaultIfEmpty()
                                   where (rm.is_deleted == false && (rm.status == "Approved" && rm.is_demo == "Dealer Demo"))
                                   select new
                                   {
                                       requisition_master_id = rm.requisition_master_id,
                                       requisition_code = rm.requisition_code,
                                       created_by = rm.created_by,
                                       created_date = rm.created_date,
                                       party_id = rm.party_id,
                                       requisition_date = rm.requisition_date,
                                       delivery_status = rm.delivery_status,
                                       party_name = pt.party_name,
                                       amount = rm.amount,
                                       expected_receiving_date = rm.expected_receiving_date,
                                       warehouse_from = rm.warehouse_from,
                                       warehouse_name = w.warehouse_name,
                                       status = rm.status,
                                       finance_status = rm.finance_status,
                                       forwarded_status = rm.forwarded_status,
                                       forwarded_by = rm.forwarded_by,
                                       forwarded_date = rm.forwarded_date,
                                       requisition_type = rm.requisition_type,
                                       company_name = com.company_name
                                   }

                    ).OrderByDescending(e => e.requisition_master_id).ToList();
                ;
                return requisition;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //when credit limit exced and accounts forward to HOOps then this list will load for HOOps(18.02.2017)
        public object GetRequisitionListForHOOps(long party_id)
        {
            try
            {
             //after six month this may be need
             //   var requisition = (from rm in _entities.requisition_master
             //                      join pt in _entities.parties on rm.party_id equals pt.party_id
             //                      join w in _entities.warehouses on rm.warehouse_from equals w.warehouse_id
             //                      join com in _entities.companies on rm.company_id equals com.company_id into tempCom from com in tempCom.DefaultIfEmpty()
             //                      where (rm.is_deleted == false && (rm.status == "Fwd to HOOps" && rm.forward_2_status == null))
             //                      select new
             //                      {
             //                          requisition_master_id = rm.requisition_master_id,
             //                          requisition_code = rm.requisition_code,
             //                          created_by = rm.created_by,
             //                          created_date = rm.created_date,
             //                          party_id = rm.party_id,
             //                          requisition_date = rm.requisition_date,
             //                          delivery_status = rm.delivery_status,
             //                          party_name = pt.party_name,
             //                          amount = rm.amount,
             //                          expected_receiving_date = rm.expected_receiving_date,
             //                          warehouse_from = rm.warehouse_from,
             //                          warehouse_name = w.warehouse_name,
             //                          status = rm.status,
             //                          finance_status = rm.finance_status,
             //                          forwarded_status = rm.forwarded_status,
             //                          forwarded_by = rm.forwarded_by,
             //                          forwarded_date = rm.forwarded_date,
             //                          requisition_type = rm.requisition_type,
             //                          company_name = com.company_name
             //                      }

             //).OrderByDescending(e => e.requisition_master_id).ToList(); ;
             //   return requisition;

                //01.04.2017
                //load requisition which are approved by both accounts & sales head
                var requisition = (from rm in _entities.requisition_master
                                   join pt in _entities.parties on rm.party_id equals pt.party_id
                                   join w in _entities.warehouses on rm.warehouse_from equals w.warehouse_id
                                   join com in _entities.companies on rm.company_id equals com.company_id into tempCom
                                   from com in tempCom.DefaultIfEmpty()
                                   //where (rm.is_deleted == false && rm.status == "Fwd to HOS" && rm.finance_status == "Approved" && (rm.forward_2_status == null || rm.forward_2_status=="Approved"))
                                   //29.04.2017
                                   where
                                       (rm.is_deleted == false &&
                                        (rm.status == "Forward to HOS" || rm.status == "Forward to HOS with payment check") &&
                                        rm.finance_status == "Approved" &&
                                        (rm.forward_2_status == null || rm.forward_2_status == "Approved"))
                                   select new
                                   {
                                       requisition_master_id = rm.requisition_master_id,
                                       requisition_code = rm.requisition_code,
                                       created_by = rm.created_by,
                                       created_date = rm.created_date,
                                       party_id = rm.party_id,
                                       requisition_date = rm.requisition_date,
                                       delivery_status = rm.delivery_status,
                                       party_name = pt.party_name,
                                       amount = rm.amount,
                                       expected_receiving_date = rm.expected_receiving_date,
                                       warehouse_from = rm.warehouse_from,
                                       warehouse_name = w.warehouse_name,
                                       status = rm.status,
                                       finance_status = rm.finance_status,
                                       forwarded_status = rm.forwarded_status,
                                       forwarded_by = rm.forwarded_by,
                                       forwarded_date = rm.forwarded_date,
                                       requisition_type = rm.requisition_type,
                                       company_name = com.company_name,
                                       forward_2_status = rm.forward_2_status,

                                       deposite_date = _entities.payment_request.FirstOrDefault(pr => pr.requisition_master_id == rm.requisition_master_id).deposite_date,
                                       verified_date = rm.verified_date,
                                       approved_date = rm.approved_date,
                                       forward_2_date = rm.forward_2_date,
                                       is_hold=rm.is_hold
                                   }

             ).OrderByDescending(e => e.requisition_master_id).ToList()
                    .Select(o => new
             {
                 requisition_master_id = o.requisition_master_id,
                        requisition_code = o.requisition_code,
                        created_by = o.created_by,
                        created_date = o.created_date,
                        party_id = o.party_id,
                 requisition_date = o.requisition_date.ToString(),
                 delivery_status = o.delivery_status,
                 party_name = o.party_name,
                 amount = o.amount,
                 expected_receiving_date = o.expected_receiving_date,
                 warehouse_from = o.warehouse_from,
                 warehouse_name = o.warehouse_name,
                 status = o.status,
                 finance_status = o.finance_status,
                 forwarded_status = o.forwarded_status,
                 forwarded_by = o.forwarded_by,
                 forwarded_date = o.forwarded_date,
                 requisition_type = o.requisition_type,
                 company_name = o.company_name,
                 forward_2_status = o.forward_2_status,

                 deposite_date = o.deposite_date.ToString() ?? "",
                 verified_date = o.verified_date.ToString() ?? "",
                 approved_date = o.approved_date.ToString() ?? "",
                 forward_2_date = o.forward_2_date.ToString() ?? "",
                 is_hold=o.is_hold
             });
               
                    
                 
                return requisition;
            }
            catch (Exception)
            {
                throw;
            }
        }

       


        public object GetAllDemoRequisition()
        {
            try
            {
                return null;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public object GetAllDeliverableRequisition()
        {
            try
            {
                var requisition = (from rm in _entities.requisition_master
                                   join pt in _entities.parties on rm.party_id equals pt.party_id
                                   join w in _entities.warehouses on rm.warehouse_from equals w.warehouse_id
                                   //where (rm.is_deleted == false && (rm.finance_status == "Approved" && (rm.delivery_status == "Not Delivered" || rm.delivery_status == "Partially Delivered"))) // maruf: approve not possible without forward by accouts
                                   //22.05.2017
                                   where
                                       (rm.is_deleted == false &&
                                        (rm.forward_2_status == "Approved" &&
                                         (rm.delivery_status == "Not Delivered" || rm.delivery_status == "Partially Delivered")))
                                   // mohiuddin: approve not possible without OOps approval
                                   select new
                                   {
                                       requisition_master_id = rm.requisition_master_id,
                                       requisition_code = rm.requisition_code,
                                       created_by = rm.created_by,
                                       created_date = rm.created_date,
                                       party_id = rm.party_id,
                                       requisition_date = rm.requisition_date,
                                       delivery_status = rm.delivery_status,
                                       party_name = pt.party_name,
                                       amount = rm.amount,
                                       expected_receiving_date = rm.expected_receiving_date,
                                       warehouse_from = rm.warehouse_from,
                                       warehouse_name = w.warehouse_name,
                                       status = rm.status, //accounts status
                                       finance_status = rm.finance_status, //sales head status
                                       forwarded_status = rm.forwarded_status,
                                       forwarded_by = rm.forwarded_by,
                                       forwarded_date = rm.forwarded_date,
                                      // company_name = com.company_name,
                                       forward_2_status = rm.forward_2_status
                                   }

                    ).OrderByDescending(e => e.requisition_master_id).ToList();
                ;
                return requisition;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        //to load all kind of requisitions for Accounts head forward
        public object GetRequisitionForVerifyByPartyId(long party_id)
        {
            try
            {
                var requisition = (from rm in _entities.requisition_master
                                   join pt in _entities.parties on rm.party_id equals pt.party_id
                                   join w in _entities.warehouses on rm.warehouse_from equals w.warehouse_id
                                   join com in _entities.companies on rm.company_id equals com.company_id into tempCom
                                   from com in tempCom.DefaultIfEmpty()
                                   //where (rm.is_deleted == false && rm.status == "Not Forwarded" && rm.is_demo == null && (rm.forwarded_status != "Forwarded" || rm.forwarded_status == null))
                                   //06.03.2017
                                   where (rm.is_deleted == false && rm.status == "Not Forwarded" && rm.is_demo == null)
                                  
                                   select new
                                   {
                                       requisition_master_id = rm.requisition_master_id,
                                       requisition_code = rm.requisition_code,
                                       created_by = rm.created_by,
                                       created_date = rm.created_date,
                                       party_id = rm.party_id,
                                       requisition_date = rm.requisition_date,
                                       delivery_status = rm.delivery_status,
                                       party_name = pt.party_name,
                                       amount = rm.amount,
                                       expected_receiving_date = rm.expected_receiving_date,
                                       warehouse_from = rm.warehouse_from,
                                       warehouse_name = w.warehouse_name,
                                       status = rm.status,
                                       finance_status = rm.finance_status,
                                       forwarded_status = rm.forwarded_status,
                                       forwarded_by = rm.forwarded_by,
                                       forwarded_date = rm.forwarded_date,
                                       requisition_type = rm.requisition_type,
                                       company_name = com.company_name,
                                       deposite_date = _entities.payment_request.FirstOrDefault(pr => pr.requisition_master_id == rm.requisition_master_id).deposite_date,
                                       //11.05.2017
                                       payment_request_id =
                                           (int?)
                                               _entities.payment_request.FirstOrDefault(
                                                   t => t.requisition_master_id == rm.requisition_master_id).payment_req_id,
                                       check_amount =
                                           _entities.payment_request.FirstOrDefault(
                                               t => t.requisition_master_id == rm.requisition_master_id).amount,
                                       payment_status =
                                           _entities.payment_request.FirstOrDefault(
                                               t => t.requisition_master_id == rm.requisition_master_id).amount == null
                                               ? "Without Payment"
                                               : "With Payment",
                                       check_verify =
                                           _entities.receives.FirstOrDefault(
                                               r =>
                                                   r.payment_req_id ==
                                                   (int?)
                                                       _entities.payment_request.FirstOrDefault(
                                                           t => t.requisition_master_id == rm.requisition_master_id).payment_req_id)
                                               .is_varified,
                                       verify_status =
                                           _entities.receives.FirstOrDefault(
                                               r =>
                                                   r.payment_req_id ==
                                                   (int?)
                                                       _entities.payment_request.FirstOrDefault(
                                                           t => t.requisition_master_id == rm.requisition_master_id).payment_req_id)
                                               .is_varified == true
                                               ? "Verified"
                                               : "Not Verified"

                                       
                                   }

             ).OrderByDescending(e => e.requisition_master_id).ToList()
                    .Select(o => new
                {
                requisition_master_id = o.requisition_master_id,
                requisition_code = o.requisition_code,
                created_by = o.created_by,
                created_date = o.created_date,
                party_id = o.party_id,
                requisition_date = o.requisition_date.ToString(),
                delivery_status = o.delivery_status,
                party_name = o.party_name,
                amount = o.amount,
                expected_receiving_date = o.expected_receiving_date,
                warehouse_from = o.warehouse_from,
                warehouse_name = o.warehouse_name,
                status = o.status,
                finance_status = o.finance_status,
                forwarded_status = o.forwarded_status,
                forwarded_by = o.forwarded_by,
                forwarded_date = o.forwarded_date,
                requisition_type = o.requisition_type,
                company_name = o.company_name,
                payment_request_id = o.payment_request_id,
                check_amount = o.check_amount,
                payment_status = o.payment_status,
                check_verify = o.check_verify,
                verify_status = o.verify_status,
                deposite_date=o.deposite_date
                }
            )
                ; 
                return requisition;

            }
            catch (Exception)
            {
                throw;
            }
        }

        //to load requisition for sales head approval(load after accounts approval)
        //accounts forwarded or HOOps forwarded requisition will eligible for sales head approval
        //status= accounts forward to head of sales and forward_2_status means forwarded by HOOps to sales head
        //06.02.2017
        //status= both accounts forward to HOS and forward to HOOps. if forward to HOS then status will be Fwd to HOS, if forward to HOOps then status will be Fwd to HOOps 
         //forward_2_status means forwarded by HOOps to sales head
        //29.03.2017
        //load dealer requisition && dealer demo requisition for dealer sales head
        //finance_status==sales status
        public object GetRequisitionForApprovalByPartyId(long party_id)
        {
            try
            {
                var requisition = (from rm in _entities.requisition_master
                                   join pt in _entities.parties on rm.party_id equals pt.party_id
                                   join w in _entities.warehouses on rm.warehouse_from equals w.warehouse_id
                                   join com in _entities.companies on rm.company_id equals com.company_id into tempCom
                                   from com in tempCom.DefaultIfEmpty()
                                   //where (rm.is_deleted == false && (rm.status == "Forwarded" || rm.forward_2_status=="Forwarded"))
                                   //06.03.2017
                                   //where (rm.is_deleted == false && (rm.status == "Fwd to HOS" || rm.forward_2_status == "Fwd to HOS" ))
                                   //29.03.2017
                                   //where (rm.is_deleted == false && rm.party_type_id == 4 && rm.finance_status=="Not Approved" && (rm.status == "Fwd to HOS" || rm.forward_2_status == "Fwd to HOS"))
                                   //19.04.2017
                                   //where (rm.is_deleted == false && rm.party_type_id == 4 && rm.finance_status == "Not Approved" && rm.status == "Fwd to HOS")
                                   where
                                       (rm.is_deleted == false && rm.party_type_id == 4 && rm.finance_status == "Not Approved" &&
                                        (rm.status == "Forward to HOS" || rm.status == "Forward to HOS with payment check"))
                                   select new
                                   {
                                       requisition_master_id = rm.requisition_master_id,
                                       requisition_code = rm.requisition_code,
                                       created_by = rm.created_by,
                                       created_date = rm.created_date,
                                       party_id = rm.party_id,
                                       requisition_date = rm.requisition_date,
                                       delivery_status = rm.delivery_status,
                                       party_name = pt.party_name,
                                       amount = rm.amount,
                                       expected_receiving_date = rm.expected_receiving_date,
                                       warehouse_from = rm.warehouse_from,
                                       warehouse_name = w.warehouse_name,
                                       status = rm.status,
                                       finance_status = rm.finance_status,
                                       forwarded_status = rm.forwarded_status,
                                       forwarded_by = rm.forwarded_by,
                                       forwarded_date = rm.forwarded_date,
                                       requisition_type = rm.requisition_type,
                                       forward_2_status = rm.forward_2_status,
                                       company_name = com.company_name,
                                       deposite_date = _entities.payment_request.FirstOrDefault(pr => pr.requisition_master_id == rm.requisition_master_id).deposite_date,
                                       verified_date = rm.verified_date,
                                       approved_date = rm.approved_date,
                                       forward_2_date = rm.forward_2_date,
                                       is_hold=rm.is_hold
                                   }

             ).OrderByDescending(e => e.requisition_master_id).ToList()
                    .Select(o => new
             {
                        requisition_master_id = o.requisition_master_id,
                        requisition_code = o.requisition_code,
                        created_by = o.created_by,
                        created_date = o.created_date,
                        party_id = o.party_id,
                        requisition_date = o.requisition_date.ToString(),
                        delivery_status = o.delivery_status,
                        party_name = o.party_name,
                        amount = o.amount,
                        expected_receiving_date = o.expected_receiving_date,
                        warehouse_from = o.warehouse_from,
                        warehouse_name = o.warehouse_name,
                        status = o.status,
                        finance_status = o.finance_status,
                        forwarded_status = o.forwarded_status,
                        forwarded_by = o.forwarded_by,
                        forwarded_date = o.forwarded_date,
                        requisition_type = o.requisition_type,
                        forward_2_status = o.forward_2_status,
                        company_name = o.company_name,
                        deposite_date = o.deposite_date.ToString() ?? "",
                        verified_date = o.verified_date.ToString() ?? "",
                        approved_date = o.approved_date.ToString() ?? "",
                        forward_2_date = o.forward_2_date.ToString() ?? "",
                        is_hold=o.is_hold

             });
              
                return requisition;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //load all B2B requisition list(after accounts approval) for B2B sales head
        public object GetBtoBRequisitionForApprovalByPartyId(long party_id)
        {
            try
            {
                var requisition = (from rm in _entities.requisition_master
                                   join pt in _entities.parties on rm.party_id equals pt.party_id
                                   join w in _entities.warehouses on rm.warehouse_from equals w.warehouse_id
                                   join com in _entities.companies on rm.company_id equals com.company_id into tempCom
                                   from com in tempCom.DefaultIfEmpty()
                                   //29.03.2017
                                   //where (rm.is_deleted == false && rm.party_type_id == 5 && rm.finance_status == "Not Approved" && rm.status == "Fwd to HOS")
                                   //29.04.2017
                                   where
                                       (rm.is_deleted == false && rm.party_type_id == 5 && rm.finance_status == "Not Approved" &&
                                        (rm.status == "Forward to HOS" || rm.status == "Forward to HOS with payment check"))
                                   select new
                                   {
                                       requisition_master_id = rm.requisition_master_id,
                                       requisition_code = rm.requisition_code,
                                       created_by = rm.created_by,
                                       created_date = rm.created_date,
                                       party_id = rm.party_id,
                                       requisition_date = rm.requisition_date,
                                       delivery_status = rm.delivery_status,
                                       party_name = pt.party_name,
                                       amount = rm.amount,
                                       expected_receiving_date = rm.expected_receiving_date,
                                       warehouse_from = rm.warehouse_from,
                                       warehouse_name = w.warehouse_name,
                                       status = rm.status,
                                       finance_status = rm.finance_status,
                                       forwarded_status = rm.forwarded_status,
                                       forwarded_by = rm.forwarded_by,
                                       forwarded_date = rm.forwarded_date,
                                       requisition_type = rm.requisition_type,
                                       forward_2_status = rm.forward_2_status,
                                       company_name = com.company_name,
                                       is_hold=rm.is_hold
                                   }

                    ).OrderByDescending(e => e.requisition_master_id).ToList();
                ;
                return requisition;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public decimal GetPaidAmtofRequisition(long requisition_master_id)
        {
            try
            {
                decimal paidAmt = 0;

                var paid_amt =
                    _entities.payment_request.FirstOrDefault(w => w.requisition_master_id == requisition_master_id);
                if (paid_amt != null)
                {
                    paidAmt = paid_amt.amount ?? 0;
                }

                return paidAmt;

            }
            catch
            {
                return 0;
            }
        }

        public object GetPaymentVerifyStatus(long requisition_master_id)
        {
            try
            {
                //var paymentSts = "";
                bool paymentSts;
                var paymentStatus =
                    _entities.payment_request.FirstOrDefault(w => w.requisition_master_id == requisition_master_id);
                if (paymentStatus != null)
                {
                    paymentSts = (bool)paymentStatus.approved;
                    return paymentSts;
                }
                return null;
            }
            catch
            {
                //return "false";
                return null;
            }
        }

        public string GetPaymentStatus(long requisition_master_id)
        {
            try
            {
                //var amount="";
                var Sts = "";

                var paymentStatus = (from pq in _entities.payment_request
                                     join rcv in _entities.receives on pq.payment_req_id equals rcv.payment_req_id into temprcv
                                     from rcv in temprcv.DefaultIfEmpty()
                    select new
                    {
                        amount = pq.amount,
                                         requisition_master_id = pq.requisition_master_id,
                        is_varified = rcv.is_varified
                                     }).Where(w => w.is_varified == true && w.requisition_master_id == requisition_master_id)
                    .FirstOrDefault();
                if (paymentStatus != null)
                {
                    Sts = "PAYMENT VERIFIED";
                }
                else
                {
                    Sts = "WITHOUT PAYMENT";
                }
                return Sts;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public decimal GetPartyCreditLimit(long party_id)
        //{
        //    try
        //    {
        //        decimal creditLimit = 0;
        //        //var opening_balance = _entities.party_journal.Where(w => w.party_id == party_id).OrderByDescending(o => o.party_journal_id).FirstOrDefault();
        //        var credit_limit = _entities.parties.Find(party_id);
        //        if (credit_limit != null)
        //        {
        //            creditLimit = credit_limit.credit_limit ?? 0;
        //        }

        //        return creditLimit;

        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //Requisition Report
        public object GetRequisitionReportById(long requisition_master_id)
        {
            try
            {
                var requisitionCombined = new RequisitionCombinedModel();
               
                decimal totalRebateBforSave = 0;

                string query =
                    "SELECT pom.requisition_master_id,company.company_name, pro.product_name, col.color_name, pod.quantity, pod.price, u.unit_name, pod.is_gift, pom.status, " +
                               " pom.requisition_date, pom.remarks, pom.requisition_code, usr.full_name,pod.line_total, pom.amount, pom.party_id, " +
                               " (select party_name from party where party_id=pom.party_id) as party, (select party.address from party where party_id=pom.party_id) as party_address, " +
                               " (select party.mobile from party where party_id=pom.party_id) as party_mobile, (select party_type_name from party_type inner join party on party_type.party_type_id=party.party_type_id where party.party_id=pom.party_id) as party_type_name, " +
                               " (select party_prefix from party_type inner join party on party_type.party_type_id=party.party_type_id where party.party_id=pom.party_id) as party_prefix, " +
                    " CAST(" + totalRebateBforSave +
                    " AS numeric) as totalRebateBforSave,  pv.product_version_name, pod.is_gift, pod.gift_type, pod.discount_amount, pod.discount " +
                               " FROM requisition_details pod " +
                               " INNER JOIN requisition_master pom ON pod.requisition_master_id = pom.requisition_master_id " +
                               " INNER JOIN product pro ON pod.product_id = pro.product_id " +
                               " LEFT JOIN color col ON pod.color_id = col.color_id left join unit u on  pod.unit_id=u.unit_id " +
                    " LEFT JOIN product_version pv on pod.product_version_id=pv.product_version_id " +
                               " LEFT JOIN users usr ON pom.created_by = usr.user_id " +
                               " LEFT JOIN company on pom.company_id=company.company_id " +
                               " WHERE pod.requisition_master_id = " + requisition_master_id + " order by col.color_name";
                var reData = _entities.Database.SqlQuery<RequisitionReportModel>(query).ToList();
                var list1 = new List<RequisitionReportModel>(reData);

                //subreport for incentive
                //string queryIncentive = " select payment_method.payment_method_name,  (CASE WHEN payment_method.payment_method_name='Cash' THEN 'Cash' when payment_method.payment_method_name='Cheque' then 'Cheque' else bank.bank_name END) as bank_name, receive.receive_date, receive.amount as receivedAmount, (CASE WHEN receive.cheque_no='' THEN 'DC:' ELSE 'DC:'  END) as cheque_no from receive left join payment_method on receive.payment_method_id=payment_method.payment_method_id left join bank on receive.bank_id=bank.bank_id where received_invoice_no ='" + invcNo + "' and party_id='" + partyId + "'";
                //var incentiveData = _entities.Database.SqlQuery<ReceivedBreakdownModel>(queryIncentive);
                //var list3 = new List<ReceivedBreakdownModel>(incentiveData);

                requisitionCombined.RequisitionReportModels = list1;
                
              
                return requisitionCombined;
            }
            catch (Exception)
            {

                throw;
            }

        }


        public class DetailsForReportModel
        {
            public int requisition_master_id { get; set; }
            public int requisition_details_id { get; set; }
            public int product_id { get; set; }
            public string product_name { get; set; }
            public int unit_id { get; set; }
            public int brand_id { get; set; }
            public int product_category_id { get; set; }
            public decimal price { get; set; }
            public int quantity { get; set; }
            public int color_id { get; set; }
        }



        /// <summary>
        /// Modified by: Maruf
        /// </summary>
        /// <param name="requisition_master_id"></param>
        /// <returns></returns>
        public RequisitionModel GetRequisitionByIdForDelivery(long requisition_master_id)
        {
            try
            {
                var requisitionModel = new RequisitionModel();
                requisitionModel.RequisitionMasterData = _entities.requisition_master.Find(requisition_master_id);
                requisitionModel.RequisitionDetailsList =
                    (
                        from rd in _entities.requisition_details
                        join p in _entities.products on rd.product_id equals p.product_id
                        join pc in _entities.product_category on p.product_category_id equals pc.product_category_id
                            into proCat
                        from pc in proCat.DefaultIfEmpty()
                        join b in _entities.brands on rd.brand_id equals b.brand_id into brnd
                        from b in brnd.DefaultIfEmpty()
                        join pv in _entities.product_version on rd.product_version_id equals pv.product_version_id into
                            verTemp
                        from ver in verTemp.DefaultIfEmpty()
                        join c in _entities.colors on rd.color_id equals c.color_id into colorTmp
                        from col in colorTmp.DefaultIfEmpty()
                        join u in _entities.units on rd.unit_id equals u.unit_id into uit
                        from u in uit.DefaultIfEmpty()
                        join com in _entities.companies on requisitionModel.RequisitionMasterData.company_id equals
                            com.company_id into comTemp
                        from com in comTemp.DefaultIfEmpty()
                        where (rd.requisition_master_id == requisition_master_id)
                        select new RequisitionDetailsModel
                        {
                            requisition_master_id = rd.requisition_master_id,
                            requisition_details_id = rd.requisition_details_id,
                            is_gift = rd.is_gift,
                            gift_type = rd.gift_type,
                            is_live_dummy = rd.is_live_dummy,
                            product_id = rd.product_id,
                            product_name = p.product_name,
                            product_version_id = ver.product_version_id,
                            product_version_name = ver.product_version_name,
                            brand_id = rd.brand_id,
                            product_category_id = pc.product_category_id,
                            color_id = col.color_id,
                            color_name = col.color_name,
                            unit_id = rd.unit_id,
                            unit_name = u.unit_name,
                            quantity = rd.quantity,
                            price = rd.price,
                            delivered_quantity = rd.delivered_quantity,
                            line_total = rd.line_total,
                            company_name = com.company_name,
                            has_serial = p.has_serial ?? false,
                            discount_amount = rd.discount_amount
                        }
                        ).OrderBy(req => req.requisition_details_id).ToList();
                return requisitionModel;

            }
            catch (Exception)
            {

                throw;
            }


        }

        // Party Type Wise Product Price : Kiron:17-12-2016
        public object GetProductPriceByPartyTypeId(long party_type_id, long product_id)
        {
            try
            {
                //MD
                if (party_type_id == 2)
                {
                    var price = _entities.products.Find(product_id);
                    return price.md_price;

                }
                //DBIS
                if (party_type_id == 3)
                {
                    var price = _entities.products.Find(product_id);
                    return price.bs_price;
                }

                //Central
                if (party_type_id == 1)
                {
                    var price = _entities.products.Find(product_id);
                    return price.rp_price;
                }

                //Other Price
                if (party_type_id == 4)
                {
                    var price = _entities.products.Find(product_id);
                    return price.rp_price;
                }


                return "";
            }
            catch (Exception)
            {

                return 0;
            }

        }

        public bool cancelRequisition(long requisition_master_id, long userid)
        {
            try
            {
                requisition_master rqm = _entities.requisition_master.Find(requisition_master_id);
                rqm.is_deleted = true;
                rqm.updated_by = userid;
                rqm.updated_date = DateTime.Now;
                _entities.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public bool cancelRequisitionWithReason(long requisition_master_id, long userid, string reason_for_cancel_hold)
        {
            requisition_master rqm = _entities.requisition_master.Find(requisition_master_id);
            rqm.is_deleted = true;
            rqm.updated_by = userid;
            rqm.updated_date = DateTime.Now;
            rqm.reason_for_cancel_hold = reason_for_cancel_hold;

            _entities.SaveChanges();

            //06.07.2017
            // Send Email Nofication When Updated
            //Get Mail Data

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
                                }).Where(c => c.is_deleted != true && c.process_code_name == "REQUISITION DECLINE")
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
                               }).FirstOrDefault(c => c.is_deleted != true && c.process_code_name == "REQUISITION DECLINE");



            //GET REQUISITION INFO
            var requsitionInfo =
                GetRequsitionInformatioForEmailNotificationById(requisition_master_id);
            //GET WHO IS UPDATING
            var approvedBy =
                _entities.users.Where(d => d.user_id == userid)
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
            sb.Append("<td><b>DECLINE BY </b></td>");
            sb.Append("<td><b>: " + approvedBy + "</b></td>");
            sb.Append("</tr>");
            sb.Append("<tr>");
            sb.Append("<td><b>DECLINE DATE</b></td>");
            sb.Append("<td><b>: " + DateTime.Now + "</b></td>");
            sb.Append("</tr>");

            sb.Append("<tr>");
            sb.Append("<td ><b>HOLD REASON</b></td>");
            sb.Append("<td><b>: <span style='color: red'>" + requsitionInfo[0].reason_for_cancel_hold + "</b></td>");
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

            return true;
        }

        public bool holdRequisitionWithReason(long requisition_master_id, long userid, string reason_for_cancel_hold)
        {
            requisition_master rqm = _entities.requisition_master.Find(requisition_master_id);
            rqm.is_hold = true;
            rqm.updated_by = userid;
            rqm.updated_date = DateTime.Now;
            rqm.reason_for_cancel_hold = reason_for_cancel_hold;

            _entities.SaveChanges();

            // Send Email Nofication When Updated
            //Get Mail Data
          
           
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
                                    }).Where(c => c.is_deleted != true && c.process_code_name == "REQUISITION HOLD")
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
                                   }).FirstOrDefault(c => c.is_deleted != true && c.process_code_name == "REQUISITION HOLD");



                //GET REQUISITION INFO
                var requsitionInfo =
                    GetRequsitionInformatioForEmailNotificationById(requisition_master_id);
                //GET WHO IS UPDATING
                var approvedBy =
                    _entities.users.Where(d => d.user_id == userid)
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
                sb.Append("<td><b>HOLD BY </b></td>");
                sb.Append("<td><b>: " + approvedBy + "</b></td>");
                sb.Append("</tr>");
                sb.Append("<tr>");
                sb.Append("<td><b>HOLD DATE</b></td>");
                sb.Append("<td><b>: " + DateTime.Now + "</b></td>");
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append("<td><b>HOLD REASON</b></td>");
                sb.Append("<td><b>: <span style='color: red'>" + requsitionInfo[0].reason_for_cancel_hold + "</b></td>");
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


            

            return true;
        }
        public List<RequisitionUpdateEmailNotificationModel> GetRequsitionInformatioForEmailNotificationById(long masterId)
        {

            string query = "select rm.requisition_code ,rm.requisition_date, rm.requisition_type, rm.requisition_master_id, rm.reason_for_cancel_hold, pro.product_name ,col.color_name ,pv.product_version_name ,pc.product_category_name ,p.party_name ,p.address ,pt.party_type_name as party_type,rd.quantity ,rd.discount_amount ,rd.line_total ,us.full_name as approved_by ,rm.approved_date from requisition_master rm left join requisition_details rd on rm.requisition_master_id=rd.requisition_master_id left join product pro on rd.product_id=pro.product_id left join color col on rd.color_id = col.color_id left join product_version pv on rd.product_version_id=pv.product_version_id left join product_category pc on pro.product_category_id = pc.product_category_id left join party p on rm.party_id =p.party_id left join party_type pt on p.party_type_id =pt.party_type_id left join users us on rm.approved_by =us.user_id where rm.requisition_master_id='" + masterId + "'";
            var requsitionUpdateEmailData = _entities.Database.SqlQuery<RequisitionUpdateEmailNotificationModel>(query).ToList();

            return requsitionUpdateEmailData;
        }

        //proceed to HOOps if credit limit exceed.
        //accounts head will forward to HOOps if credit limit exceed.
        public bool ProceedToHOS(long requisition_master_id, long userid)
        {
            try
            {
                requisition_master rqmaster = _entities.requisition_master.Find(requisition_master_id);
                rqmaster.forwarded_by = userid;
                rqmaster.forwarded_date = DateTime.Now;
                //rqmaster.forwarded_status = "Forwarded";
                //06.02.2017
                rqmaster.status = "Fwd to HOOps";
                _entities.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                
                throw;
            }
        }




        public object GetPromotionRequisitionById(long requisition_master_id)
        {
            try
            {
                string query =
                    "select V.promotion_details_id,V.promotion_master_id,V.gift_type,V.quantity,V.requisition_details_id, product.product_name,V.product_id,V.color_id,color.color_name,V.product_version_id,product_version.product_version_name,V.product_category_id,V.brand_id,V.unit_id from (select distinct B.promotion_details_id,A.product_id,B.promotion_master_id,A.color_id,A.product_version_id ,A.quantity,A.gift_type,A.requisition_details_id,A.product_category_id,A.unit_id,A.brand_id from (select * from requisition_details as r where r.product_id in (select product_id from promotion_details as p where p.promotion_master_id IN (select promotion_master_id from requisition_details where requisition_master_id = " +
                    requisition_master_id + " and (promotion_master_id != 0))) and r.requisition_master_id = " +
                    requisition_master_id +
                    " and r.gift_type = 'Promotional Gift')A , (select * from promotion_details as p where p.promotion_master_id IN (select promotion_master_id from requisition_details where requisition_master_id = " +
                    requisition_master_id +
                    " and (promotion_master_id != 0)))B where A.product_id = B.product_id)V inner join product on V.product_id = product.product_id left join color on V.color_id = color.color_id left join product_version on V.product_version_id = product_version.product_version_id";

                var reData = _entities.Database.SqlQuery<PromotionLoadModel>(query).ToList();

                return reData;
            }
            catch (Exception)
            {
       
                throw;
            }
        }


        // Added By Kiron: 08-06-2017 : As Per Maruf Vai Said

        public object GetAllHOSApprovedRequisitionList(long userId)
        {
            // Get Role From User Id Then If : Else

            try
            {
                var requisition = (from rm in _entities.requisition_master
                                   join pt in _entities.parties on rm.party_id equals pt.party_id
                                   join w in _entities.warehouses on rm.warehouse_from equals w.warehouse_id
                                   join com in _entities.companies on rm.company_id equals com.company_id into temCom
                                   from com in temCom.DefaultIfEmpty()
                                   where rm.finance_status == "Approved"

                                   select new
                                   {
                                       requisition_master_id = rm.requisition_master_id,
                                       requisition_code = rm.requisition_code,
                                       created_by = rm.created_by,
                                       created_date = rm.created_date,
                                       party_id = rm.party_id,
                                       requisition_date = rm.requisition_date,
                                       delivery_status = rm.delivery_status,
                                       party_name = pt.party_name,
                                       amount = rm.amount,
                                       expected_receiving_date = rm.expected_receiving_date,
                                       warehouse_from = rm.warehouse_from,
                                       warehouse_name = w.warehouse_name,
                                       status = rm.status,
                                       finance_status = rm.finance_status,
                                       requisition_type = rm.requisition_type,
                                       company_name = com.company_name,
                                       forward_2_status = rm.forward_2_status
                                   }

               ).OrderByDescending(e => e.requisition_master_id).ToList()
               .Select(o => new
               {
                   requisition_master_id = o.requisition_master_id,
                   requisition_code = o.requisition_code,
                   created_by = o.created_by,
                   created_date = o.created_date,
                   party_id = o.party_id,
                   requisition_date = o.requisition_date.ToString(),
                   delivery_status = o.delivery_status,
                   party_name = o.party_name,
                   amount = o.amount,
                   expected_receiving_date = o.expected_receiving_date,
                   warehouse_from = o.warehouse_from,
                   warehouse_name = o.warehouse_name,
                   status = o.status,
                   finance_status = o.finance_status,
                   requisition_type = o.requisition_type,
                   company_name = o.company_name,
                   forward_2_status = o.forward_2_status
               });


                return requisition;
        }
            catch (Exception ex)
            {

                throw ex;
            }
        
    }


    public class ActivePromotion
    {
        public int promotion_master_id { get; set; }
        public string promotion_description { get; set; }
    }

        // Added By Kiron: 08-06-2017 : As Per Maruf Vai Said
        public object GetAllOPHsApprovedRequisitionList(long userId)
        {
            try
            {
                var requisition = (from rm in _entities.requisition_master
                                   join pt in _entities.parties on rm.party_id equals pt.party_id
                                   join w in _entities.warehouses on rm.warehouse_from equals w.warehouse_id
                                   join com in _entities.companies on rm.company_id equals com.company_id into temCom
                                   from com in temCom.DefaultIfEmpty()
                                   where rm.forward_2_status == "Approved"

                                   select new
                                   {
                                       requisition_master_id = rm.requisition_master_id,
                                       requisition_code = rm.requisition_code,
                                       created_by = rm.created_by,
                                       created_date = rm.created_date,
                                       party_id = rm.party_id,
                                       requisition_date = rm.requisition_date,
                                       delivery_status = rm.delivery_status,
                                       party_name = pt.party_name,
                                       amount = rm.amount,
                                       expected_receiving_date = rm.expected_receiving_date,
                                       warehouse_from = rm.warehouse_from,
                                       warehouse_name = w.warehouse_name,
                                       status = rm.status,
                                       finance_status = rm.finance_status,
                                       requisition_type = rm.requisition_type,
                                       company_name = com.company_name,
                                       forward_2_status = rm.forward_2_status
                                   }

                    ).OrderByDescending(e => e.requisition_master_id).ToList()
                    .Select(o => new
                    {
                        requisition_master_id = o.requisition_master_id,
                        requisition_code = o.requisition_code,
                        created_by = o.created_by,
                        created_date = o.created_date,
                        party_id = o.party_id,
                        requisition_date = o.requisition_date.ToString(),
                        delivery_status = o.delivery_status,
                        party_name = o.party_name,
                        amount = o.amount,
                        expected_receiving_date = o.expected_receiving_date,
                        warehouse_from = o.warehouse_from,
                        warehouse_name = o.warehouse_name,
                        status = o.status,
                        finance_status = o.finance_status,
                        requisition_type = o.requisition_type,
                        company_name = o.company_name,
                        forward_2_status = o.forward_2_status
                    });


                return requisition;
            }
            catch (Exception ex)
            {
                
                throw ex;
            }


        }

       
    }
}