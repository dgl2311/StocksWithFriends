using StocksWithFriends.Attributes;
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
        DBEntities _db;

        public CalenderController()
        {
            _db = new DBEntities();
        }

        //
        // GET: /Calender/Home

        public ActionResult Home()
        {
            return PartialView();
        }

        //
        // GET: /Calender/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddEvent(string name, string description, int startYear, int startMonth, int startDay,
            int startHour, int startMinute, int startSecond, int endYear, int endMonth, int endDay, int endHour,
            int endMinute, int endSecond)
        {
            CalendarEvent calendarEvent = new CalendarEvent();
            calendarEvent.event_name = name;
            calendarEvent.event_description = description;
            calendarEvent.start_timestamp = new System.DateTime(startYear, startMonth, startDay, startHour, startMinute, startSecond);
            calendarEvent.end_timestamp = new System.DateTime(endYear, endMonth, endDay, endHour, endMinute, endSecond);
            calendarEvent.user_id = (string)Session["userId"];
            calendarEvent.id = 0;

            if (_db.CalendarEvents.Count() > 0)
            {
                calendarEvent.id = _db.CalendarEvents.ToList().Last().id + 1;
            }

            _db.CalendarEvents.Add(calendarEvent);
            _db.SaveChanges();

            return RedirectToAction("Index", "Calender");
        }

        public JsonResult GetEvents()
        {
            List<JsonCalendarEvent> jsonEvents = new List<JsonCalendarEvent>();

            foreach (CalendarEvent e in _db.CalendarEvents.ToList())
            {
                if (e.user_id.Equals(Session["userId"]))
                {
                    jsonEvents.Add(new JsonCalendarEvent(e));
                }
            }

            return Json(jsonEvents, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEventsForToday()
        {
            List<JsonCalendarEvent> jsonEvents = new List<JsonCalendarEvent>();

            foreach (CalendarEvent e in _db.CalendarEvents.ToList())
            {
                if (e.user_id == Convert.ToInt64(Session["userId"]))
                {
                    System.DateTime now = System.DateTime.Now;

                    if (e.start_timestamp.Date.CompareTo(now.Date) == 0 ||
                        (e.start_timestamp.Date.CompareTo(now.Date) <= 0 &&
                         e.end_timestamp.Date.CompareTo(now.Date) >= 0)) {
                        jsonEvents.Add(new JsonCalendarEvent(e));
                    }                    
                }
            }

            return Json(jsonEvents, JsonRequestBehavior.AllowGet);
        }

    }

    class JsonCalendarEvent
    {
        public JsonCalendarEvent(CalendarEvent calendarEvent)
        {
            id = calendarEvent.id;
            user_id = calendarEvent.user_id;
            event_name = calendarEvent.event_name;
            event_description = calendarEvent.event_description;
            start_string = calendarEvent.start_timestamp.ToString("yyyy-MM-dd HH:mm:00");
            end_string = calendarEvent.end_timestamp.ToString("yyyy-MM-dd HH:mm:00");

            System.DateTime eventStart = calendarEvent.start_timestamp;
            System.DateTime eventEnd = calendarEvent.end_timestamp;
            Boolean start = (eventStart.Hour == 0 && eventStart.Minute == 0);
            Boolean end = (eventEnd.Hour == 23 && eventEnd.Minute == 59);
            all_day = (start && end);
        }

        public int id { get; set; }
        public string user_id { get; set; }
        public string event_name { get; set; }
        public string event_description { get; set; }
        public string start_string { get; set; }
        public string end_string { get; set; }
        public Boolean all_day { get; set; }
    }
}
