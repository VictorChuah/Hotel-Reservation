using Hotel_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using PagedList;

namespace Hotel_Web.Controllers
{


    public class AdminController : Controller
    {
        dbEntities1 db = new dbEntities1();
        PasswordHasher ph = new PasswordHasher();


        //verify photo
        //==========================================================
        private string ValidatePhoto(HttpPostedFileBase f)
        {
            var reType = new Regex(@"^image\/(jpeg|png)$", RegexOptions.IgnoreCase);
            var reName = new Regex(@"^.+\.(jpg|jpeg|png)$", RegexOptions.IgnoreCase);

            if (f == null)
            {
                return "No photo.";
            }
            else if (!reType.IsMatch(f.ContentType) || !reName.IsMatch(f.FileName))
            {
                return "Only JPG or PNG photo is allowed.";
            }
            else if (f.ContentLength > 1 * 1024 * 1024)
            {
                return "Photo size cannot more than 1MB.";
            }

            return null;
        }

        private string SavePhoto(HttpPostedFileBase f, string type)
        {
            string name = Guid.NewGuid().ToString("n") + ".jpg";
            string path = null;
            if (type == "profile") {
                path = Server.MapPath($"~/Image/Profile/{name}");
            } else if (type == "room") {
                path = Server.MapPath($"~/Image/Room/{name}");
            }
            

            var img = new WebImage(f.InputStream);

            if (img.Width > img.Height)
            {
                int px = (img.Width - img.Height) / 2;
                img.Crop(0, px, 0, px);
            }
            else
            {
                int px = (img.Height - img.Width) / 2;
                img.Crop(px, 0, px, 0);
            }

            img.Resize(201, 201)
               .Crop(1, 1)
               .Save(path, "jpeg");

            return name;
        }

        private void DeletePhoto(string name, string status)
        {
            name = System.IO.Path.GetFileName(name);
            string path = null;
            if (status == "profile")
            {
                path = Server.MapPath($"~/Image/Profile/{name}");
            }
            else if (status == "room") {
                path = Server.MapPath($"~/Image/Room/{name}");
            }
          
            System.IO.File.Delete(path);
        }
        //=======================================================



        //Customer
        //===========================================================================

        [Authorize(Roles = "Admin")]
        public ActionResult ListCustomer(string sort, string sortdir, string name = "", int page = 1 )
        {
            Func<Customer, object> fn = s => s.Username;
            switch (sort)
            { 
                case "Username": fn = s => s.Username; break;
                case "Name": fn = s => s.Name; break;
                case "Gender": fn = s => s.Gender; break;
                case "Phone": fn = s => s.PhoneNo; break;
                case "Email": fn = s => s.Email; break;
             }

            string sorted = sortdir == "DESC" ?
                      "OrderByDescending(fn)" :
                      "OrderBy(fn)";

            if (page < 1) {
               
                return RedirectToAction(null, new { page = 1 });
            }
            name = name.Trim();
            //var model = db.Customers.Where(c => c.Active == true && c => c.Name == name).FirstOrDefault();

            if (sortdir == "DESC")
            {
                var model = db.Customers.Where(c => c.Active == true && c.Name.Contains(name)).OrderBy(c => c.Username).OrderByDescending(fn).ToPagedList(page, 2);

                if (page > model.PageCount && model.PageCount != 0)
                {

                    return RedirectToAction(null, new { page = model.PageCount });

                }
                if (Request.IsAjaxRequest()) return PartialView("_CustomerList", model);

                return View(model);
            }
            else {
                var model = db.Customers.Where(c => c.Active == true && c.Name.Contains(name)).OrderBy(fn).ToPagedList(page, 2);

                if (page > model.PageCount && model.PageCount != 0)
                {

                    return RedirectToAction(null, new { page = model.PageCount });

                }

                if (Request.IsAjaxRequest()) return PartialView("_CustomerList", model);

                return View(model);
            }
             


           
        }

        //auth?
        public ActionResult Customrdetail(string username)
        {
            var model = db.Customers.Find(username);

            if (model == null)
            {
                return RedirectToAction("ListCustomer");
            }
            return PartialView("_CustomerDetail", model);
        }

        [Authorize]
        public ActionResult CustomerEdit(string username)
        {
            var model = db.Customers.Find(username);
            //TempData["info"] = model.Username;

            if (model == null)
            {
                return RedirectToAction("ListCustomer");
            }

            var m = new AdminCSEditVM
            {
                Username = model.Username,
                Name = model.Name,
                PhoneNo = model.PhoneNo,
                Gender = model.Gender,
                Email = model.Email,
                PhotoURL = model.PhotoURL

            };
           
            return View(m);

        }

        [HttpPost]
        [Authorize]
        public ActionResult CustomerEdit(AdminCSEditVM model)
        {
            var c = db.Customers.Find(model.Username);


            if (c == null)
            {
                return RedirectToAction("ListCustomer");
            }

            if (model.Photo != null)
            {

                string err = ValidatePhoto(model.Photo); // validate the photo
                if (err != null)
                {

                    ModelState.AddModelError("Photo", err);
                }
            }

            if (ModelState.IsValidField("Email"))
            {
                if (c.Email == model.Email)
                {

                }
                else if (db.Admins.Any(a => a.Email == model.Email) || db.Customers.Any(m => m.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Duplicated Email.");
                }
            }


            if (ModelState.IsValid)
            {

                c.Name = model.Name;
                c.Gender = model.Gender;
                c.PhoneNo = model.PhoneNo;
                c.Username = model.Username;
                c.Email = model.Email;

                if (model.Photo != null)
                {

                    DeletePhoto(c.PhotoURL, "profile");
                    c.PhotoURL = SavePhoto(model.Photo, "profile");
                }

                db.SaveChanges();
                TempData["Info"] = "Customer record edited";
                return RedirectToAction("ListCustomer");
            }


            model.PhotoURL = c.PhotoURL;
            return View(model);

        }

   

        [HttpPost]
        public ActionResult CustomerDelete(string username)
        {



            // TODO
            var m = db.Customers.Find(username);
            Boolean delete = false;
            if (m != null)
            {

                m.Active = delete;
                db.SaveChanges();

                TempData["Info"] = "Member Deleted.";
            }

            string url = Request.UrlReferrer?.AbsolutePath ?? "/";
            return Redirect(url);

        }
 //==================================================================================================

// reservation
//========================================================================================

        private SelectList GetYearList(int min, int max, bool reserve = false) {

            var items = new List<int>();

            for (int n = min; n <= max; n++) {
                items.Add(n);
            }
            if (reserve) items.Reverse();
            return new SelectList(items);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult ReservationList(string type, int SelectedYear = 0, string name = "", int page = 1)
        {
            TempData["Info"] = SelectedYear;
            int min = DateTime.Today.Year;
            int max = DateTime.Today.Year;

            min = db.Reservations.Min(o => o.CheckOut).Year;
            max = db.Reservations.Max(o => o.CheckOut).Year;
            ViewBag.year = GetYearList(min, max);



         

            name = name.Trim();



            var reservationList = db.Reservations.Where(c => c.Username.Contains(name));

            //search by year
            if (SelectedYear != 0)
            {
                reservationList = reservationList.Where(y => y.CheckOut.Year == SelectedYear);
            }


            var tables = reservationList.OrderByDescending(x => x.CheckOut).ToPagedList(page, 10);

            //search by username
            if (type == "username")
            {

                tables = tables.Where(r => r.Username.Contains(name)).ToPagedList(page, 10);

                if (page > tables.PageCount && tables.PageCount != 0)
                {

                    return RedirectToAction(null, new { page = tables.PageCount });

                }
                if (Request.IsAjaxRequest()) return PartialView("_ReservationList", tables);

                return View(tables);

            }
            //search by payment method
            else if (type == "Payment_Method")
            {
                tables = tables.Where(r => r.PaymentMethod.Contains(name)).ToPagedList(page, 10);

                if (page > tables.PageCount && tables.PageCount != 0)
                {

                    return RedirectToAction(null, new { page = tables.PageCount });

                }
                if (Request.IsAjaxRequest()) return PartialView("_ReservationList", tables);

                return View(tables);
            }
            //search by reservation id
            else if (type == "ReservationId") {

                tables = tables.Where(r => r.Id.Contains(name) ).ToPagedList(page, 10);

                if (page > tables.PageCount && tables.PageCount != 0)
                {

                    return RedirectToAction(null, new { page = tables.PageCount });

                }
                if (Request.IsAjaxRequest()) return PartialView("_ReservationList", tables);

                return View(tables);

            }

            if (page > tables.PageCount && tables.PageCount != 0)
            {

                return RedirectToAction(null, new { page = tables.PageCount });

            }
            if (Request.IsAjaxRequest()) return PartialView("_ReservationList", tables);

            return View(tables);





        }

        [HttpPost]
        public ActionResult ReservationUpdatePaid(string id, int status) {

            var reservationPaid = db.Reservations.Find(id);

            if (reservationPaid != null)
            {


                if (status == 1)
                {
                    reservationPaid.Paid = true;
                    db.SaveChanges();
                    TempData["Info"] = "Reservation is paid.";
                }
                else if (status == 0)
                {
                    reservationPaid.Paid = false;
                    db.SaveChanges();
                    TempData["Info"] = "Reservation is unpaid.";
                }
                else
                {
                    TempData["Info"] = "Error Invalid Status !!" + status;
                }
            }
            else {
                TempData["Info"] = "Reservation Record Not Found!!";
            }

            string url = Request.UrlReferrer?.AbsolutePath ?? "/";
            return Redirect(url);
        }

        [HttpPost]
        public ActionResult ReservationStatus(string id, string status) {

            var ReservationStatus = db.Reservations.Find(id);
            if (ReservationStatus != null) {

                if (status == "0")
                {
                    if (ReservationStatus.Paid == true)
                    {
                        ReservationStatus.Status = "Check-Out";
                        db.SaveChanges();
                        TempData["Info"] = "Reservation is Check-Out.";
                    }
                    else {
                        TempData["Info"] = "Cannot Check Out. Payment Not Paid!!";
                    }
                }
                else if (status == "1") {

                    ReservationStatus.Status = "Check-In";
                    db.SaveChanges();
                    TempData["Info"] = "Reservation is Check-In.";

                }
            
            }

            string url = Request.UrlReferrer?.AbsolutePath ?? "/";
            return Redirect(url);
        }
        //=================================================================================================================================================================


        //Admin side (display admin, edit admin, add admin)
        //================================================================================================================

        
        [Authorize(Roles = "Admin")]
        public ActionResult AdminList(string sort, string sortdir, string name = "", int page = 1)
        {
            Func<Admin, object> fn = s => s.Username;

            switch (sort)
            {
                case "Name": fn = s => s.Name; break;
                case "Phone": fn = s => s.Gender; break;
                case "Gender": fn = s => s.PhoneNo; break;
                case "Email": fn = s => s.Email; break;
            }

            if (page < 1)
            { 
                return RedirectToAction(null, new { page = 1 });
            }

            name = name.Trim();

            if (sortdir == "DESC")
            {
                var model = db.Admins.Where(c => c.Name.Contains(name)).OrderBy(c => c.Username).OrderByDescending(fn).ToPagedList(page, 2);

                if (page > model.PageCount && model.PageCount != 0)
                {

                    return RedirectToAction(null, new { page = model.PageCount });

                }
                if (Request.IsAjaxRequest()) return PartialView("_AdminList", model);

                return View(model);
            }
            else
            {
                var model = db.Admins.Where(c => c.Name.Contains(name)).OrderBy(fn).ToPagedList(page, 2);

                if (page > model.PageCount && model.PageCount != 0)
                {

                    return RedirectToAction(null, new { page = model.PageCount });

                }

                if (Request.IsAjaxRequest()) return PartialView("_AdminList", model);

                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminEdit(string username)
        {

            var model = db.Admins.Find(username);

            if (model == null)
            {
                TempData["Info"] = "Admin Not Found !!";
                return RedirectToAction("AdminList");
            }

            var m = new EditAdminDetail
            {
                Username = model.Username,
                Name = model.Name,
                PhoneNo = model.PhoneNo,
                Gender = model.Gender,
                Email = model.Email,
                PhotoURL = model.PhotoURL

            };

            return View(m);

        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AdminEdit(EditAdminDetail model)
        {
            var admin = db.Admins.Find(model.Username);

            if (admin == null)
            {
                return RedirectToAction("AdminList");
            }

            if (ModelState.IsValidField("Email"))
            {
                if (admin.Email == model.Email)
                {

                }
                else if (db.Admins.Any(a => a.Email == model.Email) || db.Customers.Any(m => m.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Duplicated Email.");
                }
            }

            if (model.Photo != null)
            {

                string err = ValidatePhoto(model.Photo); // validate the photo
                if (err != null)
                {
                    ModelState.AddModelError("Photo", err);
                }
            }



            if (ModelState.IsValid)
            {

                admin.Name = model.Name;
                admin.Gender = model.Gender;
                admin.PhoneNo = model.PhoneNo;
                admin.Username = model.Username;
                admin.Email = model.Email;

                if (model.Photo != null)
                {
                    if (admin.PhotoURL != null)
                    {
                        DeletePhoto(admin.PhotoURL, "profile");
                    }
                    Session["PhotoUrl"] = admin.PhotoURL = SavePhoto(model.Photo, "profile");
                }

                db.SaveChanges();
                TempData["Info"] = "Admin record edited";
                return RedirectToAction("AdminList");
            }


            model.PhotoURL = admin.PhotoURL;
            return View(model);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult AddAdmin() {
            return View();
        }


        private string HashPassword(string password)
        {
            return ph.HashPassword(password);
        }


        public ActionResult CheckUsername(string username)
        {
            bool result = db.Admins.Find(username) == null;
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult AddAdmin (InsertAdmin newAdmin) {

            if (db.Admins.Find(newAdmin.Username) != null)
            {

                ModelState.AddModelError("Username", "Duplicated Username.");

            }

            if (ModelState.IsValidField("Email") && (db.Admins.Any(a => a.Email == newAdmin.Email) || db.Customers.Any(c => c.Email == newAdmin.Email)))
            {
                ModelState.AddModelError("Email", "Duplicated Email.");
            }

            string err = ValidatePhoto(newAdmin.Photo);

            if (err != null)
            {

                ModelState.AddModelError("Phote", err);
            }

            if (ModelState.IsValid)
            {

                var a = new Admin
                {
                    Name = newAdmin.Name,
                    Username = newAdmin.Username,
                    Email = newAdmin.Email,
                    PhoneNo = newAdmin.PhoneNo,
                    HashPass = HashPassword(newAdmin.Password),
                    Gender = newAdmin.Gender,
                    PhotoURL = SavePhoto(newAdmin.Photo, "profile")
                };

                db.Admins.Add(a);
                db.SaveChanges();

                TempData["Info"] = "Admin Inserted.";
                return RedirectToAction("AdminList");
            }

            return View(newAdmin);

        }
        //=====================================================================================================

        //Room
        //====================================================================================================

        private string NextRoomId(string type)
        {
            string id = null;
            if (type == "Room") {
                string max = db.Rooms.Max(s => s.Id) ?? "R000";
                int n = int.Parse(max.Substring(1));
                id = (n + 1).ToString("'R'000");
            } else if (type == "RoomType") {

                string max = db.RoomTypes.Max(s => s.Id) ?? "RT000";
                int n = int.Parse(max.Substring(2));
                id = (n + 1).ToString("'RT'000");
            }
            return id;
            
        }
        public ActionResult Room(string type, string room = "",  int page = 1) {

            List<Room> R = db.Rooms.ToList();
            List<RoomType> RT = db.RoomTypes.ToList();
            room = room.Trim();
            if (page < 1)
            {
                // new {} is object without class
                return RedirectToAction(null, new { page = 1 }); // it mean if the page is 0 the system will send the user back to the same page

            }


            var display_room = from r in R
                                join rt in RT on r.RoomTypeId equals rt.Id
                                where !r.Status.Contains("D")
                               select new joinRoom { room = r, roomtype = rt};
           //=============================
            if (type == "Id")
            {

                var display = display_room.Where(c => c.room.Id.Contains(room)).ToPagedList(page, 10); 
                if (display == null) {
                    TempData["info"] = "Record Not Found!";
                }

                if (page > display.PageCount && display.PageCount != 0)
                {

                    return RedirectToAction(null, new { page = display.PageCount });

                }

                if (Request.IsAjaxRequest()) return PartialView("_Room", display);
                return View(display);

            }
            //===========================
            else if (type == "status")
            {
                var display = display_room.Where(c => c.room.Status.Contains(room)).ToPagedList(page, 10); 

                if (display == null)
                {
                    TempData["info"] = "Record Not Found!";
                }

                if (page > display.PageCount && display.PageCount != 0)
                {

                    return RedirectToAction(null, new { page = display.PageCount });

                }
                if (Request.IsAjaxRequest()) return PartialView("_Room", display);
                return View(display);
            }
            //========================
            else if (type == "name"){

                var display = display_room.Where(c => c.roomtype.Name.Contains(room)).ToPagedList(page, 10); 

                if (display == null)
                {
                    TempData["info"] = "Record Not Found!";
                }

                if (page > display.PageCount && display.PageCount != 0)
                {

                    return RedirectToAction(null, new { page = display.PageCount });

                }
                if (Request.IsAjaxRequest()) return PartialView("_Room", display);
                return View(display);
            }
            //=================
           var display_all_room = display_room.ToPagedList(page, 10);
            if (page > display_all_room.PageCount && display_all_room.PageCount != 0)
            {

                return RedirectToAction(null, new { page = display_all_room.PageCount });

            }
            if (Request.IsAjaxRequest()) return PartialView("_Room", display_all_room);
            return View(display_all_room);
        }


        public ActionResult AddRoom() {

            ViewBag.RTList = new SelectList(db.RoomTypes, "Id", "Name");
            var room = new Room {
            
                Id = NextRoomId("Room"),
                Status = "A"

            };


            return PartialView("_AddRoom");
        }

        [HttpPost]
        public ActionResult AddRoom( Room model) {

            /*TempData["Info"] = "Room Added.";
            if(roomtype == null){
                return RedirectToAction("AdminList");
            }
            */
            if (ModelState.IsValid) {
                model.Id = NextRoomId("Room");
                model.Status = "A";

                db.Rooms.Add(model);
                db.SaveChanges();
                TempData["Info"] = "Room Added.";
            }

            string url = Request.UrlReferrer?.AbsolutePath ?? "/";
            return Redirect(url);
            
            //return RedirectToAction("Room");
        }

        [HttpPost]
        public ActionResult RoomEdit(string RoomId, char status) {

            var room = db.Rooms.Find(RoomId);

            if (room == null) {
                
                TempData["Info"] = "Edit Error!!";
                return RedirectToAction("Room");
            }

            if (ModelState.IsValid) {

                if (status == 'a')
                {
                    room.Status = "A";
                }
                else if (status == 'b')
                {
                    room.Status = "B";
                }
                else if (status == 'v') {

                    room.Status = "V";
                }

                db.SaveChanges();
                TempData["Info"] = "Room Status Updated..";


            }

            string url = Request.UrlReferrer?.AbsolutePath ?? "/";
            return Redirect(url);
        }

        
        public ActionResult RoomType() {

            var roomtype = db.RoomTypes;

            return View(roomtype);
        }

        public ActionResult EditRoomType(string id) {

            var RoomType = db.RoomTypes.Find(id);

            var Rt = new editRoomType
            {
                Id = RoomType.Id,
                Name = RoomType.Name,
                Price = RoomType.Price,
                PhotoURL = RoomType.PhotoURL,
                person = RoomType.Person

            };
            TempData["Info"] = "Database"+RoomType.Id + " Metadata" +Rt.Id;

            return View(Rt);
        }

        [HttpPost]
        public ActionResult EditRoomType(editRoomType models)
        {

            var RmTe = db.RoomTypes.Find(models.Id);

            if (RmTe == null) {

                TempData["Info"] = "Not Found " + models.Id + " name " + models.Name ;
                return RedirectToAction("RoomType");
            }

            if (models.Photo != null) {
                string err = ValidatePhoto(models.Photo); // validate the photo
                if (err != null)
                {
                    ModelState.AddModelError("Photo", err);
                }
            }

            if (ModelState.IsValid)
            {

                RmTe.Name = models.Name;
                RmTe.Price = models.Price;
                RmTe.Person = models.person;

                if (models.Photo != null)
                {

                    DeletePhoto(RmTe.PhotoURL, "profile");
                    RmTe.PhotoURL = SavePhoto(models.Photo, "profile");
                }

                db.SaveChanges();
                TempData["Info"] = "Room Record edited";
                return RedirectToAction("RoomType");
            }


            models.PhotoURL = RmTe.PhotoURL;
            return View(models);
        }
        public ActionResult AddRoomType()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddRoomType(addRoomType model) {


            string err = ValidatePhoto(model.Photo);

            if (err != null)
            {

                ModelState.AddModelError("Phote", err);
            }

            if (ModelState.IsValid)
            {

                var RT = new RoomType
                {
                    Id = NextRoomId("RoomType"),
                    Name = model.name,
                    Person = model.person,
                    Price = model.Price,
                    PhotoURL = SavePhoto(model.Photo, "room")
                };

                db.RoomTypes.Add(RT);
                db.SaveChanges();

                TempData["Info"] = "Room Type Inserted.";
                return RedirectToAction("RoomType");
            }

            return View(model);
        }

        public ActionResult DeleteRoom( string roomId) {

            // TODO
            var m = db.Rooms.Find(roomId);

            if (m != null)
            {

                m.Status = "D";
                db.SaveChanges();

                TempData["Info"] = "Room Deleted.";
            }

            string url = Request.UrlReferrer?.AbsolutePath ?? "/";
            return Redirect(url);

        }
        //====================================================================================================

    }
}