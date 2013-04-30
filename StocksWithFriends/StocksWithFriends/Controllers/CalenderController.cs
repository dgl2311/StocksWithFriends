using StocksWithFriends.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace StocksWithFriends.Controllers
{
    public class CalenderController : Controller
    {
        //
        // GET: /Calender/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddEvent(string title, string start, string end, Boolean allDay)
        {
            CalendarEvent calendarEvent = new CalendarEvent();
            calendarEvent.Title = title;
            calendarEvent.Start = start;
            calendarEvent.End = end;
            calendarEvent.AllDay = allDay;

            // TODO: Save to SQL

            return RedirectToAction("Index", "Calender");
        }

        public JsonResult GetEvents()
        {
            // TODO: Read from SQL
            List<CalendarEvent> events = new List<CalendarEvent>();

            CalendarEvent event1 = new CalendarEvent();
            event1.Title = "event1";
            event1.Start = "2013-04-09";
            event1.AllDay = true;
            events.Add(event1);

            CalendarEvent event2 = new CalendarEvent();
            event2.Title = "event2";
            event2.Start = "2013-04-09";
            event2.End = "2014-01-07";
            event2.AllDay = true;
            events.Add(event2);

            CalendarEvent event3 = new CalendarEvent();
            event3.Title = "event3";
            event3.Start = "2013-04-09 12:30:00";
            event3.AllDay = false;
            events.Add(event3);

            return Json(events, JsonRequestBehavior.AllowGet);
        }

    }
}
