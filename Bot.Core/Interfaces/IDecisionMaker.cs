using System;

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
    }
}
