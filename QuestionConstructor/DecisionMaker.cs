using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;
using System.Configuration;
using System.Text;

namespace DecisionMakers
{
    /// <summary>
    /// The main DecisionMaker class.
    /// Provides communication between the bot and the json file.
    /// </summary>
    public class DecisionMaker : IDecisionMaker
    {

        public readonly string _path = @"..\Bot.Core\Dialogs.json";
        public readonly string _pathLocations = @"..\Bot.Core\DataFiles\Locations.json";
        public readonly string _pathMainMenu = @"..\Bot.Core\DataFiles\MainMenu.json";
        public readonly string _pathQAs = @"..\Bot.Core\DataFiles\QAs.json";
        public readonly string _pathCourses = @"..\Bot.Core\DataFiles\Courses.json";
        public readonly string _pathPlannedEvents = @"..\Bot.Core\DataFiles\PlannedEvents.json";
        public readonly string _pathChooseOptionList = @"..\Bot.Core\DataFiles\ChooseOptionList.json";

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
            var json = File.ReadAllText(_path);

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
            var json = File.ReadAllText(_path);

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


        // ToDo return correct data from jsons



        /// <summary>
        /// Method that takes and store all info about locations. 
        /// </summary>
        /// <param name="rss"> Array of tokens in json file. </param>
        /// <param name="lang"> Topic from which info will be taken. </param>
        /// <returns> New instance of <see cref="QuestionModel"/> class. </returns>
        private Country GetLocationsModel(string lang)
        {
            string json = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(File.ReadAllText(_pathLocations)));
            var rss = JArray.Parse(json);
            var model = new Country();

            // Taking array of all tokens.
            var tokens = rss.Children();

            // Searching in array token with given topic 
            foreach (var item in tokens)
            {
                if (item["lang"].ToObject<string[]>().Contains(lang))
                {
                    var country = item["model"]["country"];
                    var cities = item["model"]["cities"];
                    
                    model.CountryName = country.ToString();
                    model.Cities = cities.ToObject<List<string>>();
                }
            }

            return model;
        }

        /// <summary>
        /// Gets all available countries from json file.
        /// </summary>
        /// <returns></returns>
        public List<Country> GetCountries(string lang)
        {
            // ToDo if needed more countries change GetLocationsModel and json

            List<Country> countries = new List<Country>
            {
                GetLocationsModel(lang)
            };

            return countries;
        }
        
        /// <summary>
        /// Gets all localized main menu items (e.g. navigation bar)
        /// from the json file.
        /// </summary>
        /// <returns></returns>
        public List<MainMenuItem> GetMainMenuItems(string lang)
        {
            var json = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(File.ReadAllText(_pathMainMenu)));
            var rss = JArray.Parse(json);
            var model = new MainMenu();

            // Taking array of all tokens.
            var tokens = rss.Children();

            // Searching in array token with given topic 
            foreach (var item in tokens)
            {
                if (item["lang"].ToObject<string[]>().Contains(lang))
                {
                    var items = item["items"];
                    
                    model.Items = items.ToObject<List<MainMenuItem>>();
                }
            }

            return model.Items;
        }

        /// <summary>
        /// Gets all main menu items (e.g. navigation bar) from the json
        /// file in the neutral language.
        /// </summary>
        /// <returns></returns>
        public List<string> GetMainMenuItemsNeutral()
        {
            var mainMenuItems = new List<string>();

            return mainMenuItems;
        }

        /// <summary>
        /// Gets courses in selected city from the json file.
        /// </summary>
        /// <returns></returns>
        public List<Course> GetCourses(string lang)
        {
            var json = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(File.ReadAllText(_pathCourses)));
            var rss = JArray.Parse(json);
            var courses = new List<Course>();

            // Taking array of all tokens.
            var tokens = rss.Children();

            // Searching in array token with given topic 
            foreach (var item in tokens)
            {
                if (item["lang"].ToObject<string[]>().Contains(lang))
                {
                    var items = item["model"]["courses"];

                    courses = items.ToObject<List<Course>>();
                }
            }

            return courses;
        }

        //public List<string> GetAbout()
        //{

        //}

        /// <summary>
        /// Gets questions/answers from the json.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<string>> GetQAs(string lang)
        {
            var json = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(File.ReadAllText(_pathQAs)));
            var rss = JArray.Parse(json);
            var qAs = new Dictionary<string, List<string>>();
            var tmpQAs = new List<QA>();

            // Taking array of all tokens.
            var tokens = rss.Children();

            // Searching in array token with given topic 
            foreach (var item in tokens)
            {
                if (item["lang"].ToObject<string[]>().Contains(lang))
                {
                    var items = item["model"]["QAs"];

                    tmpQAs = items.ToObject<List<QA>>();
                }
            }
            foreach (var tmpQA in tmpQAs)
            {
                qAs.Add(tmpQA.Question,tmpQA.Answer);
            }

            return qAs;
        }

        /// <summary>
        /// Gets planned events from the json.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<string>> GetPlannedEvents(string lang)
        {
            var json = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(File.ReadAllText(_pathPlannedEvents)));
            var rss = JArray.Parse(json);
            var model = new List<PlannedEvent>();

            // Taking array of all tokens.
            var tokens = rss.Children();

            // Searching in array token with given topic 
            foreach (var item in tokens)
            {
                if (item["lang"].ToObject<string[]>().Contains(lang))
                {
                    var items = item["events"];

                    model = items.ToObject<List<PlannedEvent>>();
                }
            }

            var events = new Dictionary<string, List<string>>();
            foreach (var plannedEvent in model)
            {
                events.Add(plannedEvent.Name, plannedEvent.Resources);
            }

            return events;
        }

        /// <summary>
        /// Gets  a ChooseOption list from the json.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetChooseOptions(string lang)
        {
            var json = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(File.ReadAllText(_pathChooseOptionList)));
            var rss = JArray.Parse(json);
            var options = new Dictionary<string, string>();
            var tmpOptions = new List<ChooseOptionList>();

            // Taking array of all tokens.
            var tokens = rss.Children();

            // Searching in array token with given topic 
            foreach (var item in tokens)
            {
                if (item["lang"].ToObject<string[]>().Contains(lang))
                {
                    var items = item["items"];

                    tmpOptions = items.ToObject<List<ChooseOptionList>>();
                }
            }
            foreach (var tmpOption in tmpOptions)
            {
                options.Add(tmpOption.Name, tmpOption.Id);
            }

            return options;
        }
    }
}
