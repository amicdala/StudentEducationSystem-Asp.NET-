using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StudentEducationSystem.Models
{
    public class Exam
    {
        public int Id { get; set; }
        public int Point { get; set; }
        public int TrueCounter { get; set; }
        public int FalseCounter { get; set; }
        public DateTime Date { get; set; }
        public int StudentId { get; set; }
    }
}