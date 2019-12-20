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
            int teacherID = GetTeacherID();
            List<Student> students = context.Students.Where(x => x.TeacherId == teacherID).ToList();
            List<User> users = new List<User>();
            foreach (Student item in students)
            {
                users.Add(context.Users.FirstOrDefault(x => x.Id == item.UserId));
            }
            
            return View(users);
        }

        private int GetTeacherID()
        {
            return Convert.ToInt32(Session["TeacherId"]);
        }



    }
}