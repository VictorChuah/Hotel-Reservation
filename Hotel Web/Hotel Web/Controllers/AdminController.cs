using Hotel_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;


namespace Hotel_Web.Controllers
{


    public class AdminController : Controller
    {
        dbEntities1 db = new dbEntities1();

        //Customer===========================================================================

        public ActionResult ListCustomer()
        {
            var model = db.Customers.Where(c => c.Active == true);
            return View(model);
        }

        public ActionResult Customrdetail(string username)
        {
            var model = db.Customers.Find(username);

            if (model == null)
            {
                return RedirectToAction("ListCustomer");
            }
            return PartialView("_CustomerDetail", model);
        }

        public ActionResult CustomerEdit(string username)
        {
            var model = db.Customers.Find(username);

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



            if (ModelState.IsValid)
            {

                c.Name = model.Name;
                c.Gender = model.Gender;
                c.PhoneNo = model.PhoneNo;
                c.Username = model.Username;
                c.Email = model.Email;

                if (model.Photo != null)
                {

                    DeletePhoto(c.PhotoURL);
                    c.PhotoURL = SavePhoto(model.Photo);
                }

                db.SaveChanges();
                TempData["Info"] = "Customer record edited";
                return RedirectToAction("ListCustomer");
            }


            model.PhotoURL = c.PhotoURL;
            return View(model);

        }

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

        private string SavePhoto(HttpPostedFileBase f)
        {
            string name = Guid.NewGuid().ToString("n") + ".jpg";
            string path = Server.MapPath($"~/Photo/{name}");

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

        private void DeletePhoto(string name)
        {
            name = System.IO.Path.GetFileName(name);
            string path = Server.MapPath($"~/Photo/{name}");
            System.IO.File.Delete(path);
        }
        //=======================================================

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
        // Admin==============================================================================
        public ActionResult AdminList()
        {
            var admin = db.Admins;

            return View(admin);
        }

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
        public ActionResult AdminEdit(EditAdminDetail model)
        {
            var admin = db.Admins.Find(model.Username);

            if (admin == null)
            {
                return RedirectToAction("AdminList");
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

                    DeletePhoto(admin.PhotoURL);
                    admin.PhotoURL = SavePhoto(model.Photo);
                }

                db.SaveChanges();
                TempData["Info"] = "Admin record edited";
                return RedirectToAction("AdminList");
            }


            model.PhotoURL = admin.PhotoURL;
            return View(model);
        }

        public ActionResult ReservationList()
        {

            List<Customer> CusName = db.Customers.ToList();
            List<Reservation> Re = db.Reservations.ToList();
            List<Room> room = db.Rooms.ToList();
            List<RoomType> RT = db.RoomTypes.ToList();
            List<Service> service = db.Services.ToList();
            List<ServiceType> ST = db.ServiceTypes.ToList();

            var multipletable = from c in CusName
                                join re in Re on c.Username equals re.Username //into table1
                                //from re in table1.DefaultIfEmpty()
                                join r in room on re.RoomId equals r.Id //into table2
                                //from r in table2.DefaultIfEmpty()
                                join rt in RT on r.RoomTypeId equals rt.Id //into table3
                                //from rt in table3.DefaultIfEmpty()
                                join S in service on re.Id equals S.ReservationId //into table4
                                //from S in table4.DefaultIfEmpty()
                                join st in ST on S.ServiceId equals st.Id //into table5
                                //from st in table5.DefaultIfEmpty()
                                select new MultipleClass { Cus = c, Re = re, room = r, roomtype = rt, se = S, Setype = st };



            return View(multipletable);

        }

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
                    TempData["Info"] = "Error Invalid Status !!";
                }
            }
            else {
                TempData["Info"] = "Reservation Record Not Found!!";
            }

            string url = Request.UrlReferrer?.AbsolutePath ?? "/";
            return Redirect(url);
        }

        public ActionResult ReservationStatus(string id, int status) {

            var ReservationStatus = db.Reservations.Find(id);
            if (ReservationStatus != null) {

                if (status == 0)
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
                else if (status == 1) {

                    ReservationStatus.Status = "Check-In";
                    db.SaveChanges();
                    TempData["Info"] = "Reservation is Check-In.";

                }
            
            }

            string url = Request.UrlReferrer?.AbsolutePath ?? "/";
            return Redirect(url);
        }

        public ActionResult AddAdmin () {

            return View();
        }

    }
}