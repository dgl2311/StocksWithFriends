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
    
    public partial class StockTransaction
    {
        public int id { get; set; }
        public long user_id { get; set; }
        public System.DateTime timestamp { get; set; }
        public string stock_symbol { get; set; }
        public int tx_quantity_delta { get; set; }
        public double tx_price { get; set; }
    }
}
