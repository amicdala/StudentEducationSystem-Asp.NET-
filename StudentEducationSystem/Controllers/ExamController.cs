using StudentEducationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StudentEducationSystem.Controllers
{
    public class ExamController : Controller
    {
        private EducationSystemContext context = new EducationSystemContext();
        private static List<int> questionsId = new List<int>();
        // GET: Exam
        public ActionResult TakeTheExam()
        {
            int teacherIdForStudent = GetTeacherIDForStudent();
            List<Question> questions = context.Questions.Where(x => x.TeacherId == teacherIdForStudent).ToList();
            
            return View(questions);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TakeTheExam(FormCollection form)
        {
            int teacherIdForStudent = GetTeacherIDForStudent();
            List<Question> questions = context.Questions.Where(x => x.TeacherId == teacherIdForStudent).ToList();
            foreach (var item in questions)
                questionsId.Add(item.Id);
            

            return View(questions);
        }

        private int GetTeacherIDForStudent()
        {

            string userName = Session["UserName"].ToString();
            int userID = context.Users.FirstOrDefault(x => x.Username == userName).Id;
            int teacherID = context.Students.FirstOrDefault(x => x.UserId == userID).TeacherId;
            return teacherID;
        }
    }
}