using StudentEducationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace StudentEducationSystem.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private EducationSystemContext context = new EducationSystemContext();
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]


        public ActionResult Login(FormCollection form)
        {
            string Username = form["username"];
            string Password = form["password"];
            string Role = form["role"];

            if (LoginControl(Username, Password, Role))
            {
                if (Role == "T")
                {

                    ToastrService.AddToUserQueue(null, "Giriş Başarılı", ToastrType.Success);
                    return RedirectToAction("Index", "Teacher");
                }
                else if (Role == "S")
                {
                    ToastrService.AddToUserQueue(null, "Giriş Başarılı", ToastrType.Success);
                    return RedirectToAction("Index", "Student");
                }
                else
                {
                    ToastrService.AddToUserQueue("Lütfen giriş bilgilerinizi kontrol ediniz ", "Giriş Yapılamadı", ToastrType.Error);
                    return View();
                }

            }
            ToastrService.AddToUserQueue("Lütfen giriş bilgilerinizi kontrol ediniz ", "Giriş Yapılamadı", ToastrType.Error);
            return View();


        }



        public ActionResult Logout()
        {
            ToastrService.AddToUserQueue(new Toastr("Başarılı Bir Şekilde Gerçekleşti", "Çıkış Yapıldı", ToastrType.Success));
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public bool LoginControl(string Username, string Password, string Role)
        {
            if (Role == "T")
            {
                var teacher = context.Users.FirstOrDefault(x => x.Username == Username && x.Password == Password && x.Role == Role);
                if (teacher != null)
                {
                    FormsAuthentication.SetAuthCookie(teacher.Username.ToString(), false);
                    Session["username"] = Username;
                    return true;
                }
                else
                    return false;

            }
            else if (Role == "S")
            {
                var student = context.Users.FirstOrDefault(x => x.Username == Username && x.Password == Password && x.Role == Role);
                if (student != null)
                {
                    FormsAuthentication.SetAuthCookie(student.Username.ToString(), false);
                    return true;

                }
                else
                    return false;
            }
            else
            {
                return false;
            }

        }
    }
}