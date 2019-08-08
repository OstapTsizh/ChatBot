using System;
using System.Collections.Generic;
using System.Text;

namespace StuddyBot.Core.Models
{
    /// <summary>
    /// Represents all the course info.
    /// </summary>
    public class Course
    {
        /// <summary>
        /// A name of a course.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Resources of the course.
        /// </summary>
        public string Resources { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime RegistrationStartDate { get; set; }



    }
}
