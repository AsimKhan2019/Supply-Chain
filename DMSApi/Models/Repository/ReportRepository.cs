﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DMSApi.Models.crystal_models;
using DMSApi.Models.IRepository;
using DMSApi.Models.StronglyType;

namespace DMSApi.Models.Repository
{
    public class ReportRepository : IReportRepository
    {
        private DMSEntities _entities;

        public ReportRepository()
        {
            _entities = new DMSEntities();

        }
        public object GetAccountsDueAdvanceReport(long partyId, long regoinId, long areaId, long partyTypeId)
        {
            try
            {
                if (partyId == 0 && regoinId == 0 && areaId == 0 && partyTypeId == 0)
                {
                    string query = "select * from (select party.party_id ,party_type.party_type_name ,party.party_name ,party.party_code ,region.region_name ,area.area_name ,territory.territory_name ,(select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc) as closing_balance ,(select sum(invoice_total)as invoice_total from invoice_master inner join party p on invoice_master.party_id=p.party_id where invoice_master.party_id=party.party_id group by p.party_id) as invoice_total,(select top 1 pj.opening_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id asc) as opening_balance ,(select sum(amount)as invoice_total from receive inner join party p on receive.party_id=p.party_id where receive.party_id=party.party_id group by p.party_id) as received_amount, ( CASE WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc) >=0 THEN 'Due' WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc)=0 THEN 'Full Paid' WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc) <=0 THEN 'Advance' END ) AS remain_balance_status from party inner join party_type on party.party_type_id = party_type.party_type_id inner join region on party.region_id = region.region_id inner join area on party.area_id = area.area_id inner join territory on party.territory_id= territory.territory_id where party.is_active=1 )A where A.closing_balance != 0 order by A.region_name";
                    var advanceDue = _entities.Database.SqlQuery<AdvanceDueReportModel>(query).ToList();
                    return advanceDue;

                }
                if (partyId != 0 && regoinId != 0 && areaId != 0 && partyTypeId != 0)
                {
                    string query = "select * from (select party.party_id ,party_type.party_type_name ,party.party_name ,party.party_code ,region.region_name ,area.area_name ,territory.territory_name ,(select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc ) as closing_balance,(select sum(invoice_total)as invoice_total from invoice_master inner join party p on invoice_master.party_id=p.party_id where invoice_master.party_id=party.party_id group by p.party_id) as invoice_total,(select top 1 pj.opening_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id asc) as opening_balance ,(select sum(amount)as invoice_total from receive inner join party p on receive.party_id=p.party_id where receive.party_id=party.party_id group by p.party_id) as received_amount, ( CASE WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc) >=0 THEN 'Due' WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc)=0 THEN 'Full Paid' WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc) <=0 THEN 'Advance' END ) AS remain_balance_status from party inner join party_type on party.party_type_id = party_type.party_type_id inner join region on party.region_id = region.region_id inner join area on party.area_id = area.area_id inner join territory on party.territory_id= territory.territory_id where party.is_active=1 and party.party_id='" + partyId + "')A where A.closing_balance != 0  order by A.region_name";
                    var advanceDue = _entities.Database.SqlQuery<AdvanceDueReportModel>(query).ToList();
                    return advanceDue;

                }
                if (regoinId != 0 && areaId == 0)
                {
                    string queryRegion = "select * from (select party.party_id ,party_type.party_type_name ,party.party_name ,party.party_code ,region.region_name ,area.area_name ,territory.territory_name ,(select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc ) as closing_balance,(select sum(invoice_total)as invoice_total from invoice_master inner join party p on invoice_master.party_id=p.party_id where invoice_master.party_id=party.party_id group by p.party_id) as invoice_total,(select top 1 pj.opening_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id asc) as opening_balance ,(select sum(amount)as invoice_total from receive inner join party p on receive.party_id=p.party_id where receive.party_id=party.party_id group by p.party_id) as received_amount, ( CASE WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc) >=0 THEN 'Due' WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc)=0 THEN 'Full Paid' WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc) <=0 THEN 'Advance' END ) AS remain_balance_status from party inner join party_type on party.party_type_id = party_type.party_type_id inner join region on party.region_id = region.region_id inner join area on party.area_id = area.area_id inner join territory on party.territory_id= territory.territory_id where party.is_active=1 and party.region_id='" + regoinId + "' )A where A.closing_balance != 0 order by A.region_name";
                    var advanceDueRegion = _entities.Database.SqlQuery<AdvanceDueReportModel>(queryRegion).ToList();
                    return advanceDueRegion;
                }
                if (regoinId != 0 && areaId != 0 && partyTypeId != 0)
                {
                    string queryRegion = "select * from (select party.party_id ,party_type.party_type_name ,party.party_name ,party.party_code ,region.region_name ,area.area_name ,territory.territory_name ,(select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc ) as closing_balance,(select sum(invoice_total)as invoice_total from invoice_master inner join party p on invoice_master.party_id=p.party_id where invoice_master.party_id=party.party_id group by p.party_id) as invoice_total ,(select top 1 pj.opening_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id asc) as opening_balance ,(select sum(amount)as invoice_total from receive inner join party p on receive.party_id=p.party_id where receive.party_id=party.party_id group by p.party_id) as received_amount, ( CASE WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc) >=0 THEN 'Due' WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc)=0 THEN 'Full Paid' WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc) <=0 THEN 'Advance' END ) AS remain_balance_status from party inner join party_type on party.party_type_id = party_type.party_type_id inner join region on party.region_id = region.region_id inner join area on party.area_id = area.area_id inner join territory on party.territory_id= territory.territory_id where party.is_active=1 and party.area_id='" + areaId + "' and party.party_type_id='" + partyTypeId + "' )A where A.closing_balance != 0 order by A.region_name";
                    var advanceDueRegion = _entities.Database.SqlQuery<AdvanceDueReportModel>(queryRegion).ToList();
                    return advanceDueRegion;
                }
                if (regoinId != 0 && areaId != 0)
                {
                    string queryRegion = "select * from (select party.party_id ,party_type.party_type_name ,party.party_name ,party.party_code ,region.region_name ,area.area_name ,territory.territory_name ,(select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc ) as closing_balance,(select sum(invoice_total)as invoice_total from invoice_master inner join party p on invoice_master.party_id=p.party_id where invoice_master.party_id=party.party_id group by p.party_id) as invoice_total,(select top 1 pj.opening_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id asc) as opening_balance ,(select sum(amount)as invoice_total from receive inner join party p on receive.party_id=p.party_id where receive.party_id=party.party_id group by p.party_id) as received_amount, ( CASE WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc) >=0 THEN 'Due' WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc)=0 THEN 'Full Paid' WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc) <=0 THEN 'Advance' END ) AS remain_balance_status from party inner join party_type on party.party_type_id = party_type.party_type_id inner join region on party.region_id = region.region_id inner join area on party.area_id = area.area_id inner join territory on party.territory_id= territory.territory_id where party.is_active=1 and party.area_id='" + areaId + "' )A where A.closing_balance != 0 order by A.region_name";
                    var advanceDueRegion = _entities.Database.SqlQuery<AdvanceDueReportModel>(queryRegion).ToList();
                    return advanceDueRegion;
                }
                if (areaId != 0)
                {
                    string queryArea = "select party.party_id ,party_type.party_type_name ,party.party_name ,party.party_code ,region.region_name ,area.area_name ,territory.territory_name ,(select top 1 -pj.closing_balance from party_journal pj inner join party on pj.party_id=party.party_id order by party_journal_id desc ) as closing_balance,(select sum(invoice_total)as invoice_total from invoice_master inner join party p on invoice_master.party_id=p.party_id where invoice_master.party_id=party.party_id group by p.party_id) as invoice_total,(select top 1 pj.opening_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id asc) as opening_balance ,(select sum(amount)as invoice_total from receive inner join party p on receive.party_id=p.party_id where receive.party_id=party.party_id group by p.party_id) as received_amount from party inner join party_type on party.party_type_id = party_type.party_type_id inner join region on party.region_id = region.region_id inner join area on party.area_id = area.area_id inner join territory on party.territory_id= territory.territory_id where party.is_active=1 and party.area_id='" + areaId + "' order by territory.territory_name";
                    var advanceDueArea = _entities.Database.SqlQuery<AdvanceDueReportModel>(queryArea).ToList();
                    return advanceDueArea;
                }
                if (partyTypeId != 0 && partyId == 0)
                {
                    string queryPartyType = "select * from (select party.party_id ,party_type.party_type_name ,party.party_name ,party.party_code ,region.region_name ,area.area_name ,territory.territory_name ,(select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc ) as closing_balance,(select sum(invoice_total)as invoice_total from invoice_master inner join party p on invoice_master.party_id=p.party_id where invoice_master.party_id=party.party_id group by p.party_id) as invoice_total,(select top 1 pj.opening_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id asc) as opening_balance ,(select sum(amount)as invoice_total from receive inner join party p on receive.party_id=p.party_id where receive.party_id=party.party_id group by p.party_id) as received_amount, ( CASE WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc) >=0 THEN 'Due' WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc)=0 THEN 'Full Paid' WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc) <=0 THEN 'Advance' END ) AS remain_balance_status from party inner join party_type on party.party_type_id = party_type.party_type_id inner join region on party.region_id = region.region_id inner join area on party.area_id = area.area_id inner join territory on party.territory_id= territory.territory_id where party.is_active=1 and  party.party_type_id='" + partyTypeId + "' )A where A.closing_balance != 0 order by A.region_name";
                    var advanceDueAreaPartyType = _entities.Database.SqlQuery<AdvanceDueReportModel>(queryPartyType).ToList();
                    return advanceDueAreaPartyType;
                }
                if (partyTypeId != 0 && partyId != 0)
                {
                    string queryPartyType = "select * from (select party.party_id ,party_type.party_type_name ,party.party_name ,party.party_code ,region.region_name ,area.area_name ,territory.territory_name ,(select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc ) as closing_balance,(select sum(invoice_total)as invoice_total from invoice_master inner join party p on invoice_master.party_id=p.party_id where invoice_master.party_id=party.party_id group by p.party_id) as invoice_total,(select top 1 pj.opening_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id asc) as opening_balance ,(select sum(amount)as invoice_total from receive inner join party p on receive.party_id=p.party_id where receive.party_id=party.party_id group by p.party_id) as received_amount, ( CASE WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc) >=0 THEN 'Due' WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc)=0 THEN 'Full Paid' WHEN (select top 1 -pj.closing_balance from party_journal pj where pj.party_id=party.party_id order by party_journal_id desc) <=0 THEN 'Advance' END ) AS remain_balance_status from party inner join party_type on party.party_type_id = party_type.party_type_id inner join region on party.region_id = region.region_id inner join area on party.area_id = area.area_id inner join territory on party.territory_id= territory.territory_id where party.is_active=1 and  party.party_type_id='" + partyTypeId + "'  and party.party_id='" + partyId + "')A where A.closing_balance != 0 order by A.region_name";
                    var advanceDueAreaPartyType = _entities.Database.SqlQuery<AdvanceDueReportModel>(queryPartyType).ToList();
                    return advanceDueAreaPartyType;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public object GetDailySalesReport(string fromDate, string toDate, long productCategoryId, long productId)
        {
            //try
            //{

            //    DateTime dateFrom = Convert.ToDateTime(fromDate);
            //    var fromDateAsString = dateFrom;

            //    DateTime dataTo = Convert.ToDateTime(toDate);

            //    var toDateAsString = dataTo.AddDays(1);
            //    if (fromDate !="" && toDate != "" && productCategoryId==0)
            //    {
            //        var data = _entities.viewDailySales.Where(d => d.delivery_date >= fromDateAsString && d.delivery_date <= toDateAsString).ToList();
            //        return data;
            //    }
            //    if (fromDate !="" && toDate != "" && productCategoryId !=0 && productId==0)
            //    {
            //        var data = _entities.viewDailySales.Where(d => d.delivery_date >= fromDateAsString && d.delivery_date <= toDateAsString && d.product_category_id==productCategoryId).ToList();
            //        return data;
            //    }
            //    if (fromDate != "" && toDate != "" && productId != 0)
            //    {
            //        var data = _entities.viewDailySales.Where(d => d.delivery_date >= fromDateAsString && d.delivery_date <= toDateAsString && d.product_id == productId).ToList();
            //        return data;
            //    }
            //    return null;
            //}
            //catch (Exception)
            //{
            return null;
            //}
        }


        public object GetDailySalesReportPDF(string fromDate, string toDate, long productCategoryId, long productId)
        {
            try
            {

                if (fromDate != "" && toDate != "" && productCategoryId == 0)
                {
                    string queryWithFromDateToDate = "select distinct im.invoice_no ,im.invoice_date as delivery_date ,pt.party_type_name ,p.party_name ,c.color_name ,pv.product_version_name ,t.territory_name ,r.region_name ,a.area_name , pc.product_category_name ,pro.product_name ,id.quantity as delivered_quantity ,id.price as unit_price ,(id.quantity*id.price) as total_price ,cast ((id.discount_amount/id.quantity) as decimal) as per_incentive  ,(id.discount_amount *id.quantity) as total_incentive_amt ,((id.price-id.discount_amount)*id.quantity)as total_price_after_incen ,im.invoice_total ,pr.amount as paid_amount , (pr.amount-im.invoice_total) as balance_amount , ( CASE WHEN pr.amount-invoice_total >=0 THEN 'Advance' WHEN pr.amount-invoice_total =0 THEN 'Full Paid' WHEN pr.amount-invoice_total <=0 THEN 'Due' ELSE 'No Transaction' END) AS remain_balance_status FROM invoice_master im inner join party p on im.party_id = p.party_id inner join party_type pt on p.party_type_id=pt.party_type_id inner join territory t on p.territory_id = t.territory_id inner join area a on p.area_id = a.area_id inner join region r on p.region_id = r.region_id left join invoice_details id on im.invoice_master_id=id.invoice_master_id inner join product pro on id.product_id = pro.product_id left join color c on id.color_id=c.color_id left join product_version pv on id.product_version_id = pv.product_version_id inner join product_category pc on pro.product_category_id= pc.product_category_id left join internal_requisition_master intrm on im.requisition_master_id=intrm.internal_requisition_master_id and im.party_id=78 left join payment_request pr on im.party_id =pr.party_id and im.requisition_master_id=pr.requisition_master_id where im.invoice_date BETWEEN '2017-05-01' AND DATEADD(DAY,1,'2017-06-05' )";

                    var data = _entities.Database.SqlQuery<DailySalesDeliveryModel>(queryWithFromDateToDate).ToList();
                    return data;
                }
                if (fromDate != "" && toDate != "" && productCategoryId != 0 && productId == 0)
                {
                    string queryWithFromDateToDate = "select distinct im.invoice_no ,im.invoice_date as delivery_date ,pt.party_type_name ,p.party_name ,c.color_name ,pv.product_version_name ,t.territory_name ,r.region_name ,a.area_name , pc.product_category_name ,pro.product_name ,id.quantity as delivered_quantity,id.price as unit_price ,(id.quantity*id.price) as total_price ,cast ((id.discount_amount/id.quantity) as decimal) as per_incentive  ,(id.discount_amount *id.quantity) as total_incentive_amt ,((id.price-id.discount_amount)*id.quantity)as total_price_after_incen ,im.invoice_total ,pr.amount as paid_amount , (pr.amount-im.invoice_total) as balance_amount , ( CASE WHEN pr.amount-invoice_total >=0 THEN 'Advance' WHEN pr.amount-invoice_total =0 THEN 'Full Paid' WHEN pr.amount-invoice_total <=0 THEN 'Due' ELSE 'No Transaction' END) AS remain_balance_status FROM invoice_master im inner join party p on im.party_id = p.party_id inner join party_type pt on p.party_type_id=pt.party_type_id inner join territory t on p.territory_id = t.territory_id inner join area a on p.area_id = a.area_id inner join region r on p.region_id = r.region_id left join invoice_details id on im.invoice_master_id=id.invoice_master_id inner join product pro on id.product_id = pro.product_id left join color c on id.color_id=c.color_id left join product_version pv on id.product_version_id = pv.product_version_id inner join product_category pc on pro.product_category_id= pc.product_category_id left join internal_requisition_master intrm on im.requisition_master_id=intrm.internal_requisition_master_id and im.party_id=78 left join payment_request pr on im.party_id =pr.party_id and im.requisition_master_id=pr.requisition_master_id where im.invoice_date BETWEEN '" + fromDate + "' AND DATEADD(DAY,1,'" + toDate + "' ) and id.product_category_id='" + productCategoryId + "'";

                    var data = _entities.Database.SqlQuery<DailySalesDeliveryModel>(queryWithFromDateToDate).ToList();
                    return data;
                }
                if (fromDate != "" && toDate != "" && productId != 0)
                {
                    string queryWithFromDateToDate = "select distinct im.invoice_no ,im.invoice_date as delivery_date ,pt.party_type_name ,p.party_name ,c.color_name ,pv.product_version_name ,t.territory_name ,r.region_name ,a.area_name , pc.product_category_name ,pro.product_name ,id.quantity as delivered_quantity,id.price as unit_price ,(id.quantity*id.price) as total_price ,cast ((id.discount_amount/id.quantity) as decimal) as per_incentive  ,(id.discount_amount *id.quantity) as total_incentive_amt ,((id.price-id.discount_amount)*id.quantity)as total_price_after_incen ,im.invoice_total ,pr.amount as paid_amount , (pr.amount-im.invoice_total) as balance_amount , ( CASE WHEN pr.amount-invoice_total >=0 THEN 'Advance' WHEN pr.amount-invoice_total =0 THEN 'Full Paid' WHEN pr.amount-invoice_total <=0 THEN 'Due' ELSE 'No Transaction' END) AS remain_balance_status FROM invoice_master im inner join party p on im.party_id = p.party_id inner join party_type pt on p.party_type_id=pt.party_type_id inner join territory t on p.territory_id = t.territory_id inner join area a on p.area_id = a.area_id inner join region r on p.region_id = r.region_id left join invoice_details id on im.invoice_master_id=id.invoice_master_id inner join product pro on id.product_id = pro.product_id left join color c on id.color_id=c.color_id left join product_version pv on id.product_version_id = pv.product_version_id inner join product_category pc on pro.product_category_id= pc.product_category_id left join internal_requisition_master intrm on im.requisition_master_id=intrm.internal_requisition_master_id and im.party_id=78 left join payment_request pr on im.party_id =pr.party_id and im.requisition_master_id=pr.requisition_master_id where im.invoice_date BETWEEN '" + fromDate + "' AND DATEADD(DAY,1,'" + toDate + "' ) and id.product_id='" + productId + "'";

                    var data = _entities.Database.SqlQuery<DailySalesDeliveryModel>(queryWithFromDateToDate).ToList();
                    return data;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object InvoiceWiseImeiReport(string fromDate, string toDate, long invoiceMasterId)
        {
            try
            {
                string invoice_Details = " select im.invoice_master_id,im.invoice_no, im.invoice_date,p.product_name, c.color_name, v.product_version_name, s.imei_no, s.imei_no2 "
                                         + " from invoice_details id "
                                         +
                                         " inner join invoice_master im on id.invoice_details_id=im.invoice_master_id "
                                         +
                                         " inner join requisition_master rm on im.requisition_master_id=rm.requisition_master_id "
                                         +
                                         " inner join receive_serial_no_details s on rm.requisition_master_id=s.requisition_id "
                                         + " inner join product p on id.product_id=p.product_id "
                                         + " inner join color c on id.color_id=c.color_id "
                                         +
                                         " inner join product_version v on id.product_version_id=v.product_version_id "
                                         + " where s.deliver_master_id<>0 and im.invoice_master_id = " + invoiceMasterId + " ";

                var data = _entities.Database.SqlQuery<InvoiceWiseImeiModel>(invoice_Details).ToList();
                return data;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object GetInvoiceWiseImeiReportPDF(long invoiceMasterId)
        {
            string invoice_Details = " select distinct im.invoice_master_id,im.invoice_no, im.invoice_date,p.product_name, c.color_name, v.product_version_name,s.requisition_id, "
                                     +
                                     " (SELECT STUFF((SELECT ' ' + imei_no FROM receive_serial_no_details where receive_serial_no_details.requisition_id=s.requisition_id  and receive_serial_no_details.product_id=p.product_id and receive_serial_no_details.color_id=c.color_id and receive_serial_no_details.product_version_id=v.product_version_id FOR XML PATH('')) ,1,1,'') AS Txt) as imei_no, "
                                     +
                                     " (SELECT STUFF((SELECT ' ' + imei_no2 FROM receive_serial_no_details where  receive_serial_no_details.requisition_id=s.requisition_id  and receive_serial_no_details.product_id=p.product_id and receive_serial_no_details.color_id=c.color_id and receive_serial_no_details.product_version_id=v.product_version_id FOR XML PATH('')) ,1,1,'') AS Txt) as imei_no2  "

                                     + " from invoice_details id "
                                     + " inner join invoice_master im on id.invoice_details_id=im.invoice_master_id "
                                     +
                                     " inner join requisition_master rm on im.requisition_master_id=rm.requisition_master_id "
                                     +
                                     " inner join receive_serial_no_details s on rm.requisition_master_id=s.requisition_id "
                                     + " inner join product p on id.product_id=p.product_id "
                                     + " inner join color c on id.color_id=c.color_id "
                                     + " inner join product_version v on id.product_version_id=v.product_version_id "
                                     + " where im.invoice_master_id=" + invoiceMasterId + " ";
            var data = _entities.Database.SqlQuery<InvoiceWiseImeiModel>(invoice_Details).ToList();
            return data;
        }


        public object GetProductHistoryReport(string fromDate, string toDate, long productCategoryId, long productId, long colorId)
        {
            try
            {

                if (fromDate != "" && toDate != "" && productCategoryId == 0)
                {
                    string queryWithFromDateToDate = "select inv.inventory_id ,inv.transaction_date ,inv.transaction_type ,inv.document_code ,pc.product_category_name ,pro.product_name ,col.color_name ,pv.product_version_name ,war.warehouse_name ,party.party_name ,pt.party_type_name ,reg.region_name ,area.area_name ,ter.territory_name ,closing_stock as quantity from inventory inv inner join product pro on inv.product_id=pro.product_id inner join product_category pc on pro.product_category_id=pc.product_category_id inner join color col on inv.color_id = col.color_id inner join product_version pv on inv.product_version_id=pv.product_version_id inner join warehouse war on inv.warehouse_id=war.warehouse_id left join region reg on war.region_id = reg.region_id left join area on war.area_id =area.area_id left join territory ter on war.territory_id = ter.territory_id left join party on war.party_id=party.party_id left join party_type pt on party.party_type_id =pt.party_type_id where closing_stock >0 and inv.transaction_date BETWEEN '" + fromDate + "' AND DATEADD(DAY,1,'" + toDate + "' )";

                    var data = _entities.Database.SqlQuery<InventoryBreakDownReportModel>(queryWithFromDateToDate).ToList();
                    return data;
                }
                if (fromDate != "" && toDate != "" && productCategoryId != 0 && productId == 0)
                {
                    string queryWithproductCategoryId = "select inv.inventory_id ,inv.transaction_date ,inv.transaction_type ,inv.document_code ,pc.product_category_name ,pro.product_name ,col.color_name ,pv.product_version_name ,war.warehouse_name ,party.party_name ,pt.party_type_name ,reg.region_name ,area.area_name ,ter.territory_name ,closing_stock as quantity from inventory inv inner join product pro on inv.product_id=pro.product_id inner join product_category pc on pro.product_category_id=pc.product_category_id inner join color col on inv.color_id = col.color_id inner join product_version pv on inv.product_version_id=pv.product_version_id inner join warehouse war on inv.warehouse_id=war.warehouse_id left join region reg on war.region_id = reg.region_id left join area on war.area_id =area.area_id left join territory ter on war.territory_id = ter.territory_id left join party on war.party_id=party.party_id left join party_type pt on party.party_type_id =pt.party_type_id where closing_stock >0 and inv.transaction_date BETWEEN '" + fromDate + "' AND DATEADD(DAY,1,'" + toDate + "') and pc.product_category_id='" + productCategoryId + "' ";

                    var data = _entities.Database.SqlQuery<InventoryBreakDownReportModel>(queryWithproductCategoryId).ToList();
                    return data;
                }
                if (fromDate != "" && toDate != "" && productCategoryId != 0 && productId != 0 && colorId == 0)
                {
                    string queryWithproductCategoryId = "select inv.inventory_id ,inv.transaction_date ,inv.transaction_type ,inv.document_code ,pc.product_category_name ,pro.product_name ,col.color_name ,pv.product_version_name ,war.warehouse_name ,party.party_name ,pt.party_type_name ,reg.region_name ,area.area_name ,ter.territory_name ,closing_stock as quantity from inventory inv inner join product pro on inv.product_id=pro.product_id inner join product_category pc on pro.product_category_id=pc.product_category_id inner join color col on inv.color_id = col.color_id inner join product_version pv on inv.product_version_id=pv.product_version_id inner join warehouse war on inv.warehouse_id=war.warehouse_id left join region reg on war.region_id = reg.region_id left join area on war.area_id =area.area_id left join territory ter on war.territory_id = ter.territory_id left join party on war.party_id=party.party_id left join party_type pt on party.party_type_id =pt.party_type_id where closing_stock >0 and inv.transaction_date BETWEEN '" + fromDate + "' AND DATEADD(DAY,1,'" + toDate + "') and pc.product_category_id='" + productCategoryId + "'and pro.product_id='" + productId + "' ";

                    var data = _entities.Database.SqlQuery<InventoryBreakDownReportModel>(queryWithproductCategoryId).ToList();
                    return data;
                }

                if (fromDate != "" && toDate != "" && productId != 0 && colorId != 0)
                {
                    string queryproductId = "select inv.inventory_id ,inv.transaction_date ,inv.transaction_type ,inv.document_code ,pc.product_category_name ,pro.product_name ,col.color_name ,pv.product_version_name ,war.warehouse_name ,party.party_name ,pt.party_type_name ,reg.region_name ,area.area_name ,ter.territory_name ,closing_stock as quantity from inventory inv inner join product pro on inv.product_id=pro.product_id inner join product_category pc on pro.product_category_id=pc.product_category_id inner join color col on inv.color_id = col.color_id inner join product_version pv on inv.product_version_id=pv.product_version_id inner join warehouse war on inv.warehouse_id=war.warehouse_id left join region reg on war.region_id = reg.region_id left join area on war.area_id =area.area_id left join territory ter on war.territory_id = ter.territory_id left join party on war.party_id=party.party_id left join party_type pt on party.party_type_id =pt.party_type_id where closing_stock >0 and inv.transaction_date BETWEEN '" + fromDate + "' AND DATEADD(DAY,1,'" + toDate + "' ) and pro.product_id='" + productId + "' and col.color_id='" + colorId + "' ";

                    var data = _entities.Database.SqlQuery<InventoryBreakDownReportModel>(queryproductId).ToList();
                    return data;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public object GetProductHistoryPdfReport(string fromDate, string toDate, long productCategoryId, long productId, long colorId)
        {
            try
            {

                if (fromDate != "" && toDate != "" && productCategoryId == 0)
                {
                    string queryWithFromDateToDate = "select inv.inventory_id ,inv.transaction_date ,inv.transaction_type ,inv.document_code ,pc.product_category_name ,pro.product_name ,col.color_name ,pv.product_version_name ,war.warehouse_name ,party.party_name ,pt.party_type_name ,reg.region_name ,area.area_name ,ter.territory_name ,closing_stock as quantity from inventory inv inner join product pro on inv.product_id=pro.product_id inner join product_category pc on pro.product_category_id=pc.product_category_id inner join color col on inv.color_id = col.color_id inner join product_version pv on inv.product_version_id=pv.product_version_id inner join warehouse war on inv.warehouse_id=war.warehouse_id left join region reg on war.region_id = reg.region_id left join area on war.area_id =area.area_id left join territory ter on war.territory_id = ter.territory_id left join party on war.party_id=party.party_id left join party_type pt on party.party_type_id =pt.party_type_id where closing_stock >0 and inv.transaction_date BETWEEN '" + fromDate + "' AND DATEADD(DAY,1,'" + toDate + "' )";

                    var data = _entities.Database.SqlQuery<InventoryBreakDownReportModel>(queryWithFromDateToDate).ToList();
                    return data;
                }
                if (fromDate != "" && toDate != "" && productCategoryId != 0 && productId == 0)
                {
                    string queryWithproductCategoryId = "select inv.inventory_id ,inv.transaction_date ,inv.transaction_type ,inv.document_code ,pc.product_category_name ,pro.product_name ,col.color_name ,pv.product_version_name ,war.warehouse_name ,party.party_name ,pt.party_type_name ,reg.region_name ,area.area_name ,ter.territory_name ,closing_stock as quantity from inventory inv inner join product pro on inv.product_id=pro.product_id inner join product_category pc on pro.product_category_id=pc.product_category_id inner join color col on inv.color_id = col.color_id inner join product_version pv on inv.product_version_id=pv.product_version_id inner join warehouse war on inv.warehouse_id=war.warehouse_id left join region reg on war.region_id = reg.region_id left join area on war.area_id =area.area_id left join territory ter on war.territory_id = ter.territory_id left join party on war.party_id=party.party_id left join party_type pt on party.party_type_id =pt.party_type_id where closing_stock >0 and inv.transaction_date BETWEEN '" + fromDate + "' AND DATEADD(DAY,1,'" + toDate + "') and pc.product_category_id='" + productCategoryId + "' ";

                    var data = _entities.Database.SqlQuery<InventoryBreakDownReportModel>(queryWithproductCategoryId).ToList();
                    return data;
                }
                if (fromDate != "" && toDate != "" && productCategoryId != 0 && productId != 0 && colorId == 0)
                {
                    string queryWithproductCategoryId = "select inv.inventory_id ,inv.transaction_date ,inv.transaction_type ,inv.document_code ,pc.product_category_name ,pro.product_name ,col.color_name ,pv.product_version_name ,war.warehouse_name ,party.party_name ,pt.party_type_name ,reg.region_name ,area.area_name ,ter.territory_name ,closing_stock as quantity from inventory inv inner join product pro on inv.product_id=pro.product_id inner join product_category pc on pro.product_category_id=pc.product_category_id inner join color col on inv.color_id = col.color_id inner join product_version pv on inv.product_version_id=pv.product_version_id inner join warehouse war on inv.warehouse_id=war.warehouse_id left join region reg on war.region_id = reg.region_id left join area on war.area_id =area.area_id left join territory ter on war.territory_id = ter.territory_id left join party on war.party_id=party.party_id left join party_type pt on party.party_type_id =pt.party_type_id where closing_stock >0 and inv.transaction_date BETWEEN '" + fromDate + "' AND DATEADD(DAY,1,'" + toDate + "') and pc.product_category_id='" + productCategoryId + "'and pro.product_id='" + productId + "' ";

                    var data = _entities.Database.SqlQuery<InventoryBreakDownReportModel>(queryWithproductCategoryId).ToList();
                    return data;
                }

                if (fromDate != "" && toDate != "" && productId != 0 && colorId != 0)
                {
                    string queryproductId = "select inv.inventory_id ,inv.transaction_date ,inv.transaction_type ,inv.document_code ,pc.product_category_name ,pro.product_name ,col.color_name ,pv.product_version_name ,war.warehouse_name ,party.party_name ,pt.party_type_name ,reg.region_name ,area.area_name ,ter.territory_name ,closing_stock as quantity from inventory inv inner join product pro on inv.product_id=pro.product_id inner join product_category pc on pro.product_category_id=pc.product_category_id inner join color col on inv.color_id = col.color_id inner join product_version pv on inv.product_version_id=pv.product_version_id inner join warehouse war on inv.warehouse_id=war.warehouse_id left join region reg on war.region_id = reg.region_id left join area on war.area_id =area.area_id left join territory ter on war.territory_id = ter.territory_id left join party on war.party_id=party.party_id left join party_type pt on party.party_type_id =pt.party_type_id where closing_stock >0 and inv.transaction_date BETWEEN '" + fromDate + "' AND DATEADD(DAY,1,'" + toDate + "' ) and pro.product_id='" + productId + "' and col.color_id='" + colorId + "' ";

                    var data = _entities.Database.SqlQuery<InventoryBreakDownReportModel>(queryproductId).ToList();
                    return data;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public object GetSalableAndNonSalableStock()
        {
            try
            {
                string query = "SELECT product.product_name ,warehouse.warehouse_name ,warehouse.warehouse_id ,sum(inventory_stock.stock_quantity) as salable_quantity, (select TOP 1 stock_quantity from inventory_stock where warehouse_id in(13,14) and product_id=product.product_id) as non_salable_quantity FROM inventory_stock LEFT JOIN product ON inventory_stock.product_id=product.product_id LEFT JOIN warehouse ON inventory_stock.warehouse_id=warehouse.warehouse_id where warehouse_name ='WE Central Warehouse' group by product.product_name,product.product_id ,warehouse.warehouse_name,warehouse.warehouse_id";
                var quantity = _entities.Database.SqlQuery<SalableAndNonSalableStockModel>(query).ToList();
                return quantity;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public object DailyPaymentReport(string fromDate, string toDate, long partyTypeId, long partyId)
        {
            try
            {
                //DateTime dateFrom = Convert.ToDateTime(fromDate);
                //var fromDateAsString = dateFrom;

                //DateTime dataTo = Convert.ToDateTime(toDate);

                if (fromDate != "" && toDate != "" && partyTypeId == 0)
                {
                    string queryWithFromDateToDate = "select  CONVERT(VARCHAR,r.receive_date,103)as receive_date,r.party_id ,p.party_name ,pt.party_type_name ,a.area_name ,t.territory_name ,pm.payment_method_name ,b.bank_name ,bb.bank_branch_name ,(select top 1 bank_account_name from bank_account bc where bc.bank_account_id=r.bank_account_id) as bank_account_name ,r.amount ,pr.sales_representative ,r.received_invoice_no , ( CASE WHEN r.received_invoice_no is not null THEN 'INVOICE' WHEN r.received_invoice_no is null THEN 'ADVANCE' END ) AS reference ,users.full_name from receive r left join party p on r.party_id=p.party_id left join party_type pt on p.party_type_id =pt.party_type_id left join area a on p.area_id=a.area_id left join territory t on p.territory_id=t.territory_id left join payment_method pm on r.payment_method_id=pm.payment_method_id left join bank b on r.bank_id = b.bank_id left join bank_branch bb on r.bank_branch_id=bb.bank_branch_id left join payment_request pr on r.payment_req_id= pr.payment_req_id left join users on r.created_by=users.user_id where r.receive_date BETWEEN convert(datetime,'" + fromDate + "') AND DATEADD(DAY,1,convert(datetime,'" + toDate + "'))";

                    var data = _entities.Database.SqlQuery<DailyPaymentReportModel>(queryWithFromDateToDate).ToList();
                    return data;
                }
                if (fromDate != "" && toDate != "" && partyTypeId != 0 && partyId == 0)
                {
                    string queryWithFromDateToDate = "select  CONVERT(VARCHAR,r.receive_date,103)as receive_date,r.party_id ,p.party_name ,pt.party_type_name ,a.area_name ,t.territory_name ,pm.payment_method_name ,b.bank_name ,bb.bank_branch_name ,(select top 1 bank_account_name from bank_account bc where bc.bank_account_id=r.bank_account_id) as bank_account_name ,r.amount ,pr.sales_representative ,r.received_invoice_no , ( CASE WHEN r.received_invoice_no is not null THEN 'INVOICE' WHEN r.received_invoice_no is null THEN 'ADVANCE' END ) AS reference ,users.full_name from receive r left join party p on r.party_id=p.party_id left join party_type pt on p.party_type_id =pt.party_type_id left join area a on p.area_id=a.area_id left join territory t on p.territory_id=t.territory_id left join payment_method pm on r.payment_method_id=pm.payment_method_id left join bank b on r.bank_id = b.bank_id left join bank_branch bb on r.bank_branch_id=bb.bank_branch_id left join payment_request pr on r.payment_req_id= pr.payment_req_id left join users on r.created_by=users.user_id where r.receive_date BETWEEN convert(datetime,'" + fromDate + "') AND DATEADD(DAY,1,convert(datetime,'" + toDate + "')) and pt.party_type_id='" + partyTypeId + "'";

                    var data = _entities.Database.SqlQuery<DailyPaymentReportModel>(queryWithFromDateToDate).ToList();
                    return data;
                }
                if (fromDate != "" && toDate != "" && partyId != 0)
                {
                    string queryWithFromDateToDate = "select  CONVERT(VARCHAR,r.receive_date,103)as receive_date,r.party_id ,p.party_name ,pt.party_type_name ,a.area_name ,t.territory_name ,pm.payment_method_name ,b.bank_name ,bb.bank_branch_name ,(select top 1 bank_account_name from bank_account bc where bc.bank_account_id=r.bank_account_id) as bank_account_name ,r.amount ,pr.sales_representative ,r.received_invoice_no , ( CASE WHEN r.received_invoice_no is not null THEN 'INVOICE' WHEN r.received_invoice_no is null THEN 'ADVANCE' END ) AS reference ,users.full_name from receive r left join party p on r.party_id=p.party_id left join party_type pt on p.party_type_id =pt.party_type_id left join area a on p.area_id=a.area_id left join territory t on p.territory_id=t.territory_id left join payment_method pm on r.payment_method_id=pm.payment_method_id left join bank b on r.bank_id = b.bank_id left join bank_branch bb on r.bank_branch_id=bb.bank_branch_id left join payment_request pr on r.payment_req_id= pr.payment_req_id left join users on r.created_by=users.user_id where r.receive_date BETWEEN convert(datetime,'" + fromDate + "') AND DATEADD(DAY,1,convert(datetime,'" + toDate + "')) and r.party_id='" + partyId + "'";

                    var data = _entities.Database.SqlQuery<DailyPaymentReportModel>(queryWithFromDateToDate).ToList();
                    return data;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object GetAllInvoiceNo()
        {
            try
            {
                var productFromPricing = (from r in _entities.invoice_master
                                          select new
                                          {
                                              r.invoice_master_id,
                                              r.invoice_no
                                          }).ToList();

                return productFromPricing;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public object GetAllInvoiceNo(long partyId)
        {
            try
            {
                var productFromPricing = (from r in _entities.invoice_master
                                          where r.party_id == partyId
                                          select new
                                          {
                                              r.invoice_master_id,
                                              r.invoice_no
                                          }).ToList();

                return productFromPricing;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public object GetAccountsReport(string fromDate, string toDate, long partyTypeId, long partyId, string receivedInvoiceNo)
        {
            try
            {

                if (fromDate != "" && toDate != "" && partyTypeId == 0)
                {
                    string queryWithFromDateToDate = "select CONVERT(VARCHAR,r.receive_date,103)as receive_date ,r.party_id ,p.party_name ,pt.party_type_name ,a.area_name ,t.territory_name ,pm.payment_method_name ,b.bank_name ,bb.bank_branch_name ,(select top 1 bank_account_name from bank_account bc where bc.bank_account_id=r.bank_account_id) as bank_account_name ,(select invoice_total from invoice_master im where im.invoice_no=r.received_invoice_no ) as invoice_amount ,r.amount ,pr.sales_representative ,r.received_invoice_no , ( CASE WHEN r.received_invoice_no is not null THEN 'INVOICE' WHEN r.received_invoice_no is null THEN 'ADVANCE' END ) AS reference ,users.full_name from receive r left join party p on r.party_id=p.party_id left join party_type pt on p.party_type_id =pt.party_type_id left join area a on p.area_id=a.area_id left join territory t on p.territory_id=t.territory_id left join payment_method pm on r.payment_method_id=pm.payment_method_id left join bank b on r.bank_id = b.bank_id left join bank_branch bb on r.bank_branch_id=bb.bank_branch_id left join payment_request pr on r.payment_req_id= pr.payment_req_id left join users on r.created_by=users.user_id where r.receive_date BETWEEN convert(datetime,'"+fromDate+"') AND DATEADD(DAY,1,convert(datetime,'"+toDate+"'))";

                    var data = _entities.Database.SqlQuery<AccountsReportModel>(queryWithFromDateToDate).ToList();
                    return data;
                }
                if (fromDate != "" && toDate != "" && partyTypeId != 0 && partyId == 0)
                {
                    string queryWithFromDateToDate = "select CONVERT(VARCHAR,r.receive_date,103)as receive_date ,r.party_id ,p.party_name ,pt.party_type_name ,a.area_name ,t.territory_name ,pm.payment_method_name ,b.bank_name ,bb.bank_branch_name ,(select top 1 bank_account_name from bank_account bc where bc.bank_account_id=r.bank_account_id) as bank_account_name ,(select invoice_total from invoice_master im where im.invoice_no=r.received_invoice_no ) as invoice_amount ,r.amount ,pr.sales_representative ,r.received_invoice_no , ( CASE WHEN r.received_invoice_no is not null THEN 'INVOICE' WHEN r.received_invoice_no is null THEN 'ADVANCE' END ) AS reference ,users.full_name from receive r left join party p on r.party_id=p.party_id left join party_type pt on p.party_type_id =pt.party_type_id left join area a on p.area_id=a.area_id left join territory t on p.territory_id=t.territory_id left join payment_method pm on r.payment_method_id=pm.payment_method_id left join bank b on r.bank_id = b.bank_id left join bank_branch bb on r.bank_branch_id=bb.bank_branch_id left join payment_request pr on r.payment_req_id= pr.payment_req_id left join users on r.created_by=users.user_id where pt.party_type_id='"+partyTypeId+"' r.receive_date BETWEEN convert(datetime,'2017-05-01') AND DATEADD(DAY,1,convert(datetime,'2017-07-09'))";

                    var data = _entities.Database.SqlQuery<AccountsReportModel>(queryWithFromDateToDate).ToList();
                    return data;
                }
                if (fromDate != "" && toDate != "" && partyId != 0 && receivedInvoiceNo == null)
                {
                    string queryWithFromDateToDate = "select CONVERT(VARCHAR,r.receive_date,103)as receive_date ,r.party_id ,p.party_name ,pt.party_type_name ,a.area_name ,t.territory_name ,pm.payment_method_name ,b.bank_name ,bb.bank_branch_name ,(select top 1 bank_account_name from bank_account bc where bc.bank_account_id=r.bank_account_id) as bank_account_name ,(select invoice_total from invoice_master im where im.invoice_no=r.received_invoice_no ) as invoice_amount ,r.amount ,pr.sales_representative ,r.received_invoice_no , ( CASE WHEN r.received_invoice_no is not null THEN 'INVOICE' WHEN r.received_invoice_no is null THEN 'ADVANCE' END ) AS reference ,users.full_name from receive r left join party p on r.party_id=p.party_id left join party_type pt on p.party_type_id =pt.party_type_id left join area a on p.area_id=a.area_id left join territory t on p.territory_id=t.territory_id left join payment_method pm on r.payment_method_id=pm.payment_method_id left join bank b on r.bank_id = b.bank_id left join bank_branch bb on r.bank_branch_id=bb.bank_branch_id left join payment_request pr on r.payment_req_id= pr.payment_req_id left join users on r.created_by=users.user_id where pt.party_type_id='" + partyTypeId + "' and r.party_id='"+partyId+"' and r.receive_date BETWEEN convert(datetime,'2017-05-01') AND DATEADD(DAY,1,convert(datetime,'2017-07-09'))";

                    var data = _entities.Database.SqlQuery<AccountsReportModel>(queryWithFromDateToDate).ToList();
                    return data;
                }
                if (fromDate != "" && toDate != "" && partyId != 0 && receivedInvoiceNo!=null)
                {
                    string queryWithFromDateToDate = "select CONVERT(VARCHAR,r.receive_date,103)as receive_date ,r.party_id ,p.party_name ,pt.party_type_name ,a.area_name ,t.territory_name ,pm.payment_method_name ,b.bank_name ,bb.bank_branch_name ,(select top 1 bank_account_name from bank_account bc where bc.bank_account_id=r.bank_account_id) as bank_account_name ,(select invoice_total from invoice_master im where im.invoice_no=r.received_invoice_no ) as invoice_amount ,r.amount ,pr.sales_representative ,r.received_invoice_no , ( CASE WHEN r.received_invoice_no is not null THEN 'INVOICE' WHEN r.received_invoice_no is null THEN 'ADVANCE' END ) AS reference ,users.full_name from receive r left join party p on r.party_id=p.party_id left join party_type pt on p.party_type_id =pt.party_type_id left join area a on p.area_id=a.area_id left join territory t on p.territory_id=t.territory_id left join payment_method pm on r.payment_method_id=pm.payment_method_id left join bank b on r.bank_id = b.bank_id left join bank_branch bb on r.bank_branch_id=bb.bank_branch_id left join payment_request pr on r.payment_req_id= pr.payment_req_id left join users on r.created_by=users.user_id where pt.party_type_id='" + partyTypeId + "' and r.party_id='" + partyId + "'and  r.received_invoice_no='" + receivedInvoiceNo + "' and r.receive_date BETWEEN convert(datetime,'2017-05-01') AND DATEADD(DAY,1,convert(datetime,'2017-07-09'))";

                    var data = _entities.Database.SqlQuery<AccountsReportModel>(queryWithFromDateToDate).ToList();
                    return data;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}