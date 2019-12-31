using System;
using Xunit;
using StudentEducationSystem;
using StudentEducationSystem.Models;
using StudentEducationSystem.Test;
using StudentEducationSystem.Controllers;
using System.Linq;

namespace StudentEducationSystemTest
{
    public class UnitTest
    {
        private EducationSystemContext context = new EducationSystemContext();
        Test xunitTest;
        [Fact]
        public void CategoryTest()
        {
            xunitTest = new Test();
            string name = "Coðrafya";
            int id = xunitTest.AddCategory(name);
            string dbName = context.Categories.FirstOrDefault(x => x.Id == id).Name;

            Assert.Equal(name,dbName);

        }
        [Fact]
        public void QuestionTest()
        {
            xunitTest = new Test();
            string title = "Test Sorusu";
            string answer = "Test Cevabý";
            int categoryId = 1;
            string choiceOne = "Test Cevap 1";
            string choiceTwo = "Test Cevap 1";
            string choiceThree = "Test Cevap 1";
            string choiceFour = "Test Cevap 1";
            int teacherId = 1;

            int id = xunitTest.AddQuestion(title, answer, categoryId, choiceOne, choiceTwo, choiceThree, choiceFour, teacherId);

            int dbID = context.Questions.FirstOrDefault(x => x.Title == title && x.Answer == answer && x.ChoiceOne == x.ChoiceOne).Id;

            Assert.Equal(id, dbID);

        }
        [Fact]
        public void LoginTest()
        {
            xunitTest = new Test();
            string username = "oguzhankaymak";
            string password = "12345";
            string role = "T";
            bool control = xunitTest.Login(username, password, role);
            Assert.True(control);
        }

    }
}
