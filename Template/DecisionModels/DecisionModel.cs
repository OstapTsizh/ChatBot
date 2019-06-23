using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Template.DecisionModels
{
    public class DecisionModel
    {
        // List<string> Topics ???
        
        /// <summary>
        /// A topic question is related to.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// A question to ask user.
        /// </summary>
        public string Question { get; set; }

        /// <summary>
        /// Is this question from few sub-questions
        /// to make next decision?
        /// </summary>
        public bool IsWaterfallQuestion { get; set; }

        /// <summary>
        /// Is this question final or we have next question in the tree?
        /// </summary>
        public bool HasNextQuestion { get; set; }

        /// <summary>
        /// A list of possible choices (answers) in
        /// the body of a question for a user.
        /// </summary>
        public List<string> Options { get; set; }
    }
}
