using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Diagnostics;

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
        List<string> symbols = new List<string> { "GOOG", "AAPL", "MSFT", "DIS", "T" };

        public ActionResult Index()
        {
            if (Request.QueryString["symbol"] != null)
                symbols.Add(Request.QueryString["symbol"]);
            GetResponseText();
            return View();
        }

        public ActionResult Ticker()
        {
            List<Stock> stocks = new List<Stock>();
            foreach (string symbol in symbols)
            {
                Stock stock = GetStock(MakeStockUrl(symbol));
                if (stock != null) stocks.Add(stock);
            }

            if (stocks.Count == 0)
                ViewBag.success = false;

            ViewBag.stocks = stocks;
            ViewBag.success = true;
            return PartialView();
        }

        public ActionResult LookUp()
        {
            if (Request.QueryString["symbol"] == null)
            {
                ViewBag.success = false;
                return View();
            }

            Stock stock = GetStock(MakeStockUrl(Request.QueryString["symbol"]));
            ViewBag.symbol = Request.QueryString["symbol"];

            if (stock == null)
            {
                ViewBag.success = false;
                return View();
            }

            ViewBag.stock = stock;
            ViewBag.success = true;

            return View();
        }

        public void GetResponseText()
        {
            List<Stock> stocks = new List<Stock>();
            foreach (string symbol in symbols)
            {
                Stock stock = GetStock(MakeStockUrl(symbol));
                if (stock != null) stocks.Add(stock);
            }

            if (stocks.Count == 0)
                ViewBag.success = false;

            ViewBag.stocks = stocks;
            ViewBag.success = true;
        }

        private Uri MakeStockUrl(string symbol)
        {
            string queryURL = "http://download.finance.yahoo.com/d/quotes.csv?s={0}&f=nsl1t7&e=.csv";
            return new Uri(String.Format(queryURL, symbol));
        }

        public Stock GetStock(Uri callUrl)
        {
            Stock output = new Stock();

            string responseText = String.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(callUrl);
            request.Method = WebRequestMethods.Http.Get;
            request.Accept = "application/json";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                string testing = sr.ReadToEnd();
                string[] stockInfo = Regex.Split(testing, "\r\n");
                for (int i = 0; i < stockInfo.Length; i++)
                {
                    stockInfo[i] = stockInfo[i].Replace("\\", "");
                    stockInfo[i] = stockInfo[i].Replace("\"", "");
                }
                string[] commaSplit = stockInfo[0].Split(',');

                try
                {
                    output.symbol = commaSplit[1];
                    output.name = commaSplit[0];
                    output.price = Double.Parse(commaSplit[2]);
                    output.trend = Stock.CalculateTrend(commaSplit[3]);

                    if (output.price == 0)
                        return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return output;
        }

        public JsonResult AddStock(string symbol)
        {
            Stock s = GetStock(MakeStockUrl(symbol));
            Tuple<bool, Stock> result = new Tuple<bool, Stock>(s != null, s);
            Debug.WriteLine("Result: " + result.Item1);
            return Json(result, JsonRequestBehavior.AllowGet);
            /* if (s == null)
                 return Json(false, JsonRequestBehavior.AllowGet);
             else
                 return Json(s, JsonRequestBehavior.AllowGet);*/
        }
    }



    public class Stock // placeholder for actual database models
    {
        public string name, symbol, trend;
        public double price;

        public Stock() { }

        public Stock(string name, string symbol, double price, string trend = "neutral")
        {
            this.name = name;
            this.symbol = symbol;
            this.price = price;
            this.trend = trend;
        }

        public string ToString()
        {
            return String.Format("[{0}: {1}]", symbol, price);
        }

        public static string CalculateTrend(string trendString)
        {
            int trend = 0;

            trendString.Replace("&nbsp;", String.Empty);
            foreach (char c in trendString)
            {
                switch (c)
                {
                    case '-':
                        trend--;
                        break;
                    case '+':
                        trend++;
                        break;
                }
            }

            if (trend > 1)
                return "up";
            if (trend < -1)
                return "down";
            return "neutral";
        }
    }
}
