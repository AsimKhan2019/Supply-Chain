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
    
    public partial class to_delivery_master
    {
        public long to_delivery_master_id { get; set; }
        public string to_delivery_no { get; set; }
        public Nullable<System.DateTime> to_delivery_date { get; set; }
        public string to_delivery_address { get; set; }
        public string delivery_method { get; set; }
        public string to_logistics_delivered_by { get; set; }
        public Nullable<long> transfer_order_master_id { get; set; }
        public Nullable<long> courier_id { get; set; }
        public Nullable<long> company_id { get; set; }
        public string courier_slip_no { get; set; }
        public Nullable<long> from_warehouse_id { get; set; }
        public Nullable<long> to_warehouse_id { get; set; }
        public string status { get; set; }
        public string remarks { get; set; }
        public string vehicle_no { get; set; }
        public string truck_driver_name { get; set; }
        public string truck_driver_mobile { get; set; }
        public Nullable<long> created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public Nullable<long> updated_by { get; set; }
        public Nullable<System.DateTime> updated_date { get; set; }
        public Nullable<bool> is_active { get; set; }
        public Nullable<bool> is_deleted { get; set; }
    }
}
