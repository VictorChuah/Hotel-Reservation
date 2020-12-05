using Hotel_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Hotel_Web.Controllers
{
    public class ReportController : Controller
    {
        dbEntities1 db = new dbEntities1();

        // GET: Report
        public ActionResult ReservationReport()
        {
            return View();
        }

        public ActionResult Data1()
        {
            var dt = db.Reservations
                       .GroupBy(r => r.Room.RoomType.Name)
                       .ToList()
                       .Select(g => new object[] { g.Key, g.Count() });

            return Json(dt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ServicesReport()
        {
            return View();
        }

        public ActionResult Data2()
        {
            var dt = db.Services
                       .GroupBy(r => r.ServiceType.Name)
                       .ToList()
                       .Select(g => new object[] { g.Key, g.Count() });

            return Json(dt, JsonRequestBehavior.AllowGet);
        }
    }
}