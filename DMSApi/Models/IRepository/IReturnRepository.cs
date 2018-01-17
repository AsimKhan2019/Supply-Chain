﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMSApi.Models.StronglyType;

namespace DMSApi.Models.IRepository
{
    interface IReturnRepository
    {
        List<return_type> GetReturnType();
        //object GetIMEIInformation(string imei_no, int user_id);
        object GetIMEIInformation(long imei_no, long party_id);
        int AddReturn(ReturnModel ReturnModel);
        //object GetAllMDNDBISReturn();
        object GetAllReturn();
        bool ReceivingVerifiedIMEI(long return_details_id);
        ReturnModel GetReturnById(long return_master_id);
        int VerifyReturn(ReturnModel ReturnModel);
        int VerifybyDoaEngineer(ReturnModel ReturnModel);
        ReturnModel GetVerifiedReturnById(long return_master_id);
        int ReceiveReturn(ReturnModel ReturnModel);
        object GetAllRetailerReturn();

        //object GetIMEIForReplace(string imei_no);
        object GetIMEIForReplace(string replaced_imei, string replacing_imei);
        int PostReplace(ReturnModel ReturnModel);
        object ReturnInvoiceReportById(long pos_master_id);
        object GetAllReturnForReceive();
        object GetAllReturnForVerify();

    }
}
