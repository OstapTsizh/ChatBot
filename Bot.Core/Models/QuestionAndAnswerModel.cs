using System.Collections.Generic;

namespace StuddyBot.Core.Models
{
    public class QuestionAndAnswerModel
    {
        public List<string> Answers { get; set; }

        public QuestionModel QuestionModel { get; set; }
    }
}
