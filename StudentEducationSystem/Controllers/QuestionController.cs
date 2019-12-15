using StudentEducationSystem.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StudentEducationSystem.Controllers
{
    [Authorize(Roles = "T")]
    public class QuestionController : Controller
    {
        private EducationSystemContext context = new EducationSystemContext();

        // GET: Question

        public ActionResult Index()
        {
            int teacherID = GetTeacherID();
            List<Question> questions = context.Questions.Where(x => x.TeacherId == teacherID).ToList();
            return View(questions);
        }

        public ActionResult Create()
        {
            List<Category> categories = GetCategories();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection form)
        {
            string base64Str = "";
            try
            {
                
                if (Request != null)
                {
                    var image = Request.Files[0];
                    if (image.ContentLength > 0)
                    {
                        var picture = Image.FromStream(image.InputStream);
                        base64Str = ImageToBase64(picture);
                        if (base64Str.ToString().Length > 47999)
                        {
                            ToastrService.AddToUserQueue(new Toastr("Bir Hata Oluştu", "Düşük Kaliteli Fotoğraf Seçiniz", ToastrType.Error));
                            return RedirectToAction("Index");
                        }
                    }
                }
            }
            catch (Exception)
            {

                ToastrService.AddToUserQueue(new Toastr("Bir Hata Oluştu", "Resim seçtiğinize emin olunuz !", ToastrType.Error));
                return View();
            }



            string title = form["title"];
            string file = base64Str;
            string answer = form["answer"];
            string categoryId = form["Categories"];
            string choiceOne = form["choiceOne"];
            string choiceTwo = form["choiceTwo"];
            string choiceThree = form["choiceThree"];
            string choiceFour = form["choiceFour"];
            int teacherId = GetTeacherID();



            Question question = new Question()
            {
                Title = title,
                Picture = file,
                Answer = answer,
                CategoryId = Convert.ToInt32(categoryId),
                AddedTime = DateTime.Now,
                ChoiceOne = choiceOne,
                ChoiceTwo = choiceTwo,
                ChoiceThree = choiceThree,
                ChoiceFour = choiceFour,
                TeacherId = teacherId
            };
            context.Questions.Add(question);
            context.SaveChanges();

            ToastrService.AddToUserQueue(new Toastr("Soru Eklendi", null, ToastrType.Success));
            return RedirectToAction("Index");
        }

        private List<Category> GetCategories()
        {
            
            return context.Categories.ToList();
        }
        private string ImageToBase64(Image img)
        {
            using (MemoryStream m = new MemoryStream())
            {
                img.Save(m, img.RawFormat);
                byte[] imageBytes = m.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        private int GetTeacherID()
        {
            return Convert.ToInt32(Session["TeacherId"]);
        }
    }
}