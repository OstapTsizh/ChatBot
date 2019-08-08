using StuddyBot.Core.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Helpers.Interfaces
{
    public interface IGetCoursesHelper
    {
        Task<IQueryable<Course>> GetCourses(DateTime startDate);

    }
}
