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
        // GET: /Facebook/UploadPhoto

        public ActionResult UploadPhoto()
        {
            return PartialView();
        }

        //
        // POST: /Facebook/UploadAndPostPhoto

        public string UploadAndPostPhoto(HttpPostedFileBase FileData, FormCollection forms)
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

            string dataString = Convert.ToBase64String(data);
            string imageString = string.Format("data:image/png;base64,{0}", dataString);

            var mediaObject = new FacebookMediaObject
            {
                ContentType = "image/png",
                FileName = "image"
            }.SetValue(data);

            string accessToken = (string)Session["accessToken"];

            var fb = new FacebookClient(accessToken);
            dynamic result = fb.Post("me/photos", new
            {
                source = mediaObject
            });

            return dataString;
        }
    }
}
