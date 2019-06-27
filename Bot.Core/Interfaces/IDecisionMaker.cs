using System.Collections.Generic;
using StuddyBot.Core.Models;

namespace StuddyBot.Core.Interfaces
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
        /// <param name="name">A topic to look for question(s)/answer(s),
        /// decision(s).</param>
        /// <returns>Model with name, keywords, questions, decisions" </returns>
        QuestionModel GetQuestionOrResult(string name);

        /// <summary>
        /// Finds right decision accoding to user's answers
        /// </summary>
        /// <param name="answers"> Stored user's answers</param>
        /// <param name="questionModel">Topic Model</param>
        /// <returns>DesicionModel object of <see cref="DecisionModel"/> class</returns>
        DecisionModel GetDecision(List<string> answers, QuestionModel questionModel);


        List<string> GetStartTopics();
    }

    
}
