﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSApi.Models.IRepository
{
    public interface IReceiveRepository
    {


        object GetAllReceives();
        object GetInvoiceNumber(long party_id);
        decimal GetClousingBalance(long party_id);
        receive GetReceiveById(long receive_id);
        List<receive> GetReceiveList();
        long AddReceive(receive receive);
        bool EditReceive(receive receive);
        long DeleteReceive(long receive_id, long updated_by);
        bool UpdateStatus(long receive_id, long user_id);
        object GetMoneyReceiptReport(long receive_id);
        payment_request ProcessPaymentRequiest(long payment_req_id);
        object PaymentHistory(string from_date, string to_date);
        object ProductLiftingAndPaymentSummery(string from_date, string to_date, long party_id);
        object GetAllUnReceivedPaymentList();
        object GetAllPaymentReceivedList(DateTime fromDate, DateTime toDate, long partyId);
    }
}
