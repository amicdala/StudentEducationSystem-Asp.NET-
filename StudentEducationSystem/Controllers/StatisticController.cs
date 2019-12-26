using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StudentEducationSystem.Models;

namespace StudentEducationSystem.Controllers
{
    public class StatisticController : Controller
    {
        private readonly EducationSystemContext _context = new EducationSystemContext();
        public ActionResult Index()
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);
            List<Exam> exams = GetLastThreeExamByStudentId(studentId);
            
            return View(exams[0]);
        }

        private List<Exam> GetLastThreeExamByStudentId(int studentId)
        {
            return _context.Exams.Where(x => x.StudentId == studentId).OrderByDescending(x => x.Date).Take(3).ToList();
        }
    }
}