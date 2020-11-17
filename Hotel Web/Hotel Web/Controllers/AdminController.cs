using Hotel_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

        public ActionResult Customrdetail(string id) {
            var model = db.Customers.Find(id);

            if (model == null)
            {
                return RedirectToAction("ListCustomer");
            }
            return View(model);
        }

        public ActionResult CustomerEdit(string id) {
            var model = db.Customers.Find(id);

            if (model == null)
            {
                return RedirectToAction("ListCustomer");
            }

            return View(model);
            
        }

        [HttpPost]
        public ActionResult CustomerEdit(Customer model)
        {
            var c = db.Customers.Find(model.Name);

            if (model == null)
            {
                return RedirectToAction("ListCustomer");
            }

            

            if (ModelState.IsValid)
            {

                c.Name = model.Name;
                c.Gender = model.Gender;
                c.PhoneNo = model.PhoneNo;
                c.Username = model.Username;
                c.Email = model.Email;

                db.SaveChanges();
                TempData["Info"] = "Customer record edited";
                return RedirectToAction("ListCustomer");
            }

            

            return View();

        }
    }
}