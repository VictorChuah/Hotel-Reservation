using System;
using Hotel_Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Hotel_Web.Controllers
{
    public class HomeController : Controller
    {
        dbEntities1 db = new dbEntities1();

        public ActionResult Index()
        {
            return View();
        }

        //GET: Home/RoomList
        public ActionResult RoomList()
        {
            var model = db.RoomTypes;
            return View(model);
        }

        //GET: Home/Reserve
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

        //POST: Home/Reverse
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
                    Username = "User1",
                    RoomId = room.Id,
                    Price = room.RoomType.Price,
                    Person = room.RoomType.Person,
                    CheckIn = model.CheckIn,
                    CheckOut = model.CheckOut,
                    Paid = false,
                    Status = "Reserved"
                };

                r.Day = (r.CheckOut - r.CheckIn).Days;
                r.Price = model.RoomPrice * r.Day;
                r.Total = r.Price;

                //if (model.ServiceIds == null)
                //{
                //    return RedirectToAction("Index");
                //}
                //else
                //{
                //    TempData["Info"] = model.ServiceIds[0];
                //    return RedirectToAction("RoomList");
                //}
                //Process(2): Insert Services records
                //r.Services.Add(new Service
                //{
                //    ReservationId = "R005",
                //    ServiceId = "S001",
                //    Price = 1,
                //    Quantity = 1
                //});
                

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

                    //var st = new Service { 

                    //    ReservationId = r.Id,
                    //    ServiceId = s.Id,
                    //    Price = s.Price * q,
                    //    Quantity = q
                    //};

                    //db.Services.Add(st);

                    //NOTE: Insert AddOn through Reservation
                    r.Services.Add(new Service
                    {
                        ReservationId = r.Id,
                        ServiceId = s.Id,
                        Price = s.Price * q,
                        Quantity = q
                    });

                    // Accumulate total
                    r.Total += s.Price * r.Day * r.Person;
                }

                db.Reservations.Add(r);
                db.SaveChanges();

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

        // GET: Home/Detail
        public ActionResult Detail(string id)
        {
            var model = db.Reservations.Find(id);

            if (model == null)
            {
                return RedirectToAction("Home/Index");
            }

            return View(model);
        }

        private string NextId()
        {
            string max = db.Reservations.Max(r => r.Id) ?? "R000";
            int n = int.Parse(max.Substring(1));
            return (n + 1).ToString("'R'000");
        }

    }
}