//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DMSApi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class payment_request
    {
        public long payment_req_id { get; set; }
        public Nullable<long> party_id { get; set; }
        public string cheque_no { get; set; }
        public string document_attachment { get; set; }
        public Nullable<bool> approved { get; set; }
        public Nullable<System.DateTime> deposite_date { get; set; }
        public string remarks { get; set; }
        public Nullable<decimal> amount { get; set; }
        public Nullable<long> payment_method_id { get; set; }
        public Nullable<long> bank_id { get; set; }
        public Nullable<long> bank_branch_id { get; set; }
        public Nullable<long> bank_account_id { get; set; }
        public string sales_representative { get; set; }
        public Nullable<long> owner_party_id { get; set; }
        public Nullable<long> created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public Nullable<long> updated_by { get; set; }
        public Nullable<System.DateTime> updated_date { get; set; }
        public Nullable<bool> is_active { get; set; }
        public Nullable<bool> is_deleted { get; set; }
        public Nullable<long> requisition_master_id { get; set; }
        public string payment_type { get; set; }
    }
}
