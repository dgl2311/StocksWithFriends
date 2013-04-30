using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StocksWithFriends.Models
{
    public class CalendarEvent
    {
        public string Title { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public Boolean AllDay { get; set; }
    }
}