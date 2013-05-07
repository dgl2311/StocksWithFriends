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
        Random r = new Random();

        public ActionResult Index()
        {
            if (Request.QueryString["symbol"] != null)
                symbols.Add(Request.QueryString["symbol"]);

            List<Stock> stocks = LoadStocks();

            List<StockHistory> history = LoadStockHistory(stocks);

            ViewBag.data = history;
            ViewBag.success = history.Count > 0;
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

        }

        private List<Stock> LoadStocks()
        {
            List<Stock> stocks = new List<Stock>();
            foreach (string symbol in symbols)
            {
                Stock stock = GetStock(MakeStockUrl(symbol));
                if (stock != null) stocks.Add(stock);
            }

            if (stocks.Count == 0)
                ViewBag.success = false;

            return stocks;
        }

        private List<StockHistory> LoadStockHistory(List<Stock> stocks)
        {
            List<StockHistory> output = new List<StockHistory>();

            foreach (Stock s in stocks)
                output.Add(new StockHistory(s, generateDummyData(5, s.price)));

            return output;
        }

        private Uri MakeStockUrl(string symbol)
        {
            string queryURL = "http://download.finance.yahoo.com/d/quotes.csv?s={0}&f=nsl1t7&e=.csv";
            return new Uri(String.Format(queryURL, symbol));
        }

        private Stock GetStock(Uri callUrl)
        {
            Stock output = null;

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
                    output = new Stock(commaSplit[0], commaSplit[1], float.Parse(commaSplit[2]), Stock.CalculateTrend(commaSplit[3]));

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

        public JsonResult GetStock(string symbol)
        {
            Stock s = GetStock(MakeStockUrl(symbol));
            Tuple<bool, Stock> result = new Tuple<bool, Stock>(s != null, s);
            Debug.WriteLine("Result: " + result.Item1);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuyStock(string symbol, int quantity)
        {
            Stock s = GetStock(MakeStockUrl(symbol));
            Tuple<bool, string> result;

            if (s != null)
            {
                bool success = false;
                // Database entry here
                if (success) // success
                    result = new Tuple<bool, string>(true, "Stock purchased successfully");
                else
                    result = new Tuple<bool, string>(false, "Error message here?");
            }
            else
            {
                result = new Tuple<bool, string>(false, "Stock symbol not valid");
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SellStock(string symbol, int quantity)
        {
            Stock s = GetStock(MakeStockUrl(symbol));
            Tuple<bool, string> result;

            if (s != null)
            {
                bool success = false;
                // Database entry here
                if (success) // success
                    result = new Tuple<bool, string>(true, "Stock sold successfully");
                else
                    result = new Tuple<bool, string>(false, String.Format("Cannot sell {0} shares, as you only own {1}", -1, -1));
            }
            else
            {
                result = new Tuple<bool, string>(false, "Stock symbol not valid");
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveNote(string symbol, string note)
        {
            Tuple<bool, string> result;

            bool success = false;
            // Database entry here

            if (success) // success
                result = new Tuple<bool, string>(true, "Note saved successfully");
            else
                result = new Tuple<bool, string>(false, "Note not saved");

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private List<Transaction> generateDummyData(int n, float pricePoint = 50)
        {
            List<Transaction> output = new List<Transaction>();

            float price = pricePoint;
            DateTime timestamp = new DateTime(2012, 1, 1);
            int quantity = 20;
            int runningTot = quantity;

            for (int i = 0; i < n; i++)
            {
                output.Add(new Transaction(timestamp, quantity, price));
                price *= (float)(r.NextDouble() * 0.4 + 0.8);
                timestamp = timestamp.AddDays(r.Next(1, 20));
                Debug.WriteLine(String.Format("Random stuff: {0} to {1}", -runningTot + 1, (int)(runningTot / 2) + 5));
                quantity = r.Next(-runningTot + 1, (int)(runningTot / 2) + 5);
                runningTot += quantity;

                Debug.WriteLine(output[output.Count - 1]);
            }

            return output;
        }
    }

    public class StockHistory
    {
        public Stock stock;
        public List<Transaction> transactions;
        public List<int> runningTotal;
        public string note;

        public float totalProfit
        {
            get
            {
                float output = 0;

                foreach (Transaction t in transactions)
                    output += t.txAmount;

                return output;
            }
        }

        public int quantity
        {
            get
            {
                int n = 0;

                foreach (Transaction t in transactions)
                    n += t.quantity;

                return n;
            }
        }

        public float worthVia
        {
            get
            {
                return totalProfit + totalValue;
            }
        }

        public float totalValue
        {
            get
            {
                Debug.WriteLine(String.Format("{0} value: {1} stocks * ${2}", stock.symbol, quantity, stock.price));
                return quantity * stock.price;
            }
        }

        public StockHistory(Stock s, List<Transaction> t, string n = "")
        {
            stock = s;
            transactions = t;
            transactions.Sort((a, b) => b.timestamp.CompareTo(a.timestamp)); // custom sort based on transaction date
            note = n;

            runningTotal = new List<int>();
            int curTotal = 0;

            for (int i = transactions.Count - 1; i >= 0; i--)
            {
                curTotal += transactions[i].quantity;
                runningTotal.Insert(0, curTotal);
            }
            Debug.WriteLine("total count: " + runningTotal.Count);
        }
    }

    public class Transaction // placeholder
    {
        public DateTime timestamp;
        public int quantity;
        public float price;
        public float txAmount { get { return -quantity * price; } }

        public Transaction() { }

        public Transaction(DateTime timestamp, int quantity, float price)
        {
            this.timestamp = timestamp;
            this.quantity = quantity;
            this.price = price;
        }

        public override string ToString()
        {
            return String.Format("{0} :: {1} * {2}", timestamp, quantity, price);
        }
    }

    public class Stock // placeholder for actual database models
    {
        public string name, symbol, trend;
        public float price;

        public Stock(string name, string symbol, float price, string trend = "neutral")
        {
            this.name = name;
            this.symbol = symbol;
            this.price = price;
            this.trend = trend;
        }

        public override string ToString()
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
