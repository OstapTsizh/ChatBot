using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Template.Core.Interfaces;
using Template.Core.Models;

namespace Template.Core.Services
{

    public class UserAnswerResolveService : IUserAnswerResolveService
    {
        public DecisionModel GetDecision(List<string> answers, QuestionModel questionModel)
        {
            var decision = questionModel.Decisions.Where(d => d.Meta.Equals(answers.ToArray<string>())).First();
            
            return decision;
        }
    }
}
