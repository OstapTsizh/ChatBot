using StuddyBot.Core.BLL.Interfaces;
using StuddyBot.Core.DAL.Data;
using StuddyBot.Core.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StuddyBot.Core.BLL.Helpers
{
    public static class CourseExtension
    {
        
        public static async Task NotificateUsers(this Course course, StuddyBotContext db)
        {
            var query = db.User
                .Where(s => s.UserCourses
                                .Any(e => e.Course.Id == course.Id));



        }


    }
}
