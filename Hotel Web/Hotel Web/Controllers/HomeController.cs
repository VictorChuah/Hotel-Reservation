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

        public ActionResult AboutUs() {

            return View();
        }
    }
}