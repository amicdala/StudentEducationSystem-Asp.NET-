using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace StudentEducationSystem.Models
{
    public class EducationSystemInitializer:DropCreateDatabaseIfModelChanges<EducationSystemContext>
    {
        protected override void Seed(EducationSystemContext context)
        {
            List<User> users = new List<User>()
            {
                new User(){Name = "Oğuzhan",Surname = "Kaymak",Username = "oguzhankaymak",Password="12345",Role = "T"},
                new User(){Name = "Hüseyin",Surname = "Kara",Username = "huseyin",Password="12345",Role = "S"}

            };
            foreach (var item in users)
            {
                context.Users.Add(item);
            }
            context.SaveChanges();

            List<Teacher> teachers = new List<Teacher>()
            {
                new Teacher() {DepartmentName="Matematik" ,UserId = 1}
            };


            foreach (var item in teachers)
            {
                context.Teachers.Add(item);
            }
            context.SaveChanges();

            List<Student> students = new List<Student>()
            {
                new Student(){UserId=2,TeacherId = 2}
            };

            foreach (var item in students)
            {
                context.Students.Add(item);
            }
            context.SaveChanges();


            
            List<Category> categories = new List<Category>()
            {
                new Category(){Name = "Üçgenler"},
                new Category(){Name = "Açılar"},
                new Category(){Name = "Dörtgenler"},
                new Category(){Name = "Türev"}
            };
            foreach (var item in categories)
            {
                context.Categories.Add(item);
            }
            context.SaveChanges();

            List<Question> questions = new List<Question>()
            {
                new Question(){Title="Aşağıdakilerden hangisi 3 kenarlıdır ? ",Answer = "üçgen",ChoiceOne = "Dikdörtgen",ChoiceTwo = "Kare",ChoiceThree = "Yamuk",AddedTime = DateTime.Now,CategoryId = 1,Picture = null,TeacherId = 1}
            };
            foreach (var item in questions)
            {
                context.Questions.Add(item);
            };
            context.SaveChanges();

            base.Seed(context);
        }
    }
}