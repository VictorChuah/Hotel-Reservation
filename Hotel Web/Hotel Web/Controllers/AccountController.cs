using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hotel_Web.Models;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using Microsoft.Owin.Security;
using GoogleRecaptcha.Infrastructure.Attributes;
using System.Net.Mail;

namespace Hotel_Web.Controllers
{
    public class AccountController : Controller
    {
        dbEntities1 db = new dbEntities1();
        PasswordHasher ph = new PasswordHasher();


        // --------------------------------------------------------------------
        // Security helper functions
        // --------------------------------------------------------------------

        private string HashPassword(string password)
        {
            return ph.HashPassword(password);
        }

        private bool VerifyPassword(string hash, string password)
        {
            return ph.VerifyHashedPassword(hash, password) == PasswordVerificationResult.Success;
        }

        private void SignIn(string username, string role, bool rememberMe)
        {
            var iden = new ClaimsIdentity("AUTH");
            iden.AddClaim(new Claim(ClaimTypes.Name, username));
            iden.AddClaim(new Claim(ClaimTypes.Role, role));

            var prop = new AuthenticationProperties
            {
                IsPersistent = rememberMe
            };

            Request.GetOwinContext().Authentication.SignIn(prop, iden);

        }

        private void SignOut()
        {
            Request.GetOwinContext().Authentication.SignOut();
        }

        /*private char checkUserRole(string username)
        {
            char role = ' ';

            if (db.Customers.Find(username) != null)
                role = 'c';
            else if (db.Admins.Find(username) != null)
                role = 'a';

            return role;
        }*/

        // --------------------------------------------------------------------
        // Photo helper functions
        // --------------------------------------------------------------------

        private string ValidatePhoto(HttpPostedFileBase f)
        {
            var reType = new Regex(@"^image\/(jpeg|png)$", RegexOptions.IgnoreCase);
            var reName = new Regex(@"^.+\.(jpg|jpeg|png)$", RegexOptions.IgnoreCase);

            if (f == null)
            {
                return null;
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
            string path = Server.MapPath($"~/Image/Profile/{name}");

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
            string path = Server.MapPath($"~/Image/Profile/{name}");
            System.IO.File.Delete(path);
        }

        // --------------------------------------------------------------------
        // Controller action methods
        // --------------------------------------------------------------------

        // GET: Account/Login
        public ActionResult CustLogin()
        {
            return View();
        }

        // POST: Account/Login (customer)
        [HttpPost]
        public ActionResult CustLogin(LoginModel model, string returnURL = "")
        {
            if (ModelState.IsValid)
            {
                var user = db.Customers.Find(model.Username);

                if (user == null)                                       //no such user
                    ModelState.AddModelError("Username", "Username does not exist.");

                else if (user.Blocked != null && user.Blocked > DateTime.Now)                        //blocked,is current time passed the blocked time?
                {
                    ModelState.AddModelError("Username", "This account has been blocked until " + user.Blocked + ".");
                }

                else if (!VerifyPassword(user.HashPass, model.Password)) //wrong password
                {
                    ModelState.AddModelError("Password", "Username and Password not matched.");
                    user.LoginCount++;                              //add wrong attempt
                    db.SaveChanges();

                    if (user.LoginCount >= 3)                       //if wrong attempt >=3
                    {
                        user.Blocked = DateTime.Now.AddMinutes(5); //blocked user for 5min
                        user.LoginCount = 0;                        //reset attempt
                        db.SaveChanges();
                    }
                }
                else
                {
                    SignIn(user.Username, "Member", model.RememberMe);
                    Session["PhotoURL"] = user.PhotoURL;
                    user.Blocked = null;
                    db.SaveChanges();

                    if (returnURL == "")
                    {
                        return RedirectToAction("Index", "Home");
                    }

                }
            }

            return View(model);
        }

        // GET: Account/Login (admin)
        public ActionResult AdminLogin()
        {
            return View();
        }

        // POST: Account/Login (customer)
        [HttpPost]
        public ActionResult AdminLogin(LoginModel model, string returnURL = "")
        {
            if (ModelState.IsValid)
            {
                var user = db.Admins.Find(model.Username);

                if (user != null && VerifyPassword(user.HashPass, model.Password))
                {
                    SignIn(user.Username, "Admin", model.RememberMe);
                    Session["PhotoURL"] = user.PhotoURL;

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
            SignOut();
            Session.Remove("PhotoURL");

            return RedirectToAction("Index", "Home");
        }

        // GET: Account/CheckUsername                                           
        public ActionResult CheckUsername(string username)
        {
            bool valid = ((db.Customers.Find(username) == null) && (db.Admins.Find(username) == null));
            return Json(valid, JsonRequestBehavior.AllowGet);
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateGoogleCaptcha]
        public ActionResult Register(RegisterModel model)
        {

            if (ModelState.IsValidField("Username") && db.Customers.Find(model.Username) != null && db.Admins.Find(model.Username) != null)
            {
                ModelState.AddModelError("Username", "Duplicated Username.");
            }

            string photoURL = null;
            if(model.Photo != null)
            {
                photoURL = SavePhoto(model.Photo);
            }
            
            string err = ValidatePhoto(model.Photo);
            if (err != null)
            {
                ModelState.AddModelError("Photo", err);
            }

            if (ModelState.IsValid)
            {
                var m = new Customer
                {
                    Username = model.Username,
                    Name = model.Name,
                    HashPass = HashPassword(model.Password),
                    PhoneNo = model.Phone,
                    Gender = model.Gender,
                    Email = model.Email,
                    PhotoURL = photoURL,
                    Blocked = null,
                    LoginCount = 0,
                    ResetToken = null,
                    ResetExpire = null,
                    ActiveToken = null,
                    Active = false
                };

                db.Customers.Add(m);
                db.SaveChanges();

                TempData["Info"] = "Account registered. Please login.";
                return RedirectToAction("CustLogin", "Account");
            }

            return View(model);
        }

        // GET: Account/Detail
        [Authorize]
        public ActionResult Detail()
        {
             var m = db.Customers.Find(User.Identity.Name);

            if (m == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new AccDetailModel
            {
                Name = m.Name,
                Email = m.Email,
                PhotoURL = m.PhotoURL
            };

            return View(model);
        }

        // POST: Account/Detail
        [HttpPost]
        [Authorize]
        public ActionResult Detail(AccDetailModel model)
        {
            // TODO: Get member record of the current member
            var m = db.Customers.Find(User.Identity.Name);

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

        // GET: Account/Password
        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: Account/Password
        [HttpPost]
        [Authorize]
        public ActionResult ChangePassword(ChangePassModel model)
        {
            var user = db.Customers.Find(User.Identity.Name);

            if (user == null || VerifyPassword(user.HashPass, model.Current) == false)
            {
                ModelState.AddModelError("Current", "Current Password not matched.");
            }

            if (ModelState.IsValid)
            {
                string hash = HashPassword(model.New);

                db.Customers.Find(user.Username).HashPass = hash;
                db.SaveChanges();

                TempData["Info"] = "Password updated.";
                return RedirectToAction(null);
            }

            return View(model);
        }

        // GET: Account/ForgetPass
        public ActionResult ForgetPass(forgetPassModel model)
        {
            var user = db.Customers.Find(model.Username);

            if (user == null || user.Email != model.Email)
            {
                ModelState.AddModelError("Email", "Username and email not matched.");
            }

            
            if (ModelState.IsValid)
            {
                string generate = user.Username + Guid.NewGuid().ToString("n");
                user.ResetToken = generate;
                user.ResetExpire = DateTime.Now.AddMinutes(5);

                db.SaveChanges();

                SendEmail(user,generate);

                TempData["Info"] = "Please check your email to reset password.";
                return RedirectToAction(null);
            }

            return View(model);
        }

        public ActionResult resetPass(string token, string username)
        {
            var user = db.Customers.Find(username);
            if (token != user.ResetToken || user.ResetExpire < DateTime.Now)
            {
                TempData["Info"] = "Token not exist.";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public ActionResult resetPass(ResetPassModel model, string username)
        {
            var user = db.Customers.Find(username);
            
            
        if (ModelState.IsValid)
        {
            user.HashPass = HashPassword(model.New);
            //user.ResetToken = null;
            //user.ResetExpire = null;
            db.SaveChanges();

            TempData["Info"] = "Password Updated";
            return RedirectToAction("CustLogin", "Account");
        }

            return RedirectToAction("Index","Home");
        }

        private void SendEmail(Customer user, string generate)
        {
            var m = new MailMessage();
            m.To.Add($"{user.Name} <{user.Email}>");
            m.Subject = "Reset Password";
            m.IsBodyHtml = true;

            string url = Url.Action("resetPass", "Account", new {token = generate, username = user.Username }, "http"); //if got error, change to https

            if (user.PhotoURL != null)
            {
                string path = Server.MapPath($"~/Image/Profile/{user.PhotoURL}");
                var att = new Attachment(path);
                att.ContentId = "photo";
                m.Attachments.Add(att);
            }

            m.Body = $@"
                <img src='cid:photo' style='width: 100px; height: 100px;
                                            border: 1px solid #333'>
                <p>Dear {user.Name},<p>
                <p>Please click the following link to reset your password</p>
                <h1 style='color: red'><a href='{url}'>Reset Password</a></h1>
                <p>From, Funny Hotel</p>
                <img src='https://ibb.co/ChnmYWT'>
            ";

            new SmtpClient().Send(m);
        }

    }
}