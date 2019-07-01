using System;
using System.Collections.Generic;
using System.Text;
using StuddyBot.Core.Models;

namespace StuddyBot.Core.Interfaces
{
    /// <summary>
    /// Interface for getting proper bot responce accoding to user's answers.
    /// </summary>
    public interface IUserAnswerResolveService
    {
        /// <summary>
        /// Finds right decision accoding to user's answers.
        /// </summary>
        /// <param name="answers"> Stored user's answers</param>
        /// <param name="questionModel">Topic Model</param>
        /// <returns>DesicionModel object of <see cref="DecisionModel"/> class</returns>
        DecisionModel GetDecision(List<string> answers, QuestionModel questionModel);
    }
}
