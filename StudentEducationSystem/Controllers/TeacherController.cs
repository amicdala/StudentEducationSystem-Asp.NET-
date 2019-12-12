using StudentEducationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace StudentEducationSystem.Controllers
{
    [Authorize(Roles = "T")]
    public class TeacherController : Controller
    {
        private EducationSystemContext context = new EducationSystemContext();

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Profil()
        {
            return View();
        }

        public ActionResult MyStudents()
        {
            
            return View();
        }

        

    }
}