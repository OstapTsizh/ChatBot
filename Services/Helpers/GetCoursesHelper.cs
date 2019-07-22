using Services.Helpers.Interfaces;
using StuddyBot.Core.DAL.Data;
using StuddyBot.Core.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Helpers
{
    public class GetCoursesHelper : IGetCoursesHelper
    {
        private StuddyBotContext _db;

        public GetCoursesHelper(StuddyBotContext dbContext)
        {
            _db = dbContext;
        }

        public async Task<IQueryable<Course>> GetCourses(DateTime startDate)
        {
            return _db.Courses.Where(s => s.RegistrationStartDate.Day == startDate.Day);
        }



    }
}
