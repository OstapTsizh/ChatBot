using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Template.Core.Interfaces;
using Template.Core.Models;

namespace DecisionMakers
{
    public class DecisionMaker : IDecisionMaker
    {
        // Where to save previous Keywords or state?
        // in the Model or in Some Context (in the Bot or in the QAConstructor)

        public DecisionMaker() { }

        public QuestionModel GetQuestionOrResult(string topic)
        {
            // TODO: Create DB
            var path = @"..\Bot.Core\Dialogs.json"; //D:\GitHub\Template

            var json = File.ReadAllText(path);

            var jObject = JArray.Parse(json); // was - JObject

            var model = GetModel(jObject, topic);
            

            return model;
        }

       

        private QuestionModel GetModel(JArray rss, string topic)
        {
            var model = new QuestionModel();

            // Taking array of all tokens.
            var tokens = rss.Children();

            // Searching in array token with given topic 
            foreach (var item in tokens)
            {
                if((string)item["KEY"] == topic)
                {
                    var keywords = item["model"]["keywords"];
                    var questions = item["model"]["questions"];
                    var decisions = item["model"]["decisions"];

                    model.Name = (string)item["model"]["name"];
                    model.Keywords = keywords.ToObject<string[]>();
                    model.Questions = questions.ToObject<List<string>>();

                    // This doesnt work
                    // Possible fix: select inner token and create decision models
                    // model.Decisions = questions.ToObject<List<DecisionModel>>();
                }
            }

            
            return model;
        }

       
    }
}
