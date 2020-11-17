using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
// Additional using statements
using Hotel_Web.Models;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using Microsoft.Owin.Security;

namespace Demo.Controllers
{
    public class AccountController : Controller
    {
        dbEntities1 db = new dbEntities1();
        // TODO: Initialize password hasher
        PasswordHasher ph = new PasswordHasher();


        // --------------------------------------------------------------------
        // Security helper functions
        // --------------------------------------------------------------------

        private string HashPassword(string password)
        {
            // TODO: Return hashed password
            return ph.HashPassword(password);
        }

        private bool VerifyPassword(string hash, string password)
        {
            // TODO: Verify hashed password (true or false)
            return ph.VerifyHashedPassword(hash, password) == PasswordVerificationResult.Success;
        }

        private void SignIn(string username, bool rememberMe)
        {
            // TODO(1): Identity and claims
            var iden = new ClaimsIdentity("AUTH");
            iden.AddClaim(new Claim(ClaimTypes.Name, username));

            // TODO(2): Remember me
            var prop = new AuthenticationProperties
            {
                IsPersistent = rememberMe
            };

            // TODO(3): Sign in
            Request.GetOwinContext().Authentication.SignIn(prop, iden);

        }

        private void SignOut()
        {
            // TODO: Sign out
            Request.GetOwinContext().Authentication.SignOut();
        }

        // --------------------------------------------------------------------
        // Photo helper functions
        // --------------------------------------------------------------------

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

        // --------------------------------------------------------------------
        // Controller action methods
        // --------------------------------------------------------------------

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public ActionResult Login(LoginModel model, string returnURL = "")
        {
            if (ModelState.IsValid)
            {
                // TODO: Get user record based on username
                var user = db.Customers.Find(model.Username);

                // TODO: AND if password matches
                if (user != null && VerifyPassword(user.HashPassword, model.Password))
                {
                    // TODO: Sign in user + session
                    SignIn(user.Username, model.RememberMe);
                    Session["PhotoURL"] = user.PhotoURL;

                    // TODO: Handle return URL
                    if (returnURL == "")
                    {
                        return RedirectToAction("Index", "Home");
                    }

                }
                else
                {
                    ModelState.AddModelError("Password", "Username and Password not matched.");
                }
            }

            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            // TODO: Sign out user + session
            SignOut();
            Session.Remove("PhotoURL");

            return RedirectToAction("Index", "Home");
        }

        // GET: Account/CheckUsername
        public ActionResult CheckUsername(string username)
        {
            // TODO: Check if username not duplicated.
            bool valid = db.Customers.Find(username) == null;
            return Json(valid, JsonRequestBehavior.AllowGet);
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        public ActionResult Register(RegisterVM model)
        {
            // TODO: AND if username duplicated
            if (ModelState.IsValidField("Username") && db.Users.Find(model.Username) != null)
            {
                ModelState.AddModelError("Username", "Duplicated Username.");
            }

            string err = ValidatePhoto(model.Photo);
            if (err != null)
            {
                ModelState.AddModelError("Photo", err);
            }

            if (ModelState.IsValid)
            {
                var m = new Member
                {
                    Username = model.Username,
                    Hash = HashPassword(model.Password), // TODO: Generate password hash
                    Name = model.Name,
                    Email = model.Email,
                    PhotoURL = SavePhoto(model.Photo)
                };

                db.Members.Add(m);
                db.SaveChanges();

                TempData["Info"] = "Account registered. Please login.";
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        // TODO: Authorize (Member)
        // GET: Account/Detail
        [Authorize(Roles = "Member")]
        public ActionResult Detail()
        {
            // TODO: Get member record of the current member
            var m = db.Members.Find(User.Identity.Name);

            if (m == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new DetailVM
            {
                Name = m.Name,
                Email = m.Email,
                PhotoURL = m.PhotoURL
            };

            return View(model);
        }

        // TODO: Authorize (Member)
        // POST: Account/Detail
        [HttpPost]
        [Authorize(Roles = "Member")]
        public ActionResult Detail(DetailVM model)
        {
            // TODO: Get member record of the current member
            var m = db.Members.Find(User.Identity.Name);

            if (m == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (model.Photo != null)
            {
                string err = ValidatePhoto(model.Photo);
                if (err != null)
                {
                    ModelState.AddModelError("Photo", err);
                }
            }

            if (ModelState.IsValid)
            {
                if (model.Photo != null)
                {
                    DeletePhoto(m.PhotoURL);
                    m.PhotoURL = SavePhoto(model.Photo);

                    // TODO: Update session
                    Session["PhotoUrl"] = m.PhotoURL = SavePhoto(model.Photo);

                }

                m.Name = model.Name;
                m.Email = model.Email;
                db.SaveChanges();

                TempData["Info"] = "Detail updated.";
                return RedirectToAction(null);
            }

            model.PhotoURL = m.PhotoURL;
            return View(model);
        }

        // TODO: Authorize
        // GET: Account/Password
        [Authorize]
        public ActionResult Password()
        {
            return View();
        }

        // TODO: Authorize
        // POST: Account/Password
        [HttpPost]
        [Authorize]
        public ActionResult Password(PasswordVM model)
        {
            // TODO: Get user record of the current user
            var user = db.Users.Find(User.Identity.Name);

            // TODO: OR if password not matches
            if (user == null || VerifyPassword(user.Hash, model.Current) == false)
            {
                ModelState.AddModelError("Current", "Current Password not matched.");
            }

            if (ModelState.IsValid)
            {
                // TOOD: Generate password hash
                string hash = HashPassword(model.New);

                // TODO: Update member or admin record
                if (user.Role == "Member")
                {
                    db.Members.Find(user.Username).Hash = hash;
                }
                else if (user.Role == "Admin")
                {
                    db.Admins.Find(user.Username).Hash = hash;
                }

                db.SaveChanges();

                TempData["Info"] = "Password updated.";
                return RedirectToAction(null);
            }

            return View(model);
        }

        // --------------------------------------------------------------------
        // More to come...
        // --------------------------------------------------------------------

        // GET: Account/Reset
        public ActionResult Reset()
        {
            return View();
        }

    }
}