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
    
    public partial class invoice_details
    {
        public long invoice_details_id { get; set; }
        public Nullable<long> invoice_master_id { get; set; }
        public Nullable<long> product_id { get; set; }
        public Nullable<long> product_category_id { get; set; }
        public Nullable<long> unit_id { get; set; }
        public Nullable<long> color_id { get; set; }
        public Nullable<long> brand_id { get; set; }
        public Nullable<int> quantity { get; set; }
        public Nullable<decimal> price { get; set; }
        public Nullable<decimal> line_total { get; set; }
        public Nullable<bool> is_gift { get; set; }
        public string gift_type { get; set; }
        public Nullable<bool> is_live_dummy { get; set; }
        public Nullable<long> product_version_id { get; set; }
        public Nullable<long> promotion_master_id { get; set; }
        public Nullable<decimal> discount { get; set; }
        public Nullable<decimal> discount_amount { get; set; }
    }
}
