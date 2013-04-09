using Facebook;
using StocksWithFriends.Models;
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

        //
        // GET: /Facebook/NewsFeed
        public ActionResult NewsFeed()
        {

            List<NewsFeedItem> newsFeedList = new List<NewsFeedItem>();

            string accessToken = (string)Session["accessToken"];
            if (accessToken != null)
            {
                var fb = new FacebookClient(accessToken);
                dynamic p = (IDictionary<string, object>)fb.Get("me/home?fields=from,to,likes,message,picture");

                dynamic data = p.data;
                foreach (dynamic post in data)
                {
                    NewsFeedItem newsPost = new NewsFeedItem();
                    newsPost.From = post.from.name;
                    newsPost.FromID = post.from.id;
                    newsPost.ID = post.id;
                    if (post.ContainsKey("to"))
                    {
                        dynamic toArray = (IDictionary<string, object>)post.to;
                        dynamic toData = toArray.data;
                        foreach (dynamic toEach in toData)
                        {
                            newsPost.To = toEach.name;
                            newsPost.ToID = toEach.id;
                        }
                    }
                    else
                    {
                        newsPost.To = null;
                        newsPost.ToID = null;
                    }
                    newsPost.Message = (post.ContainsKey("message")) ? post.message : newsPost.Message = null;
                    newsPost.Picture = (post.ContainsKey("picture")) ? post.picture : newsPost.Picture = null;
                    DateTime d = DateTime.Parse(post.created_time);
                    newsPost.Time = d.ToString();
                    newsPost.NumLikes = (post.ContainsKey("likes.count")) ? (int)post.likes.count : newsPost.NumLikes = 0;
                    if (post.ContainsKey("comments"))
                    {
                        dynamic comments = (IDictionary<string, object>)post.comments;
                        dynamic commentData = comments.data;
                        List<List<String>> commentList = new List<List<String>>();
                        foreach (dynamic individualComment in commentData)
                        {
                            dynamic commentName = individualComment.from.name;
                            dynamic commentMessage = individualComment.message;
                            commentList.Add(new List<String> { commentName, commentMessage });
                        }
                    }
                    else
                    {
                        newsPost.Comments = null;
                    }
                    newsPost.Picture = (post.ContainsKey("picture")) ? post.picture : newsPost.Picture = null;
                    if (newsPost.Message != null)
                    {
                        newsFeedList.Add(newsPost);
                    }
                }
            }
            return PartialView(newsFeedList);
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
            if (accessToken == null) return String.Empty;
            var fb = new FacebookClient(accessToken);
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
                if (!friendDic.ContainsKey(newFriend.name))
                    friendDic.Add(friend.name, newFriend);
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

        public ActionResult FriendRequest()
        {
            return View();
        }

        //
        // GET: /Facebook/AddFriend

        public ActionResult AddFriend()
        {
            return PartialView();
        }

    }
}
