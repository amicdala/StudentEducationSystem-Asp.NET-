using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace StudentEducationSystem.Models
{
    public class EducationSystemContext : DbContext
    {
        public EducationSystemContext() : base("Db")
        {
            Database.SetInitializer(new EducationSystemInitializer());
        }
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Question> Questions { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamCategory> ExamCategories { get; set; }


    }
}   