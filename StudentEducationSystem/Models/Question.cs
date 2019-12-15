using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StudentEducationSystem.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Title  { get; set; }
        public string Answer { get; set; }
        public string ChoiceOne { get; set; }
        public string ChoiceTwo { get; set; }
        public string ChoiceThree { get; set; }
        public string ChoiceFour { get; set; }
        public string Picture { get; set; }
        public int TeacherId { get; set; }

        public DateTime AddedTime { get; set; }
       
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        

    }
}