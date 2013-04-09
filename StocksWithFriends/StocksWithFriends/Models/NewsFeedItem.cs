using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace StocksWithFriends.Models
{
    public class NewsFeedItem
    {

        public string ID { get; set; }
        public string From { get; set; }
        public string FromID { get; set; }
        public string To { get; set; }
        public string ToID { get; set; }
        public string Message { get; set; }
        public string Picture { get; set; }
        public string Time { get; set; }
        public int NumLikes { get; set; }
        public List<List<String>> Comments { get; set; }
    }

    public class NewsFeedItemDBContext : DbContext
    {
        public DbSet<NewsFeedItem> NewsFeedItems { get; set; }
    }
}