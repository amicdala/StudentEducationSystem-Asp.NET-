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

                    Random rand = new Random();
                    int n = questions.Count;
                    while (n > 1)
                    {
                        n--;
                        int k = rand.Next(n + 1);
                        var value = questions[k];
                        questions[k] = questions[n];
                        questions[n] = value;
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

            var exam = new Exam { StudentId = Convert.ToInt32(Session["StudentId"]), Date = DateTime.Now };

            var examCategories = new List<ExamCategory>();

            foreach (var item in questionsIdList)
            {
                if (form[item.Key.ToString()] == null)
                    continue;

                var result = form[item.Key.ToString()];
                var dbAnswer = context.Questions.FirstOrDefault(x => x.Id == item.Key).Answer;

                var control = false;
                foreach (var examCategory in examCategories.Where(examCategory => examCategory.CategoryId == item.Value))
                {
                    control = true;
                    if (result == dbAnswer)
                    {
                        examCategory.TrueCounter += 1;
                        trueCounter += 1;
                    }
                    else
                    {
                        examCategory.FalseCounter += 1;
                        falseCounter += 1;
                    }
                }

                if (control) continue;

                var newExamCategory = new ExamCategory { CategoryId = item.Value, TrueCounter = 0, FalseCounter = 0 };

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

            exam.TrueCounter = trueCounter;
            exam.FalseCounter = falseCounter;
            exam.Point = trueCounter * 2;

            context.Exams.Add(exam);

            foreach (var examCategory in examCategories)
            {
                int examId = exam.Id;
                examCategory.ExamId = examId;
                context.ExamCategories.Add(examCategory);
            }

            var performanceCategories = new List<PerformanceCategory>();

            foreach (var examCategory in examCategories)
            {
                var isSameCategory = false;

                foreach (var performanceCategory in performanceCategories.Where(performanceCategory => examCategory.CategoryId == performanceCategory.CategoryId))
                {
                    performanceCategory.TrueCounter += examCategory.TrueCounter;
                    performanceCategory.FalseCounter += examCategory.FalseCounter;
                    isSameCategory = true;
                }

                if (isSameCategory) continue;

                var newPerformanceCategory = new PerformanceCategory
                {
                    CategoryId = examCategory.CategoryId,
                    TrueCounter = examCategory.TrueCounter,
                    FalseCounter = examCategory.FalseCounter,
                    StudentID = GetStundetId()
                };

                performanceCategories.Add(newPerformanceCategory);
            }

            foreach (var performanceCategory in performanceCategories)
            {
                var performCategories = context.PerformanceCategories.Where(x => x.StudentID == performanceCategory.StudentID && x.CategoryId == performanceCategory.CategoryId).ToList();

                if (performCategories.Count > 0)
                {
                    performCategories[0].TrueCounter += performanceCategory.TrueCounter;
                    performCategories[0].FalseCounter += performanceCategory.FalseCounter;
                }
                else
                {
                    context.PerformanceCategories.Add(performanceCategory);
                }
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

        private int[] GetQuestionNumber(int categoryNumber)
        {
            var avg = 50 / categoryNumber;
            var remain = 50 - categoryNumber * avg;
            var questionNumbers = new int[categoryNumber];

            for (var i = 0; i < categoryNumber; i++)
                questionNumbers[i] = avg;

            for (var i = 0; i < categoryNumber - 1; i++)
            {
                var additive = 0;
                for (var j = i + 1; j < categoryNumber; j++)
                {
                    questionNumbers[j] = questionNumbers[j] - 1;
                    additive++;
                }
                questionNumbers[i] += additive;
            }

            questionNumbers[0] += remain;

            var lastValue = questionNumbers[categoryNumber - 1];

            if (lastValue < 0)
            {
                var lastValueAbsolute = lastValue * -1;

                for (var i = 0; i < categoryNumber; i++)
                {
                    if (questionNumbers[i] != lastValueAbsolute) continue;

                    for (var j = i; j < categoryNumber; j++)
                    {
                        questionNumbers[j] = 0;
                    }
                    break;
                }
            }

            return questionNumbers;
        }

        private int[] GetCategoryIdsForExam(List<PerformanceCategory> performanceCategoryList)
        {
            var categoryIds = new int[performanceCategoryList.Count];
            var performance = new double[performanceCategoryList.Count];

            var i = 0;
            foreach (var category in performanceCategoryList)
            {
                var rate = 0.0;
                if (category.FalseCounter != 0 && category.TrueCounter != 0)
                    rate = category.TrueCounter / Convert.ToDouble(category.FalseCounter);
                else if (category.TrueCounter == 0 && category.FalseCounter == 0)
                    rate = 0;
                else if (category.FalseCounter == 0)
                    rate = category.TrueCounter + 1;
                else if (category.TrueCounter == 0)
                    rate = -1;

                performance[i] = rate;
                categoryIds[i] = category.CategoryId;
                i++;
            }

            var n = performance.Length;
            for (i = 0; i < n - 1; i++)
                for (var j = 0; j < n - i - 1; j++)
                    if (performance[j] > performance[j + 1])
                    {
                        var temp = performance[j];
                        performance[j] = performance[j + 1];
                        performance[j + 1] = temp;
                        var temp2 = categoryIds[j];
                        categoryIds[j] = categoryIds[j + 1];
                        categoryIds[j + 1] = temp2;

                    }

            return categoryIds;
        }
    }
}