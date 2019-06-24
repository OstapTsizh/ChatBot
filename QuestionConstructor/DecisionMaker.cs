using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Template.Core.Interfaces;

namespace DecisionMakers
{
    public class DecisionMaker : IDecisionMaker
    {
        // Where to save previous Keywords or state?
        // in the Model or in Some Context (in the Bot or in the QAConstructor)

        public DecisionMaker() { }

        public string GetQuestionOrResult(string topic)
        {
            throw new NotImplementedException();
            // ToDo
            // reading Json from file/DBContext etc.
            //return (string) ...;
        }

        public List<Template.Core.Interfaces.DecisionModel> LoadQAList()
        {
            // Need to be refactor LoadQAList(string path)
            var path = @"D:\EPAM_Course\Template\QuestionConstructor\Dialogs.json";

            using (StreamReader reader = new StreamReader(path))
            {
                string json = reader.ReadToEnd();
                List<DecisionModel> decisionModels = JsonConvert.DeserializeObject<List<DecisionModel>>(json);
                return decisionModels;
            }
        }
    }
}
