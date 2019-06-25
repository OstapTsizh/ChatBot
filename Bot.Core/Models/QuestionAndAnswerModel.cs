using System;
using System.Collections.Generic;
using System.Text;

namespace Template.Core.Models
{
    public class QuestionAndAnswerModel
    {
        public List<string> Answers { get; set; }
        public QuestionModel QuestionModel { get; set; }
    }
}
