using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StudentEducationSystem.Models
{
    public class Student 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }
        public int UserId { get; set; }
    }
}