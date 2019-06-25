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

        public DecisionMaker() { }

        public string[] GetStartTopics()
        {
            var path = @"..\Bot.Core\Dialogs.json";

            var json = File.ReadAllText(path);

            var jArray = JArray.Parse(json);

            var result = new List<string>();

            var tokens = jArray.Children();

            foreach (var item in tokens)
            {
                result.Add((string)item["KEY"]);
            }

            return result.ToArray();

        }

        

        public QuestionModel GetQuestionOrResult(string topic)
        {            
            var path = @"..\Bot.Core\Dialogs.json";

            var json = File.ReadAllText(path);

            var jObject = JArray.Parse(json); 

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
                    var decisions = item["model"]["decisions"].Children();

                    model.Name = (string)item["model"]["name"];
                    model.Keywords = keywords.ToObject<string[]>();
                    model.Questions = questions.ToObject<List<string>>();
                    model.Decisions = new List<DecisionModel>();


                    foreach(var decision in decisions)
                    {
                        var meta = decision["meta"];
                        model.Decisions.Add(new DecisionModel
                        {
                            Meta = meta.ToObject<string[]>(),
                            Answer = (string)decision["answer"],
                            Resources = (string)decision["resources"]
                        });
                    }

                }
            }

            
            return model;
        }

       
    }
}
