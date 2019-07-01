using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.IO;
using StuddyBot.Core.Models;

namespace DecisionMakerApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DecisionMakerController : ControllerBase
    {
        // GET: DecisionMaker/?????????????????
        [HttpGet(Name = "GetStartTopics")]
        public async Task<List<string>> GetStartTopics()
        {
            var path = @"..\Bot.Core\Dialogs.json";

            var result = new List<string>();

            var tokens = JArray.Parse(
                await System.IO.File.ReadAllTextAsync(path)
                ).Children();

            foreach (var item in tokens)
            {
                result.Add((string)item["topic"]);
            }

            return result;
        }

        [HttpGet(Name = "GetDecision")]
        public async Task<DecisionModel> GetDecision(List<string> answers, QuestionModel questionModel)
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

        [HttpGet(Name = "GetQuestionOrResult")]
        public async Task<QuestionModel> GetQuestionOrResult(string topic)
        {
            var path = @"..\Bot.Core\Dialogs.json";

            var json = await System.IO.File.ReadAllTextAsync(path);

            var jObject = JArray.Parse(json);

            var model = await GetModelAsync(jObject, topic);

            return model;
        }


        private async Task<QuestionModel> GetModelAsync(JArray rss, string topic)
        {
            var model = new QuestionModel();

            // Get array of all tokens.
            var tokens = rss.Children();

            // Searching in array token with given topic 
            foreach (var item in tokens)
            {
                if ((string)item["topic"] == topic)
                {
                    model.Name = (string)item["model"]["name"];
                    model.Keywords = item["model"]["keywords"].ToObject<string[]>();
                    model.Questions = new List<Question>();
                    model.Decisions = new List<DecisionModel>();

                    foreach (var decision in item["model"]["decisions"].Children())
                    {
                        var meta = decision["meta"];
                        model.Decisions.Add(new DecisionModel
                        {
                            Meta = meta.ToObject<string[]>(),
                            Answer = (string)decision["answer"],
                            Resources = (string)decision["resourses"]
                        });
                    }

                    foreach (var question in item["model"]["questions"].Children())
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
