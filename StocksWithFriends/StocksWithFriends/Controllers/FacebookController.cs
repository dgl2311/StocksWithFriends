using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StocksWithFriends.Controllers
{
    public class FacebookController : Controller
    {
        //
        // GET: /Facebook/Index

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Facebook/Home

        public ActionResult Home()
        {
            return PartialView();
        }

        //
        // GET: /Facebook/UpdateStatus

        public ActionResult UpdateStatus()
        {
            return PartialView();
        }

    }
}
