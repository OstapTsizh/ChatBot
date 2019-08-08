using Microsoft.Bot.Builder.Dialogs;
using Microsoft.EntityFrameworkCore;
using StuddyBot.Core.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Internal;
using StuddyBot.Core.Models;
using Course = StuddyBot.Core.DAL.Entities.Course;
using Question = StuddyBot.Core.DAL.Entities.Question;

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

        /// <summary>
        /// A model to save users Questions.
        /// </summary>
        public DbSet<Question> Questions { get; set; }

        /// <summary>
        /// A model to save users Feedback.
        /// </summary>
        public DbSet<Feedback> Feedback { get; set; }


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

        public void AddSubscriptionToCourse(string userId, string courseName)
        {
            var onCourse = Courses.First(course => course.Name == courseName);

            var userSubscriptions = UserCourses.Any(item => item.UserId == userId && item.CourseId == onCourse.Id);
            if (!userSubscriptions)
            { 
                UserCourses.Add(new UserCourse()
                {
                    UserId = userId,
                    CourseId = onCourse.Id,
                    Notified = false
                });
            }
        }

        public void AddQuestion(string question)
        {
            Questions.Add(new Question()
            {
                Message = question,
                Date = DateTime.Now
            });
        }

        public void AddFeedback(string feedback)
        {
            Feedback.Add(new Feedback()
            {
                Message = feedback,
                Date = DateTime.Now
            });
        }
    }
}

