using Fleck;
using StocksWithFriends.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Services;

namespace StocksWithFriends.Controllers
{
    public class ChatController : Controller
    {

        private static List<IWebSocketConnection> allSockets = new List<IWebSocketConnection>();
        private static Dictionary<string, string> nameSessionMapping = new Dictionary<string, string>();
        private static Dictionary<string, string> nameSocketMapping = new Dictionary<string, string>();


        

       protected override void Initialize(System.Web.Routing.RequestContext requestContext)
       {
           base.Initialize(requestContext);

           string userName = (string)Session["name"];
           string userId = (string)Session["userId"];
           requestContext.HttpContext.Session["name"] = userName; // do your stuff
           requestContext.HttpContext.Session["userId"] = userId;
           if (userName != null && userId != null)
           {
               if (!nameSessionMapping.ContainsKey(userName))
                   nameSessionMapping[userName] = userId;

           }
       }

        static ChatController()
        {
            
            var server = new WebSocketServer("ws://localhost:8181");
            server.Start(socket =>
            {
               socket.OnOpen = () =>
               {
                   allSockets.Add(socket);                  
               };
               socket.OnClose = () =>
               {
                   allSockets.Remove(socket);
                   try
                   {
                       if (nameSocketMapping.ContainsKey(socket.ConnectionInfo.Id.ToString()))
                       {
                           string socketId = socket.ConnectionInfo.Id.ToString();
                           string leavingUser = nameSocketMapping[socketId];
                           string leaveMessage = "{\"name\":\"" + leavingUser + "\"" + ",\"msg\":\"has left the chat\"}";
                           allSockets.ToList().ForEach(s => s.Send(leaveMessage));
                           logChatMessage(leaveMessage, socket.ConnectionInfo.Id.ToString());
                           nameSocketMapping.Remove(socketId);
                       }                      
                   }
                   catch (Exception e)
                   {
                       Console.WriteLine(e.Message);
                   }
               };
                
               socket.OnMessage = message =>
               {
                   allSockets.ToList().ForEach(s => s.Send(message));
                   logChatMessage(message,socket.ConnectionInfo.Id.ToString());
               };
           });             
       }
        //
        // GET: /Chat/
        public static void logChatMessage(string message, string socketGUID)
        {
            
            ChatLog chatLog = new ChatLog();
            chatLog.id = 0;
            chatLog.message = message;
            DateTime customDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            chatLog.timestamp = customDate;

            string userIdString = "-1";
            
            //find user name
            string[] chunk = message.Split(',');
            string[] nameMsgChunk = chunk[0].Split(':');
            nameMsgChunk[1] = nameMsgChunk[1].Replace("\\", "");
            nameMsgChunk[1] = nameMsgChunk[1].Replace("\"", "");
            string name = nameMsgChunk[1];

            if (!nameSocketMapping.ContainsKey(socketGUID))
            {
                nameSocketMapping[socketGUID] = name;
            }

            if(nameSessionMapping.ContainsKey(name))
                userIdString = nameSessionMapping[name];

            if (userIdString != "-1")
            {             
                chatLog.user_id = Convert.ToInt32(userIdString);
                
                try
                {
                    ChatItem _db = new ChatItem();
                    if (_db.ChatLogs.Count() > 0)
                    {
                        chatLog.id = _db.ChatLogs.ToList().Last().id + 1;
                    }
                    _db.ChatLogs.Add(chatLog);
                    _db.SaveChanges();
                }
                catch (Exception cc)
                {
                    Console.WriteLine(cc.Message.ToString());
                }
            }            
        }

        public ActionResult Chat()
        {
           // nameString = (string)System.Web.HttpContext.Current.Session["name"];
            //userIdString = (string)System.Web.HttpContext.Current.Session["userId"];
            
            return View();
        }

        [WebMethod(EnableSession = true)]
        public JsonResult GetChatHistory(string userName)
        {
            string userId = "-1";
            if (nameSessionMapping.ContainsKey(userName))
                userId = nameSessionMapping[userName];

            ChatItem _db = new ChatItem();

            List<JsonChatHistory> jsonEvents = new List<JsonChatHistory>();
            var startQuery = from p in _db.ChatLogs
                        where p.message.Contains("has joined the chat") && p.user_id==userId
                        select p;
            ChatLog[] startLogs = startQuery.ToList().ToArray();
            var endQuery = from p in _db.ChatLogs
                           where p.message.Contains("has left the chat") && p.user_id == userId
                        select p;
            ChatLog[] endLogs = endQuery.ToList().ToArray();
            List<ChatLog> finalHistory = new List<ChatLog>();
            if (startLogs.Count() == endLogs.Count() + 1)
            {
                for (int i = 0; i < startLogs.Count(); i++)
                {
                    if (i == startLogs.Count()-1)
                    {
                        int startIndex = startLogs[i].id;
                        var tempQuery = from p in _db.ChatLogs
                                        where p.id >= startIndex
                                        select p;
                        IEnumerable<ChatLog> winningResults = tempQuery.ToList();
                        foreach (ChatLog newLog in winningResults)
                            finalHistory.Add(newLog);
                    }
                    else
                    {
                        int startIndex = startLogs[i].id;
                        int endIndex = endLogs[i].id;
                        var tempQuery = from p in _db.ChatLogs
                                        where p.id >= startIndex && p.id <= endIndex
                                        select p;
                        IEnumerable<ChatLog> winningResults = tempQuery.ToList();
                        foreach (ChatLog newLog in winningResults)
                            finalHistory.Add(newLog);
                    }                   
                }
            }


            foreach (ChatLog e in finalHistory)
            {
                jsonEvents.Add(new JsonChatHistory(e));
            }

            return Json(jsonEvents, JsonRequestBehavior.AllowGet);
        }

    }

    class JsonChatHistory
    {
        public JsonChatHistory(ChatLog chatLog)
        {
            id = chatLog.id;
            user_id = Convert.ToInt64(chatLog.user_id);
            timestamp = chatLog.timestamp.ToString("yyyy-MM-dd HH:mm:00");
            string complexMessage = chatLog.message;
            string[] chunk = complexMessage.Split(',');
            string[] nameMsgChunk = chunk[0].Split(':');
            nameMsgChunk[1] = nameMsgChunk[1].Replace("\\", "");
            nameMsgChunk[1] = nameMsgChunk[1].Replace("\"", "");
            name = nameMsgChunk[1];

            string[] msgChunk = chunk[1].Split(':');
            msgChunk[1] = msgChunk[1].Replace("\\", "");
            msgChunk[1] = msgChunk[1].Replace("\"", "");
            msgChunk[1] = msgChunk[1].Replace("}", "");
            message = msgChunk[1];
        }

        public int id { get; set; }
        public long user_id { get; set; }
        public string message { get; set; }
        public string name {get; set; }
        public string timestamp { get; set; }
    }
}
