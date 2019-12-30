using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using StudentEducationSystem.Models;

namespace StudentEducationSystem.Controllers
{
    public class StatisticController : Controller
    {
        private readonly EducationSystemContext _context = new EducationSystemContext();
        public ActionResult Index()
        {
            List<DateTime> examTimes = GetExamTimes();
            List<SelectListItem> examTimesSelectList = new List<SelectListItem>();
            foreach (var item in examTimes)
            {
                examTimesSelectList.Add(new SelectListItem() {Text = item.ToString(),Value = item.ToString() });
            }

            ViewBag.ExamTimes = examTimesSelectList;

            return View();
        }

        public ActionResult LastThreeExam()
        {
            List<StudentStatisticModel> result = new List<StudentStatisticModel>();

            List<Exam> exams = GetLastThreeExamByStudentId();
            foreach (Exam item in exams)
            {
                result.Add(new StudentStatisticModel() { Point = item.Point, Date = item.Date.ToString().Split(' ')[0].ToString() });
            }

            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public ActionResult CategoryExam(string date)
        {
            int studentId = GetStudentId();
            DateTime dateTime = Convert.ToDateTime(date);
            List<Exam> exams = _context.Exams.Where(x => x.Date.Day == dateTime.Day && x.Date.Month == dateTime.Month && x.Date.Year == dateTime.Year && x.StudentId == studentId).ToList();
            List<CategoryExamStatisticModel> result = new List<CategoryExamStatisticModel>();
            CategoryExamStatisticModel temp;
            int point;
            string name;
            foreach (var item in exams)
            {
                List<ExamCategory> tempExamcategory = _context.ExamCategories.Where(x => x.ExamId == item.Id).ToList();

                foreach (var value in tempExamcategory)
                {
                    point = value.TrueCounter * 2;
                    name = _context.Categories.FirstOrDefault(x => x.Id == value.CategoryId).Name;
                    temp = new CategoryExamStatisticModel() { Point = point, Name = name };
                    result.Add(temp);
                }

            }
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        
        private List<Exam> GetLastThreeExamByStudentId()
        {
            int studentId = GetStudentId();
            return _context.Exams.Where(x => x.StudentId == studentId).OrderByDescending(x => x.Date).Take(3).OrderBy(x => x.Date).ToList();
        }

        private int GetStudentId()
        {
            return Convert.ToInt32(Session["StudentId"]);
        }
        private List<DateTime> GetExamTimes()
        {
            int studentID = GetStudentId();
            List<Exam> exams = _context.Exams.Where(x => x.StudentId == studentID).ToList();
            List<DateTime> examTimes = new List<DateTime>();
            foreach (var item in exams)
            {
                examTimes.Add(item.Date);
            }
            return examTimes;
        }
    }
}