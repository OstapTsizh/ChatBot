using System.Collections.Generic;
using System.Linq;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

namespace StuddyBot.Core.Services
{

    public class UserAnswerResolveService : IUserAnswerResolveService
    {
        public DecisionModel GetDecision(List<string> answers, QuestionModel questionModel)
        {
            var metaStr = new List<string>();
            
            for(var i = 0; i < questionModel.Decisions.Count(); i++)
            {
                for(var j = 0; j < questionModel.Decisions[i].Meta.Count(); j++)
                {
                    metaStr.Add(questionModel.Decisions[i].Meta[j]);
                }
                if(metaStr.Count == answers.Count && !metaStr.Except(answers).Any())
                {
                    return questionModel.Decisions[i];
                }

                metaStr.Clear();
            }

            return null;
        }
    }
}
