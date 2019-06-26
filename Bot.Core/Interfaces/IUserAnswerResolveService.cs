using System;
using System.Collections.Generic;
using System.Text;
using Template.Core.Models;

namespace Template.Core.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUserAnswerResolveService
    {
        DecisionModel GetDecision(List<string> answers, QuestionModel questionModel);
    }
}
