using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StudentEducationSystem.Models;

namespace StudentEducationSystem.Test
{
   
    public class Test
    {
        private EducationSystemContext context = new EducationSystemContext();

        public Test()
        {

        }
        public int AddCategory(string name)
        {
            Category category = new Category()
            {
                Name = name,
                TeacherId = 1
            };
            context.Categories.Add(category);
            context.SaveChanges();
            int id = context.Categories.FirstOrDefault(x => x.Name == name).Id;
            return id;
        }

        public int AddQuestion(string title,string answer,int categoryId,string choiceOne,string choiceTwo, string choiceThree, string choiceFour, int teacherId)
        {
            Question question = new Question()
            {
                Title = title,
                Picture = null,
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

            int id = context.Questions.FirstOrDefault(x => x.Title == title && x.Answer == answer).Id;
            return id;
                        
        }

        public bool Login(string username,string password,string role)
        {
            var user = context.Users.FirstOrDefault(x => x.Username == username && x.Password == password && x.Role == role);
            if (user !=null)
            {
                return true;
            }
            return false;
        }
    }
}