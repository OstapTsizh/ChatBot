using Microsoft.Bot.Builder.Dialogs;
using Microsoft.EntityFrameworkCore;
using StuddyBot.Core.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StuddyBot.Core.Models;
using Course = StuddyBot.Core.DAL.Entities.Course;

namespace StuddyBot.Core.DAL.Data
{
    public class StuddyBotContext : DbContext
    {
        public StuddyBotContext() : base() { }

        public StuddyBotContext(DbContextOptions<StuddyBotContext> options)
            : base(options)
        { }

        /// <summary>
        /// A model to save a Dialog(Messages).
        /// </summary>
        public DbSet<MyDialog> Dialog { get; set; }

        /// <summary>
        /// A model to save Dialogs.
        /// </summary>
        public DbSet<Dialogs> Dialogs { get; set; }

        /// <summary>
        /// A model to save Users.
        /// </summary>
        public DbSet<User> User { get; set; }

        /// <summary>
        /// A model to save Courses.
        /// </summary>
        public DbSet<Course> Courses { get; set; }

        /// <summary>
        /// A model to save Courses that a user is
        /// subscribed to.
        /// </summary>
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



        public void PushCoursesToDB(List<Models.Course> courses)
        {
            foreach (var course in courses)
            {
                var Course = new Course()
                {
                    Name = course.Name,
                    RegistrationStartDate = DateTime.Now,
                    StartDate = DateTime.Now
                };
                Courses.Add(Course);
            }
        }

        public string GetUserEmail(DialogInfo dialogInfo)
        {
            return User.First(user => user.Id == dialogInfo.UserId).Email;
        }

        public void DeleteUserEmail(DialogInfo dialogInfo)
        {
            User.First(user => user.Id == dialogInfo.UserId).Email = string.Empty;
        }

        public void EditUserEmail(DialogInfo dialogInfo, string newEmail)
        {
            User.First(user => user.Id == dialogInfo.UserId).Email = newEmail;
        }

        public string GetUserConversation(int dialogId)
        {
            var text = "Hi, your conversation with StuddyBot";
            var conversation = Dialog.Where(d => d.DialogsId == dialogId).ToList();
            foreach (var message in conversation)
            {
                text += $"<br/>From: {message.Sender} <br/> &nbsp; {message.Message} &emsp; {message.Time.DateTime}<br/><hr/>";
            }

            return text;
        }
    }
}

