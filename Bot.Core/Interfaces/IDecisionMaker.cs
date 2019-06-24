using System;
using System.Collections;
using System.Collections.Generic;

namespace Template.Core.Interfaces
{
    /// <summary>
    /// An interface to implement in a class that will be
    /// getting a question/answer; making decision based on keywords
    /// from the user.
    /// </summary>
    public interface IDecisionMaker
    {
        /// <summary>
        /// Return a string with topic(s), question(s)/answer(s), next keywords.
        /// </summary>
        /// <param name="topic">A topic to look for question(s)/answer(s),
        /// decision(s).</param>
        /// <returns>A dictionary with Topic, Keywords,
        /// question(s)/answer(s), decision(s).</returns>
        string GetQuestionOrResult(string topic);
        List<DecisionModel> LoadQAList(); 
    }

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
