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
            var Answers = answers.ToArray();
            var decision = new DecisionModel();
            for (var i = 0; i < questionModel.Decisions.Count; i++) 
            {
                decision = questionModel.Decisions[i];
                if (decision.Meta.ToString() == Answers.ToString())
                {
                    return decision;
                }
            }

            return null;
        }
    }
}
