using System;
using System.Collections.Generic;
using System.Text;
using StuddyBot.Core.DAL.Entities;

namespace StuddyBot.Core.Interfaces
{
    public interface ISubscriptionManager
    {
        ICollection<UserCourse> GetUserSubscriptions(string userId);

        void CancelSubscription(string userId, string courseId);

        Course GetCourseInfo(string dialogId);
    }
}