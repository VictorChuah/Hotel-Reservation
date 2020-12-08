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

        public ActionResult SalesReportByYear()
        {
            return View();
        }

        public ActionResult Data3()
        {
            var dt = db.Reservations
                       .GroupBy(r => r.CheckOut.Year)
                       .ToList()
                       .Select(g => new object[] {
                           g.Key.ToString(),
                           g.Sum(r => r.Total)
                       });

            return Json(dt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReservationList()
        {
            var model = db.Reservations;

            return View(model);
        }

        public ActionResult SalesReportByMonth()
        {
            // Return available years for DropDownList
            int min = DateTime.Today.Year;
            int max = DateTime.Today.Year;
            if (db.Reservations.Count() > 0)
            {
                min = db.Reservations.Min(o => o.CheckOut).Year;
                max = db.Reservations.Max(o => o.CheckOut).Year;
            }
            ViewBag.YearList = GetYearList(min, max);

            return View();
        }

        public ActionResult Data4(int year = 0)
        {
            var dict = new Dictionary<string, decimal>
            {
                ["Jan"] = 0m,
                ["Feb"] = 0m,
                ["Mar"] = 0m,
                ["Apr"] = 0m,
                ["May"] = 0m,
                ["Jun"] = 0m,
                ["Jul"] = 0m,
                ["Aug"] = 0m,
                ["Sep"] = 0m,
                ["Oct"] = 0m,
                ["Nov"] = 0m,
                ["Dec"] = 0m,
            };

            // Fill database records
            db.Reservations
              .Where(r => r.CheckOut.Year == year)
              .GroupBy(r => r.CheckOut.Month)
              .ToList()
              .ForEach(g => dict[GetMonthAbbr(g.Key)] = g.Sum(r => r.Total));

            var dt = dict.Select(i => new object[] { i.Key, i.Value });

            return Json(dt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SalesReportByDay()
        {
            // Return available years
            int min = DateTime.Today.Year;
            int max = DateTime.Today.Year;
            if (db.Reservations.Count() > 0)
            {
                min = db.Reservations.Min(o => o.CheckOut).Year;
                max = db.Reservations.Max(o => o.CheckOut).Year;

            }
            ViewBag.YearList = GetYearList(min, max);

            // Return months
            ViewBag.MonthList = GetMonthList();

            // Default selections (largest year and month)
            if (db.Reservations.Count() > 0)
            {
                ViewBag.Year = db.Reservations.Max(o => o.CheckOut).Year;
                ViewBag.Month = db.Reservations.Max(o => o.CheckOut).Month;
            }

            return View();
        }

        public ActionResult Data5(int year = 0, int month = 0)
        {
            var dt = db.Reservations
                       .Where(r => r.CheckOut.Year == year &&
                                    r.CheckOut.Month == month)
                       .GroupBy(r => r.CheckOut)
                       .ToList()
                       .Select(g => new object[] {
                           g.Key.ToString("yyyy-MM-dd"),
                           g.Sum(r => r.Total)
                       });

            return Json(dt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CompareByDaySales()
        {
            int min = DateTime.Today.Year;
            int max = DateTime.Today.Year;
            if (db.Reservations.Count() > 0)
            {
                min = db.Reservations.Min(o => o.CheckOut).Year;
                max = db.Reservations.Max(o => o.CheckOut).Year;
            }
            ViewBag.YearList = GetYearList(min, max);

            // Return months
            ViewBag.MonthList = GetMonthList();

            // Default selections (largest year and month)
            if (db.Reservations.Count() > 0)
            {
                ViewBag.Year  = db.Reservations.Max(o => o.CheckOut).Year;
                ViewBag.Month = db.Reservations.Max(o => o.CheckOut).Month;
            }

            // TODO: Return product select list
            ViewBag.RoomList = new SelectList(db.RoomTypes, "Id", "Name");

            return View();
        }

        public ActionResult Data6(int year, int month, string r1, string r2)
        {
            // TODO: Return overall sales by day (filter by year and month)
            //       On the selected 2 products
            var dt = db.Reservations
                       .Where(r => r.CheckOut.Year == year &&
                                    r.CheckOut.Month == month)
                       .GroupBy(r => r.CheckOut)
                       .ToList()
                       .Select(g => new object[] {
                           g.Key.ToString("yyyy-MM-dd"),
                           g.Where(r => r.Room.RoomType.Id == r1).Sum(r => r.Price * r.Day),
                           g.Where(r => r.Room.RoomType.Id == r2).Sum(r => r.Price * r.Day)
                       });

            return Json(dt, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ServicesRevenueByMonth()
        {
            DateTime min = DateTime.Today;
            DateTime max = DateTime.Today;

            if (db.Reservations.Count() > 0)
            {
                min = db.Reservations.Min(o => o.CheckOut);
                max = db.Reservations.Max(o => o.CheckOut);
            }

            ViewBag.Min = min.ToString("yyyy-MM-dd");
            ViewBag.Max = ViewBag.Date = max.ToString("yyyy-MM-dd");
            // TODO: Return product list
            ViewBag.Services = db.Services.Select(st => new { st.ServiceType.Id, st.ServiceType.Name, st.Price, st.Quantity });

            return View();
        }

        public ActionResult Data7(DateTime date)
        {
            // TODO: Return overall quantity sold by product (filter by day)
            var dt = db.Services
                       .Where(s => s.Reservation.CheckOut == date)
                       .GroupBy(s => s.ServiceType)
                       .ToList()
                       .Select(g => new object[] {
                           g.Key.Id,
                           g.Sum(s => s.Quantity * s.ServiceType.Price)
                       });

            return Json(dt, JsonRequestBehavior.AllowGet);
        }

        // Helper -------------------------------------------------------------------------------------
        private SelectList GetYearList(int min, int max, bool reverse = false)
        {
            var items = new List<int>();
            for (int n = min; n <= max; n++)
            {
                items.Add(n);
            }
            if (reverse) items.Reverse();
            return new SelectList(items);
        }

        private SelectList GetMonthList()
        {
            var items = new List<object>();
            for (int n = 1; n <= 12; n++)
            {
                items.Add(new { Id = n, Name = new DateTime(1, n, 1).ToString("MMMM") });
            }
            return new SelectList(items, "Id", "Name");
        }

        // Given month id (1-12), return month abbrevation (Jan-Dec)
        private string GetMonthAbbr(int n)
        {
            return new DateTime(1, n, 1).ToString("MMM");
        }

        // Given month id (1-12), return month name (January-December)
        private string GetMonthName(int n)
        {
            return new DateTime(1, n, 1).ToString("MMMM");
        }

        [HttpPost]
        public ActionResult ResetAll()
        {
            db.Services.RemoveRange(db.Services);
            db.Reservations.RemoveRange(db.Reservations);
            db.SaveChanges();

            //db.Database.ExecuteSqlCommand(@"
            //    DBCC CHECKIDENT([Reservation], RESEED, 0) 
            //");
            TempData["Info"] = "Database reset.";
            return RedirectToAction("ReservationList");
        }

        [HttpPost]
        public ActionResult Generate()
        {
            DateTime a = new DateTime(2016, 01, 01); // Start date --> 01 May 2016
            DateTime b = new DateTime(2020, 12, 01 ); // End date ----> 31 August 2020
            string username = db.Customers.First().Username; // Retrieve first member's username
            
            List<ServiceType> servicesTypes = db.ServiceTypes.ToList(); // Retrieve a copy of services first
            
            Random r = new Random();
            int ReservationCount = 0;
            int ServicesCount = 0;

            DateTime timer = DateTime.Now;

            // Disable DB tracking and validation --> fast performance
            //db.Configuration.AutoDetectChangesEnabled = false;
            //db.Configuration.ValidateOnSaveEnabled = false;

            for (DateTime d = a; d <= b; d = d.AddDays(+1))
            {
                List<Reservation> rt = db.Reservations.ToList();
                List<Room> rooms = db.Rooms.ToList(); // Retrieve a copy of rooms first

                foreach (var rn in rt)
                {
                    if (rn.Status == "Reserved" && rn.CheckOut.CompareTo(d) < 0)
                    {
                        rn.Room.Status = "A";
                        rn.Status = "Check-Out";
                        db.SaveChanges();
                    }
                }

                int count = r.Next(1, 3); // 1-2 reservation per day
                for (int n = 1; n <= count; n++)
                {
                    foreach (Room ro in rooms)
                    {
                        
                        if (r.Next(1, 3) == 1) 
                        {
                            if (ro.Status != "V")
                            {
                                // (1) Add Reservation
                                ro.Status = "V";
                                db.SaveChanges();
                                var rs = new Reservation();
                                rs.Id = NextId();
                                rs.Username = username;
                                rs.RoomId = ro.Id;
                                rs.CheckIn = d;
                                rs.CheckOut = d.AddDays(+(r.Next(1, 6)));
                                rs.Price = ro.RoomType.Price;
                                rs.Person = ro.RoomType.Person;
                                rs.Day = (rs.CheckOut - rs.CheckIn).Days;
                                rs.Paid = true;
                                if (r.Next(1, 3) == 1)
                                {
                                    rs.PaymentMethod = "PayPal";
                                }
                                else
                                {
                                    rs.PaymentMethod = "Walk In Pay";
                                }
                                rs.Status = "Reserved";
                                rs.Total = rs.Price * rs.Day;

                                // (2) Add services
                                foreach (ServiceType st in servicesTypes)
                                {
                                    if (r.Next(1, 3) == 1) // 1-2. If 1 take the services
                                    {
                                        int q = 1;
                                        if (st.Id == "S009")
                                        {
                                            q = r.Next(1, 6);
                                        }
                                        else if (st.Id == "S010")
                                        {
                                            q = r.Next(1, 6);
                                        }

                                        //NOTE: Insert AddOn through ReservationB
                                        rs.Services.Add(new Service
                                        {
                                            ReservationId = rs.Id,
                                            ServiceId = st.Id,
                                            Price = st.Price,
                                            Quantity = q
                                        });

                                        // Accumulate total
                                        rs.Total += st.Price * rs.Day * rs.Person * q;
                                        ServicesCount++;
                                    }
                                }
                                db.Reservations.Add(rs);
                                ReservationCount++;
                           }
                        }
                        db.SaveChanges();

                    }
                    
                }
            }
            

            // Reenable DB tracking and validation after saved
            db.Configuration.AutoDetectChangesEnabled = true;
            db.Configuration.ValidateOnSaveEnabled = true;

            TimeSpan span = (DateTime.Now - timer);

            TempData["Info"] = $@"
                {ReservationCount} Reservation and {ServicesCount} Services generated.
                Dates between {a.ToString("yyyy-MM-dd")} and {b.ToString("yyyy-MM-dd")}.
                Total time taken = {span}.
            ";
            return RedirectToAction("ReservationList");
        }

        private string NextId()
        {
            string max = db.Reservations.Max(r => r.Id) ?? "R00000";
            int n = int.Parse(max.Substring(1));
            return (n + 1).ToString("'R'00000");
        }

    }
}