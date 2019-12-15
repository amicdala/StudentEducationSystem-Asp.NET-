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
                    return RedirectToAction("Index", "Teacher");
                }
                else if (Role == "S")
                {
                    
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
            var user = context.Users.FirstOrDefault(x => x.Username == Username && x.Password == Password && x.Role == Role);
            if (user != null)
            {
                if (Role == "T")
                {
                    int teacherID = context.Teachers.FirstOrDefault(x => x.UserId == user.Id).Id;

                    FormsAuthentication.SetAuthCookie(user.Username.ToString(), false);
                    Session["username"] = Username;
                    Session["TeacherId"] = teacherID;
                    ToastrService.AddToUserQueue(null, "Giriş Başarılı", ToastrType.Success);
                    return true;
                }
                else if(Role =="S")
                {
                    int studentID = context.Students.FirstOrDefault(x => x.UserId == user.Id).Id;
                    FormsAuthentication.SetAuthCookie(user.Username.ToString(), false);
                    Session["username"] = Username;
                    Session["StudentId"] = studentID;
                    ToastrService.AddToUserQueue(null, "Giriş Başarılı", ToastrType.Success);
                    return true;
                }
            }
            return false;
           

        }
    }
}