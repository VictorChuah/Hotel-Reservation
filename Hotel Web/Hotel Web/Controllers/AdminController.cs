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

        // GET: Admin
        public ActionResult ListCustomer()
        {
            var model = db.Customers;
            return View(model);
        }

        public ActionResult Customrdetail(string username) {
            var model = db.Customers.Find(username);

            if (model == null)
            {
                return RedirectToAction("ListCustomer");
            }
            return View(model);
        }

        public ActionResult CustomerEdit(string username) {
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
    }
}