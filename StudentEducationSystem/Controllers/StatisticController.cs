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
        public ActionResult Index(FormCollection form)
        {
            ViewData["ChartOfChange"] = ChartOfChange();
            ViewData["ChartOfSuccess"] = ChartOfSuccess("21-12-2019");
            return View();
        }

        private Chart ChartOfChange()
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);
            List<Exam> exams = GetLastThreeExamByStudentId(studentId);
            exams = exams.OrderBy(x => x.Date).ToList();

            if (exams.Count > 0)
            {
                return new Chart(width: 925, height: 725)
                    .AddTitle("Son " + exams.Count + " Sınavınız")
                    .AddSeries(
                        chartType: "column",
                        xValue: exams.Select(x => x.Date.ToString("dd-MM-yyyy")).ToArray(), xField: "Tarih",
                        yValues: exams.Select(x => x.Point).ToArray(), yFields: "Puan")
                    .Write("png");
            }

            return new Chart(width: 925, height: 100)
                .AddTitle("Sınavınız bulunmamaktadır!")
                .Write("png");
        }

        private Chart ChartOfSuccess(string date = "23-12-2019")
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);
            DateTime dateTime = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            Exam exam = _context.Exams.FirstOrDefault(x =>
                x.StudentId == studentId && DbFunctions.TruncateTime(x.Date) == dateTime);

            if (exam != null)
            {
                List<ExamCategory> examCategories = _context.ExamCategories.Where(x => x.ExamId == exam.Id).ToList();

                List<string> categoryNames = new List<string>();
                double[] rates = new double[examCategories.Count];
                int i = 0;
                foreach (var examCategory in examCategories)
                {
                    categoryNames.Add(_context.Categories.Where(x => x.Id == examCategory.CategoryId).Select(x => x.Name).FirstOrDefault());
                    int trueCounter = examCategory.TrueCounter;
                    int falseCounter = examCategory.FalseCounter;
                    if (falseCounter != 0 && trueCounter != 0)
                        rates[i] = trueCounter / Convert.ToDouble(falseCounter);
                    else if (trueCounter == 0 && falseCounter == 0)
                        rates[i] = 0;
                    else if (falseCounter == 0)
                        rates[i] = trueCounter + 1;
                    else if (trueCounter == 0)
                        rates[i] = falseCounter * -1;

                    i++;
                }

                return new Chart(width: 925, height: 725)
                    .AddTitle(date + " tarihli sınavınıza ait istatistikler")
                    .AddSeries(
                        chartType: "pie",
                        xValue: categoryNames.ToArray(),
                        yValues: rates)
                    .Write("png");
            }

            return new Chart(width: 925, height: 100)
                .AddTitle("Seçtiğiniz tarihte kayıtlı sınavınız yoktur!")
                .Write("png");

        }

        private List<Exam> GetLastThreeExamByStudentId(int studentId)
        {
            return _context.Exams.Where(x => x.StudentId == studentId).OrderByDescending(x => x.Date).Take(3).ToList();
        }
    }
}