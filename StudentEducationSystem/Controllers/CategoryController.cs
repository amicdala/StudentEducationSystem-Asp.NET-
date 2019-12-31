using StudentEducationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StudentEducationSystem.Controllers
{
    [Authorize(Roles = "T")]
    public class CategoryController : Controller
    {
        EducationSystemContext context = new EducationSystemContext();
        // GET: Category
        public ActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection form)
        {
            string name = form["name"];
            int teacherId = GetTeacherID();
            Category category = new Category() { Name = name, TeacherId = teacherId };
            context.Categories.Add(category);
            context.SaveChanges();
            ToastrService.AddToUserQueue(new Toastr("Kategori Eklendi", null, ToastrType.Success));
            return RedirectToAction("Create", "Question");
        }

        private int GetTeacherID()
        {
            return Convert.ToInt32(Session["TeacherId"]);
        }
    }
}