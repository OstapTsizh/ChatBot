using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Template.Core.Models;

namespace Template.Core.Services
{

    public class UserAnswerResolveService
    {
        private readonly UserAnswersModel userAnswers;
        private readonly QuestionModel questionModel;
        // UNDONE: Create model for saving state of users answers.

        public UserAnswerResolveService(UserAnswersModel userAnswers, QuestionModel questionModel)
        {
            this.userAnswers = userAnswers;
            this.questionModel = questionModel;
        }

        /// <summary>
        /// Compare user answers with "meta" field.
        /// 
        /// </summary>
        public DecisionModel GetDecision()
        {
            var decision = questionModel.Decisions.Where(d => d.Meta.Equals(userAnswers.Answers)).First();
            
            return decision;
        }
    }
}
