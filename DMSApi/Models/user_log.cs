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
    
    public partial class user_log
    {
        public long ulogo_id { get; set; }
        public Nullable<long> user_id { get; set; }
        public Nullable<System.TimeSpan> login_time { get; set; }
        public Nullable<System.DateTime> login_date { get; set; }
        public string ip_address { get; set; }
        public Nullable<System.TimeSpan> logout_time { get; set; }
        public Nullable<System.DateTime> logout_date { get; set; }
        public string login_name { get; set; }
    }
}
