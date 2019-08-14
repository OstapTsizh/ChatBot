using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using StuddyBot.Core;
using StuddyBot.Core.DAL.Data;
using StuddyBot.Core.Models;

namespace DecisionMakerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly PathSettings _pathSettings;

        private StuddyBotContext studdyBotContext;

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{method}/{lang}")]
        public ActionResult<string> Get(string method, string lang)
        {
            switch (method)
            {
                case "GetCourses":

                    break;
                default:
                    break;
            }

            return "value";
        }
        

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        /// <summary>
        /// Gets courses in selected city from the json file.
        /// </summary>
        /// <returns></returns>
        public List<Course> GetCourses(string lang)
        {
            var json = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(System.IO.File.ReadAllText(_pathSettings.PathCourses)));
            var rss = JArray.Parse(json);
            var courses = new List<Course>();

            // Taking array of all tokens.
            var tokens = rss.Children();

            // Searching in array token with given topic 
            foreach (var item in tokens)
            {
                if (item["lang"].ToObject<string>() == lang)
                {
                    var items = item["courses"];

                    courses = items.ToObject<List<Course>>();
                }
            }

            // ToDo check this !!!!!

            // Insert into db
            studdyBotContext = new StuddyBotContext();
            if (!studdyBotContext.Courses.Any())
            {
                var coursesDB = new List<Course>();
                foreach (var item in tokens)
                {
                    if (item["lang"].ToObject<string>().ToLower() == "en-us")
                    {
                        var items = item["courses"];

                        coursesDB = items.ToObject<List<Course>>();
                    }
                }
                studdyBotContext.PushCoursesToDB(coursesDB);
                studdyBotContext.SaveChanges();
            }

            return courses;
        }
    }
}
