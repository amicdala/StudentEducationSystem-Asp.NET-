using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StudentEducationSystem.Models
{
    public class ExamCategory
    {
        public int Id { get; set; }
        public int TrueCounter { get; set; }
        public int FalseCounter { get; set; }
        public int CategoryId { get; set; }
        public int ExamId { get; set; }
    }
}