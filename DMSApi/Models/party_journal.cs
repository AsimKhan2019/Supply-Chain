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
    
    public partial class party_journal
    {
        public long party_journal_id { get; set; }
        public Nullable<System.DateTime> transaction_date { get; set; }
        public string transaction_type { get; set; }
        public Nullable<long> party_id { get; set; }
        public Nullable<decimal> opening_balance { get; set; }
        public Nullable<decimal> dr_amount { get; set; }
        public Nullable<decimal> cr_amount { get; set; }
        public Nullable<decimal> closing_balance { get; set; }
        public string remarks { get; set; }
        public Nullable<long> owner_party_id { get; set; }
        public string document_code { get; set; }
        public Nullable<long> created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public Nullable<long> updated_by { get; set; }
        public Nullable<System.DateTime> updated_date { get; set; }
        public Nullable<bool> is_active { get; set; }
        public Nullable<bool> is_deleted { get; set; }
    }
}
