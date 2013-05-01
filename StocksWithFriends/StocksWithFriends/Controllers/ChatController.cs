using Fleck;
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
               };
    
               socket.OnMessage = message =>
               {
                   allSockets.ToList().ForEach(s => s.Send(message));
               };
           });
       }
        //
        // GET: /Chat/

        public ActionResult Chat()
        {
            return View();
        }

    }
}
