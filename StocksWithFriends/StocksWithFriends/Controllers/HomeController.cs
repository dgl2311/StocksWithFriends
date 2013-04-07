using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StocksWithFriends.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string message)
        {
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
