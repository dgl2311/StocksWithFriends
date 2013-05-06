using Fleck;
using StocksWithFriends.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StocksWithFriends.Controllers
{
    public class ChatController : Controller
    {

        private static List<IWebSocketConnection> allSockets = new List<IWebSocketConnection>();
        private static ChatItem _db = new ChatItem();
        private static string userIdString = "";
        private static string userStringName = "";
        private static int storedUserId = -1;
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
                       allSockets.ToList().ForEach(s => s.Send("{\"name\":\"" + userStringName + "\"" + ",\"msg\":\"has left the chat\"}"));
                   }
                   catch (Exception e)
                   {
                       Console.WriteLine(e.Message);
                   }
                       // logChatMessage("{\"msg\":\"" + userIdString + "has left the chat\"}");
               };
    
               socket.OnMessage = message =>
               {
                   allSockets.ToList().ForEach(s => s.Send(message));
                   logChatMessage(message);
               };
           });
            //allSockets.ToList().ForEach(s => s.Send(message));
             
       }
        //
        // GET: /Chat/
        private static void logChatMessage(string message)
        {
            
            ChatLog chatLog = new ChatLog();
            chatLog.id = 0;
            chatLog.message = message;
            DateTime customDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            chatLog.timestamp = customDate;

            int numUserId = storedUserId;
            if (userIdString != null)
            {
                chatLog.user_id = Convert.ToInt32(userIdString);

                if (_db.ChatLogs.Count()>0)
                {
                    chatLog.id = _db.ChatLogs.ToList().Last().id + 1;
                }
                _db.ChatLogs.Add(chatLog);
                _db.SaveChanges();
            }            
        }
        private static int parseID(string id)
        {
            int numUserId = -1;
            try
            {
                numUserId = Int32.Parse(id);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return numUserId;
        }

        public ActionResult Chat()
        {
            userIdString = (string)Session["userId"];
            storedUserId = parseID(userIdString);
            userStringName = (string)Session["name"];
            return View();
        }
        public JsonResult GetChatHistory()
        {
            List<JsonChatHistory> jsonEvents = new List<JsonChatHistory>();

            var query = from p in _db.ChatLogs
                        where p.user_id == Convert.ToInt64(userIdString)
                        select p;
            IEnumerable<ChatLog> products = query.ToList();
            foreach (ChatLog e in products)
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
