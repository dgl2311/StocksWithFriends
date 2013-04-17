using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;

namespace StocksWithFriends.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string message)
        {
            Dictionary<string, string> facebookAppIDs = new Dictionary<string, string>(){
                { "localhost", "160949207399524" },
                { "vm549-1", "435936946496485" },
                { "vm549-7", "503245086402321" }
            };

            Debug.WriteLine("Current place: " + Request.Url.AbsoluteUri);
            string appID = String.Empty;

            foreach(string s in facebookAppIDs.Keys)
                if (s.Contains(appID))
                {
                    appID = facebookAppIDs[s];
                    Debug.WriteLine("Using app ID for " + s + ": " + appID);
                }

            Session["appID"] = facebookAppIDs["localhost"];

            if (message != null)
            {
                ViewBag.Message = "LOL";
            }

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
