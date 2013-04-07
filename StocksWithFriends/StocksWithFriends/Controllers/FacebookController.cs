using Facebook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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

        //
        // POST: /Facebook/UpdateAndPostStatus

        public ActionResult UpdateAndPostStatus(string statusMessage)
        {
            string accessToken = (string)Session["accessToken"];

            var fb = new FacebookClient(accessToken);
            var mediaObject = (FacebookMediaObject)Session["mediaObject"];
            /*
            if (mediaObject == null)
            {
                dynamic result = fb.Post("me/feed", new
                {
                    message = statusMessage
                });
            }
            else
            {
                dynamic result = fb.Post("me/photos", new
                {
                    message = statusMessage,
                    source = mediaObject
                });

                Session["mediaObject"] = null;
            }
            */

            return RedirectToAction("Index", "Home", new { message = "Status posted to Facebook" });
        }

        //
        // GET: /Facebook/UploadPhoto

        public ActionResult UploadPhoto()
        {
            return PartialView();
        }

        //
        // POST: /Facebook/UploadAndPostPhoto

        public void UploadAndPostPhoto(HttpPostedFileBase FileData, FormCollection forms)
        {
            byte[] data;

            using (Stream inputStream = FileData.InputStream)
            {
                MemoryStream memoryStream = inputStream as MemoryStream;
                if (memoryStream == null)
                {
                    memoryStream = new MemoryStream();
                    inputStream.CopyTo(memoryStream);
                }

                data = memoryStream.ToArray();
            }

            var mediaObject = new FacebookMediaObject
            {
                ContentType = "image/png",
                FileName = "image"
            }.SetValue(data);

            Session["mediaObject"] = mediaObject;
        }
    }
}
