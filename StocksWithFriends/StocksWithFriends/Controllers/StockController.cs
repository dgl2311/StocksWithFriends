using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace StocksWithFriends.Controllers
{
    public class StockJSON
    {
        public string Name { get; set; }
        public string LastTradePriceOnly { get; set; }
        public string query { get; set; }
    }
    public class StockController : Controller
    {
        //
        // GET: /Stock/

        public ActionResult Index()
        {
            string queryURL = "http://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20yahoo.finance.quotes%20where%20symbol%20in%20(%22{0}%22)&format=json&diagnostics=true&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys";
            string symbol = "GOOG";
            Uri url = new Uri(String.Format(queryURL, symbol));
            GetResponseText(url.ToString());
            return View();
        }
        public void GetResponseText(string url)
        {
            string responseText = String.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Get;
            request.Accept = "application/json";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                JObject o = JObject.Parse(sr.ReadToEnd());
                //string first = (string)o["query"];
                try
                {
                    ViewBag.symbol = (string)o["query"]["results"]["quote"]["Symbol"];
                    ViewBag.companyName = (string)o["query"]["results"]["quote"]["Name"];
                    ViewBag.curPrice = (string)o["query"]["results"]["quote"]["LastTradePriceOnly"];
                    //JavaScriptSerializer js = new JavaScriptSerializer();
                    //var objText = sr.ReadToEnd();
                    //json = (StockJSON)js.Deserialize(objText, typeof(StockJSON));
                }
                catch (Exception e)
                {
                    ViewBag.success = false;
                }
            }
        }

    }
}
