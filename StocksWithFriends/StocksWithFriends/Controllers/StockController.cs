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
using StocksWithFriends.Models;

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
        Random r = new Random();

        DBEntities _db;

        public StockController()
        {
            _db = new DBEntities();
        }

        public ActionResult Index()
        {
            List<StockHistory> history = GetStockInfo();

            ViewBag.data = history;
            ViewBag.success = true; //history.Count > 0;
            GetResponseText();
            return View();
        }

        public ActionResult Ticker()
        {
            List<StockHistory> data = GetStockInfo();
            List<Stock> stocks = new List<Stock>();

            data.Sort((a, b) => b.totalValue.CompareTo(a.totalValue));

            int i = 0;
            while (i < 5 && i < data.Count)
                stocks.Add(data[i++].stock);

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
            if (Session["userId"] == null)
                return null;

            Stock s = GetStock(MakeStockUrl(symbol));
            Tuple<bool, string> result;

            if (s != null)
            {
                try
                {
                    _db.StockTransactions.Add(MakeTransaction(s, quantity));
                    _db.SaveChanges();

                    result = new Tuple<bool, string>(true, "Stock purchased successfully");
                }
                catch (Exception e) { result = new Tuple<bool, string>(false, e.Message); }
            }
            else
                result = new Tuple<bool, string>(false, "Stock symbol not valid");

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private StockTransaction MakeTransaction(Stock stock, int quantity)
        {
            StockTransaction tx = new StockTransaction();

            tx.stock_symbol = stock.symbol;
            tx.timestamp = DateTime.Now;
            tx.tx_price = stock.price;
            tx.tx_quantity_delta = quantity;
            tx.user_id = (string)Session["userId"];
            tx.id = (_db.StockTransactions.Count() > 0 ? _db.StockTransactions.ToList().Last().id + 1 : 0);

            return tx;
        }

        public JsonResult SellStock(string symbol, int quantity)
        {
            if (Session["userId"] == null)
                return null;

            Stock s = GetStock(MakeStockUrl(symbol));
            Tuple<bool, string> result;

            if (s != null)
            {
                List<StockHistory> history = GetStockInfo();

                bool canSell = false;
                int currentQuantity = -1;

                foreach (StockHistory data in history)
                {
                    if (data.stock.symbol.Equals(symbol))
                    {
                        currentQuantity = data.quantity;
                        canSell = (currentQuantity >= quantity);

                        break;
                    }
                }

                if (canSell)
                {
                    try
                    {
                        _db.StockTransactions.Add(MakeTransaction(s, -quantity));
                        _db.SaveChanges();

                        result = new Tuple<bool, string>(true, "Stock sold successfully");
                    }
                    catch (Exception e) { result = new Tuple<bool, string>(false, e.Message); }
                }
                else { result = new Tuple<bool, string>(false, String.Format("Cannot sell {0} shares, as you only own {1}", quantity, currentQuantity)); }
            }
            else
                result = new Tuple<bool, string>(false, "Stock symbol not valid");

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveNote(string symbol, string noteString)
        {
            if (Session["userId"] == null)
                return null;

            Stock s = GetStock(MakeStockUrl(symbol));
            Tuple<bool, string> result;

            if (s != null)
            {
                try
                {
                    StockNote note = new StockNote();

                    var oldNoteArray = from n in _db.StockNotes
                                       where n.stock_symbol == (string)symbol
                                       select n;

                    StockNote[] oldNotes = oldNoteArray.ToList().ToArray();

                    foreach (StockNote sn in oldNotes)
                        _db.StockNotes.Remove(sn);

                    _db.SaveChanges();

                    note.stock_symbol = symbol;
                    note.note_string = noteString;
                    note.user_id = (string)Session["userId"];
                    note.id = (_db.StockNotes.Count() > 0 ? _db.StockNotes.ToList().Last().id + 1 : 0);

                    _db.StockNotes.Add(note);
                    _db.SaveChanges();

                    result = new Tuple<bool, string>(true, "Stock purchased successfully");
                }
                catch (Exception e) { result = new Tuple<bool, string>(false, e.Message); }
            }
            else
                result = new Tuple<bool, string>(false, "Stock symbol not valid");

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private List<StockHistory> GetStockInfo()
        {
            List<StockHistory> output = new List<StockHistory>();

            if (Session["userId"] == null)
            {
                ViewBag.success = false;
                return output;
            }

            string uid = (string)Session["userId"];

            var transactionModels = from tx in _db.StockTransactions
                                    where tx.user_id == uid
                                    select tx;

            var noteModels = from note in _db.StockNotes
                             where note.user_id == uid
                             select note;

            StockTransaction[] models = transactionModels.ToList().ToArray();
            StockNote[] notes = noteModels.ToList().ToArray();

            Dictionary<string, List<Transaction>> txMap = new Dictionary<string, List<Transaction>>();
            Dictionary<string, string> noteMap = new Dictionary<string, string>();


            foreach (StockTransaction tx in models)
            {
                if (!txMap.ContainsKey(tx.stock_symbol))
                    txMap.Add(tx.stock_symbol, new List<Transaction>());
                txMap[tx.stock_symbol].Add(new Transaction(tx.timestamp, tx.tx_quantity_delta, (float)tx.tx_price));
            }

            foreach (StockNote n in notes)
                noteMap[n.stock_symbol] = n.note_string;

            foreach (string key in txMap.Keys)
            {
                Stock s = GetStock(MakeStockUrl(key));
                if (s == null) continue;
                output.Add(new StockHistory(s, txMap[key], (noteMap.ContainsKey(key) ? noteMap[key] : String.Empty)));
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
