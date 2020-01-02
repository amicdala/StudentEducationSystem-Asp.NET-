using StudentEducationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace StudentEducationSystem.Controllers
{
    public class ExamController : Controller
    {
        private readonly EducationSystemContext _context = new EducationSystemContext();
        private static Dictionary<int, int> _questionsIdList;

        // GET: Exam
        public ActionResult TakeTheExam()
        {
            int studentId = GetStundetId();
            List<Exam> exams = GetExamsWithStudentId(studentId);

            if (exams.Count > 0)
            {
                DateTime lastDate = exams.OrderByDescending(x => x.Id).ToList()[0].Date;

                if (!IsToday(lastDate))
                {
                    int teacherIdForStudent = GetTeacherIDForStudent();

                    int totalCategory = _context.Categories.Count(x => x.TeacherId == teacherIdForStudent);

                    List<PerformanceCategory> performance = GetPerformanceCategoriesForStudent(studentId);
                    
                    int[] categoryIds = GetAscendingRateCategoryIdsForExam(performance);

                    int[] countOfQuestion = GetDescendingCountOfQuestions(totalCategory);

                    List<Question> questions =
                        GetFiftyQuestionsWithTeacherIdAndCategoryId(teacherIdForStudent, categoryIds, countOfQuestion);

                    MatchQuestionsToCategoryId(questions, categoryIds, countOfQuestion, false);

                    ShuffleList(questions);

                    return View(questions);
                }
            }
            else
            {
                int teacherIdForStudent = GetTeacherIDForStudent();

                List<Question> questions = GetFiftyRandomQuestionWithTeacherId(teacherIdForStudent);

                MatchQuestionsToCategoryId(questions, null, null, true);

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

            Exam exam = new Exam { StudentId = Convert.ToInt32(Session["StudentId"]), Date = DateTime.Now };

            List<ExamCategory> examCategories = new List<ExamCategory>();

            foreach (var item in _questionsIdList)
            {
                if (form[item.Key.ToString()] == null)
                    continue;

                string result = form[item.Key.ToString()];
                string dbAnswer = GetRightAnswer(item.Key);

                bool isNewCategory = true;
                foreach (ExamCategory examCategory in examCategories.Where(examCategory => examCategory.CategoryId == item.Value))
                {
                    isNewCategory = false;
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

                if (!isNewCategory) continue;

                ExamCategory newExamCategory = new ExamCategory { CategoryId = item.Value, TrueCounter = 0, FalseCounter = 0 };

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

            _context.Exams.Add(exam);
            _context.SaveChanges();

            foreach (var examCategory in examCategories)
            {
                int examId = exam.Id;         
                examCategory.ExamId = examId;
                _context.ExamCategories.Add(examCategory);
            }

            List<PerformanceCategory> performanceCategories = new List<PerformanceCategory>();

            foreach (var examCategory in examCategories)
            {
                bool isExist = IsCategoryInList(performanceCategories, examCategory);

                if (isExist) continue;

                PerformanceCategory newPerformanceCategory = new PerformanceCategory
                {
                    CategoryId = examCategory.CategoryId,
                    TrueCounter = examCategory.TrueCounter,
                    FalseCounter = examCategory.FalseCounter,
                    StudentID = GetStundetId()
                };

                performanceCategories.Add(newPerformanceCategory);
            }

            foreach (PerformanceCategory performanceCategory in performanceCategories)
            {

                PerformanceCategory performCategory =
                    GetPerformanceWithCategoryId(GetStundetId(), performanceCategory.CategoryId);

                if (performCategory != null)
                {
                    performCategory.TrueCounter += performanceCategory.TrueCounter;
                    performCategory.FalseCounter += performanceCategory.FalseCounter;
                }
                else
                {
                    AddPerformanceCategory(performanceCategory);
                }
            }

            _context.SaveChanges();

            List<Category> categories = GetTeacherCategories(GetTeacherIDForStudent());

            List<PerformanceCategory> existPerformanceCategories = GetPerformanceCategoriesForStudent(GetStundetId());

            foreach (var category in categories)
            {
                bool isExist = false;
                foreach (var performanceCategory in existPerformanceCategories)
                {
                    if (category.Id == performanceCategory.CategoryId)
                    {
                        isExist = true;
                        break;
                    }
                }

                if (isExist) continue;

                var newPerformanceCategory = new PerformanceCategory
                {
                    CategoryId = category.Id,
                    TrueCounter = 0,
                    FalseCounter = 0,
                    StudentID = GetStundetId()
                };

                AddPerformanceCategory(newPerformanceCategory);
            }
            
            _context.SaveChanges();
            ToastrService.AddToUserQueue("", "Sınavınız tamamlandı!", ToastrType.Info);
            return RedirectToAction("Index", "Student");
        }

        private List<Category> GetTeacherCategories(int teacherId)
        {
            return _context.Categories.Where(x => x.TeacherId == teacherId).ToList();
        }

        private void AddPerformanceCategory(PerformanceCategory performCategory)
        {
            _context.PerformanceCategories.Add(performCategory);
        }

        private PerformanceCategory GetPerformanceWithCategoryId(int studentId, int categoryId)  // Parametre olarak gelen studentId'ye ve categoryId'ye sahip performance category i dondurur.
        {
            return _context.PerformanceCategories.FirstOrDefault(x => x.StudentID == studentId && x.CategoryId == categoryId);
        }

        private bool IsCategoryInList(List<PerformanceCategory> performanceCategories, ExamCategory examCategory)  // Parametre olarak gelen examCategory, performanceCategories listesinde var mı yok mu kontrol eder. Eger varsa bu examCategory nin dogru ve yanlislari, performanceCategorieste var olanin ustune yazdirilir.
        {
            bool isSameCategory = false;
            foreach (var performanceCategory in performanceCategories.Where(performanceCategory => examCategory.CategoryId == performanceCategory.CategoryId))
            {
                performanceCategory.TrueCounter += examCategory.TrueCounter;
                performanceCategory.FalseCounter += examCategory.FalseCounter;
                isSameCategory = true;
            }

            return isSameCategory;
        }

        private int GetStundetId()
        {
            return Convert.ToInt32(Session["StudentId"]);
        }
        private int GetTeacherIDForStudent()
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);
            int teacherID = _context.Students.FirstOrDefault(x => x.Id == studentId).TeacherId;
            return teacherID;
        }
        private bool IsToday(DateTime date)
        {
            return date.Day == DateTime.Now.Day && date.Month == DateTime.Now.Month && date.Year == DateTime.Now.Year;
        }

        private List<Exam> GetExamsWithStudentId(int studentId)
        {
            return _context.Exams.Where(x => x.StudentId == studentId).ToList();
        }
        private List<PerformanceCategory> GetPerformanceCategoriesForStudent(int studentId)  // Parametre olarak gelen id'ye sahip ogrencinin performance category tablosundaki kayitlarini dondurur.
        {
            return _context.PerformanceCategories.Where(x => x.StudentID == studentId).ToList();
        }

        private int[] GetAscendingRateCategoryIdsForExam(List<PerformanceCategory> performanceCategoryList) // Parametre olarak gelen performans kategori listesine gore, kategori id'leri basarimi en dusukten en yuksege seklinde sirali dizi olarak dondurur.
        {
            var categoryIds = new int[performanceCategoryList.Count];
            double[] performance = new double[performanceCategoryList.Count];

            var i = 0;
            foreach (var category in performanceCategoryList)
            {
                double rate = 0.0;
                if (category.FalseCounter != 0 && category.TrueCounter != 0)
                    rate = category.TrueCounter / Convert.ToDouble(category.FalseCounter);
                else if (category.TrueCounter == 0 && category.FalseCounter == 0)
                    rate = 0;
                else if (category.FalseCounter == 0)
                    rate = category.TrueCounter + 1;
                else if (category.TrueCounter == 0)
                    rate = category.FalseCounter * -1;

                performance[i] = rate;
                categoryIds[i] = category.CategoryId;
                i++;
            }

            SortArrayByAscendingRate(performance, categoryIds);

            return categoryIds;
        }

        private void SortArrayByAscendingRate(double[] performance, int[] categoryIds)  // Parametre olarak gelen performance dizisini kucukten buyuge dogru siralar. Performance dizisinde yer degisikligi olduğu zaman ayni yer degisimini categoryIds dizisinde de gerceklestirir. Boylelikle categoryId ile performance orani ayni indislerde tutulmus olur.
        {
            var n = performance.Length;
            for (var i = 0; i < n - 1; i++)
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
        }

        private int[] GetDescendingCountOfQuestions(int categoryNumber) // Parametre olarak gelen kategori sayisina gore, 50 soruyu asimetrik sekilde parcalar. Bu metodun amaci basarima gore sorulacak soru sayisini belirlemektir. 
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

        private List<Question> GetFiftyQuestionsWithTeacherIdAndCategoryId(int teacherIdForStudent, int[] categoryIds, int[] countOfQuestion)  // Parametre olarak gelen teacherId'ye sahip ogretmenin eklemis oldugu ve categoryIds dizisindeki kategoriId'lere sahip, countOfQuestion dizisindeki sayilar kadar soru getirir. teacherId = 1, categoryIds[0] = 5, countOfQuestion[0] = 9 dersek teacherId = 1 ve kategoriId'si 5 olan sorulardan 9 tane getirir.
        {
            List<Question> questions = new List<Question>();
            for (int i = 0; i < categoryIds.Length; i++)
            {
                int categoryId = categoryIds[i];
                int count = countOfQuestion[i];

                questions.AddRange(_context.Questions
                    .Where(x => x.TeacherId == teacherIdForStudent && x.CategoryId == categoryId)
                    .Take(count).ToList());
            }

            return questions;
        }

        private void MatchQuestionsToCategoryId(List<Question> questions, int[] categoryIds, int[] countOfQuestion, bool isFirstExam)  // Parametre olarak gelen sorulari key => soru, value => sorunun kategori id'si olacak sekilde _questionsIdList adli listeye ekler.
        {
            _questionsIdList = new Dictionary<int, int>();

            if (!isFirstExam)
            {
                int countOfQue = 0;
                for (int i = 0; i < categoryIds.Length; i++)
                {
                    for (int j = countOfQue; j < countOfQue + countOfQuestion[i]; j++)
                    {
                        _questionsIdList.Add(questions[j].Id, categoryIds[i]);
                    }

                    countOfQue += countOfQuestion[i];
                }
            }
            else
            {
                foreach (var item in questions)
                {
                    int categoryId = _context.Questions.FirstOrDefault(x => x.Id == item.Id).CategoryId;
                    _questionsIdList.Add(item.Id, categoryId);
                }
            }
        }
        private void ShuffleList<T>(List<T> list)
        {
            Random rand = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private List<Question> GetFiftyRandomQuestionWithTeacherId(int teacherIdForStudent)
        {
            Random rand = new Random();
            List<Question> questions = new List<Question>();
            for (int i = 0; i < 50; i++)
            {
                var skip = (int)(rand.NextDouble() * _context.Questions.Count());

                Question question = _context.Questions.OrderBy(x => x.TeacherId == teacherIdForStudent).Skip(skip).FirstOrDefault();

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

            return questions;
        }

        private string GetRightAnswer(int questionId)
        {
            return _context.Questions.FirstOrDefault(x => x.Id == questionId).Answer;
        }

    }
}