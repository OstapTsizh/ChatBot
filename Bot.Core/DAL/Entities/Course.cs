using System;
using System.Collections.Generic;
using System.Text;

namespace StuddyBot.Core.DAL.Entities
{
    public class Course
    {
        public int Id { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime RegistrationStartDate { get; set; }

        /// <summary>
        /// Users that are registered to a course
        /// </summary>
        public ICollection<UserCourse> UserCourses { get; set; }
        
    }
}
