using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

namespace DecisionMakers
{
    /// <summary>
    /// The main DecisionMaker class.
    /// Provides communication between the bot and the json file.
    /// </summary>
    public class DecisionMaker : IDecisionMaker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecisionMaker"/> class.
        /// </summary>
        public DecisionMaker() { }

        /// <summary>
        /// Method which get all topics from json file.
        /// </summary>
        /// <returns> List of all topics. </returns>
        public List<string> GetStartTopics()
        {

            var path = @"..\Bot.Core\Dialogs.json";

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

        /// <summary>
        /// Method which get result or question according to topic.
        /// </summary>
        /// <param name="topic"> Topic from which the result or question will be taken. </param>
        /// <returns> QuestionModel object of <see cref="QuestionModel"/> class. </returns>
        public QuestionModel GetQuestionOrResult(string topic)
        {            
            var path = @"..\Bot.Core\Dialogs.json";

            var json = File.ReadAllText(path);

            var jObject = JArray.Parse(json); 

            var model = GetModel(jObject, topic);
            

            return model;
        }

        /// <summary>
        /// Method that takes and store all info about topic. 
        /// </summary>
        /// <param name="rss"> Array of tokens in json file. </param>
        /// <param name="topic"> Topic from which info will be taken. </param>
        /// <returns> New instance of <see cref="QuestionModel"/> class. </returns>
        private QuestionModel GetModel(JArray rss, string topic)
        {
            var model = new QuestionModel();

            // Taking array of all tokens.
            var tokens = rss.Children();

            // Searching in array token with given topic 
            foreach (var item in tokens)
            {
                if((string)item["topic"] == topic)
                {
                    var keywords = item["model"]["keywords"];
                    var questions = item["model"]["questions"].Children();
                    var decisions = item["model"]["decisions"].Children();

                    model.Name = (string)item["model"]["name"];
                    model.Keywords = keywords.ToObject<string[]>();
                    model.Questions = new List<Question>();
                    model.Decisions = new List<DecisionModel>();

                   

                    foreach(var decision in decisions)
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
