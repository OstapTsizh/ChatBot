using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StuddyBot.Core.BLL.Interfaces;
using StuddyBot.Core.DAL.Entities;
using StuddyBot.Core.Interfaces;

namespace StuddyBot.Core.BLL.Helpers
{
    public class SubscriptionManager : ISubscriptionManager
    {
        private readonly IUnitOfWork _unitOfWork;

        private List<UserCourse> testResult = new List<UserCourse>()
        {
            new UserCourse()
            {
                CourseId = 1,
                Course = new Course()
                    {Id = 1, StartDate = DateTime.Today, RegistrationStartDate = DateTime.Today.AddDays(5)}
            },
            new UserCourse()
            {
                CourseId = 2,
                Course = new Course()
                    {Id = 2, StartDate = DateTime.Today.AddDays(-5), RegistrationStartDate = DateTime.Today.AddDays(10)}
            },
        };

        public SubscriptionManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ICollection<UserCourse> GetUserSubscriptions(string userId)
        {
            var user = _unitOfWork.Users.Get(userId);

            var result = user?.UserCourses;

            //only for test
            /*testResult = new List<UserCourse>()
            {
                new UserCourse()
                {
                    CourseId = 1,
                    Course = new Course()
                        {Id = 1, StartDate = DateTime.Today, RegistrationStartDate = DateTime.Today.AddDays(5)}
                },
                new UserCourse()
                {
                    CourseId = 2,
                    Course = new Course()
                        {Id = 2, StartDate = DateTime.Today.AddDays(-5), RegistrationStartDate = DateTime.Today.AddDays(10)}
                },
            };*/

            //test
            result = testResult;

            return result;
        }

        public void CancelSubscription(string userId, string courseId)
        {
            /*var user = _unitOfWork.Users.Get(userId);
            var cancelCourse = user.UserCourses.FirstOrDefault(item => item.CourseId.ToString() == courseId);
            if (cancelCourse != null)
            {
                user.UserCourses.Remove(cancelCourse);
            }*/

            //test
            var courseToDelete = testResult.First(s => s.CourseId.ToString() == courseId);
            testResult.Remove(courseToDelete);
        }
    }
}