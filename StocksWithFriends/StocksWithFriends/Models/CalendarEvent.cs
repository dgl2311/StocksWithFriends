//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StocksWithFriends.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class CalendarEvent
    {
        public int id { get; set; }
        public string user_id { get; set; }
        public string event_name { get; set; }
        public string event_description { get; set; }
        public System.DateTime start_timestamp { get; set; }
        public System.DateTime end_timestamp { get; set; }
    }
}
