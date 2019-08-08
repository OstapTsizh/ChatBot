using System;
using System.Collections.Generic;
using System.Text;

namespace StuddyBot.Core.Models
{
    /// <summary>
    /// A model of question to ask user in the dialog.
    /// </summary>
    public class QuestionModel
    {
        /// <summary>
        /// The name of a course.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The keywords related to the course.
        /// </summary>
        public string[] Keywords { get; set; }

        /// <summary>
        /// The list of questions related to the course to ask a user.
        /// </summary>
        public List<Question> Questions { get; set; }        

        /// <summary>
        /// The list of possible decisions related to the course
        /// to choose one basing on the users answers.
        /// </summary>
        public List<DecisionModel> Decisions { get; set; }

    }
}
