using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;
using System.Configuration;

namespace DecisionMakers
{
    public class DecisionMaker : IDecisionMaker
    {

        public string path { get; set; } = @"..\Bot.Core\Dialogs.json";

        public DecisionMaker() { }

        public List<string> GetStartTopics()
        {
            var json = File.ReadAllText(path);

            var jArray = JArray.Parse(json);

            var result = new List<string>();

            var tokens = jArray.Children();

            foreach (var item in tokens)
            {
                result.Add((string)item["topic"]);
            }

            return result;

        }

        public DecisionModel GetDecision(List<string> answers, QuestionModel questionModel)
        {
            var metaStr = new List<string>();

            for (var i = 0; i < questionModel.Decisions.Count(); i++)
            {
                for (var j = 0; j < questionModel.Decisions[i].Meta.Count(); j++)
                {
                    metaStr.Add(questionModel.Decisions[i].Meta[j]);
                }
                if (metaStr.Count == answers.Count && !metaStr.Except(answers).Any())
                {
                    return questionModel.Decisions[i];
                }

                metaStr.Clear();
            }

            return null;
        }

        public QuestionModel GetQuestionOrResult(string topic)
        {
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
                if ((string)item["topic"] == topic)
                {
                    var keywords = item["model"]["keywords"];
                    var questions = item["model"]["questions"].Children();
                    var decisions = item["model"]["decisions"].Children();

                    model.Name = (string)item["model"]["name"];
                    model.Keywords = keywords.ToObject<string[]>();
                    model.Questions = new List<Question>();
                    model.Decisions = new List<DecisionModel>();



                    foreach (var decision in decisions)
                    {
                        var meta = decision["meta"];
                        model.Decisions.Add(new DecisionModel
                        {
                            Meta = meta.ToObject<string[]>(),
                            Answer = (string)decision["answer"],
                            Resources = (string)decision["resourses"]
                        });
                    }

                    foreach (var question in questions)
                    {
                        model.Questions.Add(new Question
                        {
                            Text = (string)question["Text"],
                            IsAnswered = (string)question["IsAnswered"],
                            Keywords = question["keywords"].ToObject<string[]>()
                        });
                    }


                }
            }


            return model;
        }


    }
}
