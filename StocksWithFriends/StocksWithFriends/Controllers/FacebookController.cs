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
            string appId = fb.AppId;

            var mediaObject = (FacebookMediaObject)Session["mediaObject"];

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


            return RedirectToAction("Index", "Home", new { message = "Status posted to Facebook" });
        }
        public class FriendMapping
        {
            public string name { get; set; }
            public string id { get; set; }
        }

        // GET : Facebook/GetFriendId
        public string GetFriendId(string input)
        {
            Dictionary<string, FriendMapping> friendDic = (Dictionary<string, FriendMapping>)Session["dictionary"];
            string friendId = "FALSE";
            if (friendDic.ContainsKey(input))
            {
                friendId = friendDic[input].id;
            }
            return friendId;
        }
        // POST: /Facebook/getAllFriends
        public string GetAllFriends(string bla)
        {
            Dictionary<string, FriendMapping> friendDic = new Dictionary<string, FriendMapping>();
            string accessToken = (string)Session["accessToken"];

            FacebookClient fb;
            try { fb = new FacebookClient(accessToken); }
            catch (ArgumentNullException) { return String.Empty; }
            dynamic myInfo = fb.Get("/me/friends");
            List<string> names = new List<string>();
            string nameString = "";
            foreach (dynamic friend in myInfo.data)
            {
                // Response.Write("Name: " + friend.name + "<br/>Facebook id: " + friend.id + "<br/><br/>");
                names.Add(friend.name);
                nameString += friend.name + ",";
                FriendMapping newFriend = new FriendMapping();
                newFriend.name = friend.name;
                newFriend.id = friend.id;
                try { friendDic.Add(friend.name, newFriend); }
                catch (ArgumentException) { }
            }
            nameString.TrimEnd(',');
            // return PartialView(new string[]{"whyamidoingthis"});
            //return RedirectToAction("Index", "Home");
            Session["dictionary"] = friendDic;
            return nameString;
        }

        //
        // GET: /Facebook/UploadPhoto

        public ActionResult UploadPhoto()
        {
            return PartialView();
        }

        //
        // GET: /Facebook/PostToFriendsWallInitial

        public ActionResult PostToFriendsWall()
        {
            ViewBag.isReady = false;

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
