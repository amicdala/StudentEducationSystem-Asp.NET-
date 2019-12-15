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
        private static List<int> questionsIdList;
        private static List<int> questionsCategoryIdList;
        // GET: Exam
        public ActionResult TakeTheExam()
        {
            int teacherIdForStudent = GetTeacherIDForStudent();
            List<Question> questions = context.Questions.Where(x => x.TeacherId == teacherIdForStudent).ToList();
            questionsIdList = new List<int>();
            questionsCategoryIdList = new List<int>(); 
            foreach (var item in questions)
            {
                questionsIdList.Add(item.Id);

                if (!questionsCategoryIdList.Contains(item.CategoryId))
                    questionsCategoryIdList.Add(item.CategoryId);
                
            }
            return View(questions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TakeTheExam(FormCollection form)
        {
            int trueCounter = 0;
            int falseCounter = 0;

            Exam exam = new Exam();
            exam.StudentId = Convert.ToInt32(Session["StudentId"]);
            exam.Date = DateTime.Now;


            foreach (var item in questionsIdList)
            {
                string result = form[item.ToString()].ToString();
                string dbAnswer = context.Questions.FirstOrDefault(x => x.Id == item).Answer;

                if (result == dbAnswer)
                {
                    trueCounter += 1;
                }
                else
                {
                    falseCounter += 1;
                }
            }

            
            exam.TrueCounter = trueCounter;
            exam.FalseCounter = falseCounter;
            exam.Point = trueCounter * 2;

            context.Exams.Add(exam);
            context.SaveChanges();
            return RedirectToAction("Index","Student");
        }

        private int GetTeacherIDForStudent()
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);
            int teacherID = context.Students.FirstOrDefault(x => x.Id == studentId).TeacherId;
            return teacherID;
        }
    }
}