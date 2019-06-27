using System;
using System.Collections.Generic;
using System.Text;

namespace Template.Core.Models
{
    public class QuestionModel
    {
        public string Name { get; set; }

        public string[] Keywords { get; set; }

        public List<Question> Questions { get; set; }        

        public List<DecisionModel> Decisions { get; set; }

    }
}
