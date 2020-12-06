using System;
using Hotel_Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using Nexmo.Api;

namespace Hotel_Web.Controllers
{
    public class HomeController : Controller
    {
        dbEntities1 db = new dbEntities1();

        public ActionResult Index()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Index(string Room = "")
        {
            Room = Room.Trim();
            var roomTypes = db.RoomTypes.Where(r => r.Name.Contains(Room)).FirstOrDefault();
            if (roomTypes != null)
            {
                return RedirectToAction("Reserve", "Home", new { RoomTypeId = roomTypes.Id });
            }
            else
            {
                TempData["Info"] = "Dont have this type of room.";
            }


            return View();
        }

        //GET: Home/RoomList
        public ActionResult RoomList()
        {
            var model = db.RoomTypes;
            return View(model);
        }

        //GET: Home/Reserve
        [Authorize]
        public ActionResult Reserve(string RoomTypeId)
        {
            var m = db.RoomTypes.Find(RoomTypeId);
            var model = new ReserveVM
            {
                RoomTypeId = m.Id,
                RoomTypeName = m.Name,
                RoomPrice = m.Price,
                RoomPhotoURL = m.PhotoURL

            };
            ViewBag.ServiceList = new MultiSelectList(db.ServiceTypes, "Id", "Name", model.ServiceIds);

            return View(model);
        }

        //POST: Home/Reserve
        [Authorize]
        [HttpPost]
        public ActionResult Reserve(ReserveVM model)
        {
            DateTime min = DateTime.Today;
            DateTime max = DateTime.Today.AddDays(30);

            // Validation (1): CheckIn within 30 days range
            if (ModelState.IsValidField("CheckIn"))
            {
                if (model.CheckIn < min || model.CheckIn > max)
                {
                    ModelState.AddModelError("CheckIn", "Date out of range.");
                }
            }

            // Validation (2): CheckOut within 30 days range
            if (ModelState.IsValidField("CheckOut"))
            {
                if (model.CheckOut < min || model.CheckOut > max)
                {
                    ModelState.AddModelError("CheckOut", "Date out of range.");
                }
            }

            // Validation (3): CheckOut > CheckIn
            if (ModelState.IsValidField("CheckIn") && ModelState.IsValidField("CheckOut"))
            {
                if (model.CheckOut <= model.CheckIn)
                {
                    ModelState.AddModelError("CheckOut", "CheckOut must be after CheckIn.");
                }
            }

            Room room = null;

            // Validation (4): Room available
            if (ModelState.IsValidField("CheckIn") && ModelState.IsValidField("CheckOut"))
            {
                var occupied = db.Reservations
                                 .Where(r => model.CheckIn < r.CheckOut && r.CheckIn < model.CheckOut)
                                 .Select(r => r.Room);

                room = db.Rooms
                         .Except(occupied)
                             .FirstOrDefault(r => r.RoomTypeId == model.RoomTypeId);

                if (room == null)
                {
                    ModelState.AddModelError("RoomTypeName", "No room availble.");
                }

            }


            if (ModelState.IsValid)
            {

                // Process (1): Insert Reservation record
                var r = new Reservation
                {
                    Id = NextId(),
                    Username = User.Identity.Name,
                    RoomId = room.Id,
                    Price = room.RoomType.Price,
                    Person = room.RoomType.Person,
                    CheckIn = model.CheckIn,
                    CheckOut = model.CheckOut,
                    Paid = false,
                    Status = "Reserved"
                };

                r.Day = (r.CheckOut - r.CheckIn).Days;
                r.Total = r.Price * r.Day;

                var servicesType = db.ServiceTypes.Where(s => model.ServiceIds.Contains(s.Id));
                foreach (var s in servicesType)
                {

                    int q = 1;
                    if (s.Name == "Bed")
                    {
                        q = model.Bed;
                    }
                    else if (s.Name == "Blanket")
                    {
                        q = model.Blanket;
                    }

                    //NOTE: Insert AddOn through Reservation
                    r.Services.Add(new Service
                    {
                        //ReservationId = r.Id,
                        ServiceId = s.Id,
                        Price = s.Price,
                        Quantity = q
                    });

                    // Accumulate total
                    r.Total += s.Price * r.Day * r.Person * q;

                }

                db.Reservations.Add(r);
                db.SaveChanges();

                SendEmail(r.Username, r.Id);

                var user = db.Customers.Find(User.Identity.Name);
                var results = SMS.Send(new SMS.SMSRequest
                {
                    from = Configuration.Instance.Settings["appsettings:NEXMO_FROM_NUMBER"],
                    to = user.PhoneNo,
                    
                    text = "Dear "      + user.Name     +
                    @", your reservation is successful! Here is your reservation detail. 
                    Room : "            + r.Room.RoomType.Name + @" Room
                    Check In  : "       + r.CheckIn     + @" 12:00pm
                    Check Out : "       + r.CheckOut    + @"12:00pm
                    Total     : RM"     + r.Total       + @"
                    Paid      : "       + r.Paid        + @" 
                    More detail can review in website
                    From, Super Admin"
                });


                TempData["Info"] = "Room reserved.";
                return RedirectToAction("Detail", new { r.Id });
            }
            var m = db.RoomTypes.Find(model.RoomTypeId);
            var model1 = new ReserveVM
            {
                RoomTypeId = m.Id,
                RoomTypeName = m.Name,
                RoomPrice = m.Price,
                RoomPhotoURL = m.PhotoURL,
                ServiceIds = model.ServiceIds

            };
            ViewBag.ServiceList = new MultiSelectList(db.ServiceTypes, "Id", "Name", model1.ServiceIds);
            return View(model1);
        }
        private void SendEmail(string name, string Rid)
        {
            var user = db.Customers.Find(name);
            var r = db.Reservations.Find(Rid);

            var m = new MailMessage();
            m.To.Add($"{user.Name} <{user.Email}>");
            m.Subject = "Reservation Receipt";
            m.IsBodyHtml = true;

            m.Body = $@"
                <p>Dear {user.Name},<p>
                <p>Your reservation is successful<p>
                <p>Here is your reservation detail<p>
                <p>Room      : {r.Room.RoomType.Name} Room<p>
                <p>Check In  : {r.CheckIn}  12:00pm<p>
                <p>Check Out : {r.CheckOut} 12:00pm<p>
                <p>Total     : RM{r.Total} <p>
                <p>Paid      : {r.Paid} <p>
                <p>More detail can review in website<p>
                <p>From, 👷 Super Admin</p>

            ";

            new SmtpClient().Send(m);
        }


        [Authorize]
        // GET: Home/Detail
        public ActionResult Detail(string id)
        {
            var model = db.Reservations.Find(id);

            if (model == null || model.Username != User.Identity.Name)
            {
                return RedirectToAction("Home/Index");
            }

            return View(model);
        }

        [Authorize]
        //GET: Home/Reserve History
        public ActionResult ReserveHistory()
        {
            var model = db.Reservations.Where(r => r.Username == User.Identity.Name);
            return View(model);
        }

        // POST: Home/Reset
        [HttpPost]
        public ActionResult Reset()
        {
            db.Services.RemoveRange(db.Services);
            db.Reservations.RemoveRange(db.Reservations);
            db.SaveChanges();

            //db.Database.ExecuteSqlCommand(@"
            //    DBCC CHECKIDENT([Reservation], RESEED, 0) 
            //");

            return RedirectToAction("ReserveHistory");
        }

        private string NextId()
        {
            string max = db.Reservations.Max(r => r.Id) ?? "R000";
            int n = int.Parse(max.Substring(1));
            return (n + 1).ToString("'R'000");
        }

        //show location
        public ActionResult Location() {

            return View();
        }

        //show 
        public ActionResult Chat()
        {
            return View();
        }

    }
}