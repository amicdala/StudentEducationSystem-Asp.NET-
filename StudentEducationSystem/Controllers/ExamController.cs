using StudentEducationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace StudentEducationSystem.Controllers
{
    public class ExamController : Controller
    {
        private readonly EducationSystemContext context = new EducationSystemContext();
        private static Dictionary<int, int> questionsIdList;

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

                    questionsIdList = new Dictionary<int, int>();

                    int totalCategory = context.Categories.Count(x => x.TeacherId == teacherIdForStudent);

                    List<PerformanceCategory> performance =
                        context.PerformanceCategories.Where(x => x.StudentID == studentId).ToList();

                    int[] categoryIds = GetCategoryIdsForExam(performance);

                    int[] countOfQuestion = GetQuestionNumber(totalCategory);

                    List<Question> questions = new List<Question>();

                    for (int i = 0; i < categoryIds.Length; i++)
                    {
                        int categoryId = categoryIds[i];
                        int count = countOfQuestion[i];

                        questions.AddRange(context.Questions
                            .Where(x => x.TeacherId == teacherIdForStudent && x.CategoryId == categoryId)
                            .Take(count).ToList());
                    }

                    int countOfQue = 0;
                    for (int i = 0; i < categoryIds.Length; i++)
                    {
                        for (int j = countOfQue; j < countOfQuestion[i]; j++)
                        {
                            questionsIdList.Add(questions[j].Id, categoryIds[i]);
                        }

                        countOfQue = countOfQuestion[i];
                    }

                    return View(questions);
                }
            }

            else
            {
                int teacherIdForStudent = GetTeacherIDForStudent();

                List<Question> questions = new List<Question>();
                Random rand = new Random();
                for (int i = 0; i < 50; i++)
                {
                    var skip = (int)(rand.NextDouble() * context.Questions.Count());

                    Question question = context.Questions.OrderBy(x => x.TeacherId == teacherIdForStudent).Skip(skip).FirstOrDefault();

                    bool isSameQuestion = false;

                    foreach (var item in questions)
                    {
                        if (question != null && item.Id.Equals(question.Id))
                        {
                            isSameQuestion = true;
                            break;
                        } 
                    }

                    if (!isSameQuestion)
                    {
                        questions.Add(question);
                    }
                    else
                    {
                        i--;
                        continue;
                    }
                }

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

                string result = form[item.Key.ToString()];
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
            return RedirectToAction("Index", "Student");
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
            int[] questionNumbers = new int[categoryLength];

            for (int i = 0; i < categoryLength; i++)
                questionNumbers[i] = avg;

            for (int i = 0; i < categoryLength - 1; i++)
            {
                int additive = 0;
                for (int j = i + 1; j < categoryLength; j++)
                {
                    questionNumbers[j] = questionNumbers[j] - 1;
                    additive++;
                }
                questionNumbers[i] += additive;
            }

            questionNumbers[0] += kalan;

            int lastValue = questionNumbers[categoryLength - 1];

            if (lastValue < 0)
            {
                int lastValueAbsolute = lastValue * -1;

                for (int i = 0; i < categoryLength; i++)
                {
                    if (questionNumbers[i] == lastValueAbsolute)
                    {
                        for (int j = i; j < categoryLength; j++)
                        {
                            questionNumbers[j] = 0;
                        }
                        break;
                    }
                }
            }

            return questionNumbers;
        }

        private int[] GetCategoryIdsForExam(List<PerformanceCategory> performanceCategoryList)
        {
            int[] categoryIds = new int[performanceCategoryList.Count];
            double[] performance = new double[performanceCategoryList.Count];

            int i = 0;
            foreach (var category in performanceCategoryList)
            {
                double rate;
                if (category.FalseCounter != 0)
                    rate = category.TrueCounter / Convert.ToDouble(category.FalseCounter);
                else
                    rate = category.TrueCounter;

                performance[i] = rate;
                categoryIds[i] = category.Id;
                i++;
            }

            int n = performance.Length;
            for (i = 0; i < n - 1; i++)
                for (int j = 0; j < n - i - 1; j++)
                    if (performance[j] > performance[j + 1])
                    {
                        double temp = performance[j];
                        performance[j] = performance[j + 1];
                        performance[j + 1] = temp;
                        int temp2 = categoryIds[j];
                        categoryIds[j] = categoryIds[j + 1];
                        categoryIds[j + 1] = temp2;

                    }

            return categoryIds;
        }
    }
}