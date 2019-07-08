using Microsoft.Bot.Builder.Dialogs;
using Microsoft.EntityFrameworkCore;
using StuddyBot.Core.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StuddyBot.Core.DAL.Data
{
    public class StuddyBotContext : DbContext
    {
        public StuddyBotContext() : base() { }

        public StuddyBotContext(DbContextOptions<StuddyBotContext> options)
            : base(options)
        { }


        public DbSet<MyDialog> Dialog { get; set; }
        public DbSet<Dialogs> Dialogs { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<UserCourse> UserCourses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=StuddyBotDB;Integrated Security=True;");           
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<UserCourse>()
                .HasKey(uc => new { uc.UserId, uc.CourseId });

            modelBuilder.Entity<UserCourse>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserCourses)
                .HasForeignKey(uc => uc.UserId);

            modelBuilder.Entity<UserCourse>()
                .HasOne(uc => uc.Course)
                .WithMany(c => c.UserCourses)
                .HasForeignKey(uc => uc.CourseId);


            base.OnModelCreating(modelBuilder);

        }

    }
}

