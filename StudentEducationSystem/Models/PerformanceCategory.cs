using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StudentEducationSystem.Models
{
    public class PerformanceCategory
    {
        public int Id { get; set; }
        public int StudentID { get; set; }

        public int CategoryId { get; set; }

        public int TrueCounter { get; set; }

        public int FalseCounter { get; set; }

    }
}