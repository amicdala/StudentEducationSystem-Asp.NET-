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
        private static Dictionary<int,int> questionsIdList;
        
        // GET: Exam
        public ActionResult TakeTheExam()
        {
            int studentId = GetStundetId();
            List<Exam> exams = context.Exams.Where(x => x.StudentId == studentId).ToList();
            if (exams.Count > 0)
            {
                DateTime lastDate = exams.OrderByDescending(y => y.Date).ToList()[0].Date;
                if (!IsToday(lastDate))
                {
                    int teacherIdForStudent = GetTeacherIDForStudent();

                    List<Question> questions = context.Questions.Where(x => x.TeacherId == teacherIdForStudent).ToList();
                    questionsIdList = new Dictionary<int, int>();

                    int totalCategory = context.Categories.Count(x => x.TeacherId == teacherIdForStudent);

                    int[] questionNumber = GetQuestionNumber(totalCategory);

                    foreach (var item in questions)
                    {
                        int categoryId = context.Questions.FirstOrDefault(x => x.Id == item.Id).CategoryId;
                        questionsIdList.Add(item.Id, categoryId);

                    }

                    return View(questions);
                }
            }
            else
            {
                int teacherIdForStudent = GetTeacherIDForStudent();

                List<Question> questions = context.Questions.Where(x => x.TeacherId == teacherIdForStudent).ToList();
                questionsIdList = new Dictionary<int, int>();

                foreach (var item in questions)
                {
                    int categoryId = context.Questions.FirstOrDefault(x => x.Id == item.Id).CategoryId;
                    questionsIdList.Add(item.Id, categoryId);

                }
                return View(questions);
            }
            ToastrService.AddToUserQueue("En son yaptığınız sınavın üzerinden en az 1 gün geçmeli.", "Sınav Olamazsınız", ToastrType.Info);
            return RedirectToAction("Index", "Student");
            
           
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

            List<ExamCategory> examCategories = new List<ExamCategory>();
            
            foreach (var item in questionsIdList)
            {
                if (form[item.Key.ToString()] == null)
                    continue;
                
                string result = form[item.Key.ToString()].ToString();
                string dbAnswer = context.Questions.FirstOrDefault(x => x.Id == item.Key).Answer;
                
                bool control = false;
                for (int i = 0; i < examCategories.Count; i++)
                {
                    if (examCategories[i].CategoryId == item.Value)
                    {
                        control = true;
                        if (result == dbAnswer)
                        {
                            examCategories[i].TrueCounter += 1;
                            trueCounter += 1;
                        }
                        else
                        {
                            examCategories[i].FalseCounter += 1;
                            falseCounter += 1;
                        }
                    }
                    
                }
                if (!control)
                {
                    ExamCategory newExamCategory = new ExamCategory();
                    newExamCategory.CategoryId = item.Value;
                    newExamCategory.TrueCounter = 0;
                    newExamCategory.FalseCounter = 0;
                    examCategories.Add(newExamCategory);

                    if (result == dbAnswer)
                    {
                        newExamCategory.TrueCounter += 1;
                        trueCounter += 1;
                    }
                    else
                    {
                        newExamCategory.FalseCounter += 1;
                        falseCounter += 1;
                    }
                }
                



            }

            
            exam.TrueCounter = trueCounter;
            exam.FalseCounter = falseCounter;
            exam.Point = trueCounter * 2;

            context.Exams.Add(exam);
            context.SaveChanges();

            foreach (var item in examCategories)
            {
                int examId = exam.Id;
                item.ExamId = examId;
                context.ExamCategories.Add(item);
            }
            context.SaveChanges();
            return RedirectToAction("Index","Student");
        }

        private int GetStundetId()
        {
            return Convert.ToInt32(Session["StudentId"]);
        }
        private int GetTeacherIDForStudent()
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);
            int teacherID = context.Students.FirstOrDefault(x => x.Id == studentId).TeacherId;
            return teacherID;
        }

        private bool IsToday(DateTime date)
        {
            return date.Day == DateTime.Now.Day && date.Month == DateTime.Now.Month && date.Year == DateTime.Now.Year;
        }

        private int[] GetQuestionNumber(int categoryLength)
        {
            int avg = 50 / categoryLength;
            int kalan = 50 - categoryLength * avg;
            int[] arr = new int[categoryLength];

            for (int i = 0; i < categoryLength; i++)
                arr[i] = avg;

            for (int i = 0; i < categoryLength - 1; i++)
            {
                int eklenecek = 0;
                for (int j = i + 1; j < categoryLength; j++)
                {
                    arr[j] = arr[j] - 1;
                    eklenecek++;
                }
                arr[i] += eklenecek;
            }

            arr[0] += kalan;

            int lastValue = arr[categoryLength - 1];

            if (lastValue < 0)
            {
                int mutlak = lastValue * -1;

                for (int i = 0; i < categoryLength; i++)
                {
                    if (arr[i] == mutlak)
                    {
                        for (int j = i; j < categoryLength; j++)
                        {
                            arr[j] = 0;
                        }
                        break;
                    }
                }
            }

            return arr;
        }
    }
}