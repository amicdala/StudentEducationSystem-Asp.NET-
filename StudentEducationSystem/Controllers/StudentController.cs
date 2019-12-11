using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StudentEducationSystem.Controllers
{
    [Authorize(Roles = "S")]
    public class StudentController : Controller
    {
        // GET: Student

        public ActionResult Index()
        {
            return View();
        }
    }
}