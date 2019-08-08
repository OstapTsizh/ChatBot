using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StuddyBot.Core.BLL.Interfaces;
using StuddyBot.Core.DAL.Data;
using StuddyBot.Core.DAL.Entities;
using StuddyBot.Core.Interfaces;

namespace StuddyBot.Core.BLL.Helpers
{
    public class SubscriptionManager : ISubscriptionManager
    {
        private readonly StuddyBotContext _db;
        
        public SubscriptionManager(StuddyBotContext db)
        {
            _db = db;
        }

        public ICollection<UserCourse> GetUserSubscriptions(string userId)
        {
            var userCourses = _db.UserCourses.Where(item => item.UserId == userId);
            if (userCourses.Count() != 0)
            {
                return userCourses.ToList();
            }
            return null;
        }

        public void CancelSubscription(string userId, string courseId)
        {
            var userCourses = _db.UserCourses.Where(item => item.UserId == userId);
            if (!userCourses.Any()) return;
            {
                var cancelCourse = userCourses.First(item => item.CourseId.ToString() == courseId);
                if (cancelCourse != null)
                {
                    _db.UserCourses.Remove(cancelCourse);
                    _db.SaveChanges();
                }
            }
        }

        public Course GetCourseInfo(string courseId)
        {
            return _db.Courses.First(course => course.Id.ToString() == courseId);
        }
    }
}