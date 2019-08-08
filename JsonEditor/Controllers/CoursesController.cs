using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecisionMakers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StuddyBot.Core.DAL.Data;

namespace JsonEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {

        private readonly string _pathCourses = @"..\Bot.Core\DataFiles\Courses.json";

        private StuddyBotContext _db;

        public CoursesController(StuddyBotContext db)
        {
            _db = db;
        }



        // GET: api/Courses
        [HttpGet]
        public IEnumerable<Course> Get()
        {
            var jsonModel = JsonConvert.DeserializeObject<Course[]>(System.IO.File.ReadAllText(_pathCourses));
            return jsonModel;
        }

        
        // POST: api/Courses
        [HttpPost]
        public void Post([FromBody] Course model)
        {
            var jsonModel = JsonConvert.DeserializeObject<Course[]>(System.IO.File.ReadAllText(_pathCourses));

            if(jsonModel != null)
            {
                foreach (var token in jsonModel)
                {
                    if (token.lang == model.lang)
                    {
                        token.courses.AddRange(model.courses);
                    }
                }
            }
            else
            {
                jsonModel = new Course[]
                {
                    new Course
                    {
                        lang = model.lang,
                        courses = model.courses
                    }
                };
            }

            // Write to json.
            var newJson = JsonConvert.SerializeObject(jsonModel);
            System.IO.File.WriteAllText(_pathCourses, newJson);

            // Write to db.
            var course = new StuddyBot.Core.DAL.Entities.Course
            {
                Name = model.courses[0].name,
                StartDate = model.courses[0].StartDate,
                RegistrationStartDate = model.courses[0].RegistrationStartDate
            };

            _db.Add(course);
            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                //TODO: Log error
            }
        }

        // PUT: api/Courses
        [HttpPut("{i}/{j}")]
        public void Put(int i, int j, [FromBody] CoursesInner model)
        {
            var jsonModel = JsonConvert.DeserializeObject<Course[]>(System.IO.File.ReadAllText(_pathCourses));

            if (jsonModel != null)
            {
                //Write to json.
                jsonModel[i].courses[j] = model;
                System.IO.File.WriteAllText(_pathCourses, JsonConvert.SerializeObject(jsonModel));
            }

            // Write to db.   
            //TODO: Imporve algorithm. May cause inaccuracies if there are courses with same start dates.
            var course = _db.Courses.Where(prop => prop.Name == model.name ||
                                           prop.RegistrationStartDate == model.RegistrationStartDate ||
                                           prop.StartDate == model.StartDate).FirstOrDefault();

            course.Name = model.name;
            course.RegistrationStartDate = model.RegistrationStartDate;
            course.StartDate = model.StartDate;

            try
            {
                //TODO: Fill db with actual curses.
                _db.Entry(course).State = EntityState.Modified;
                _db.SaveChanges();
            }
            catch (Exception e)
            {
                // TODO: log error
            }



        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            //TODO: delete item from database
        }
    }




    public class Course
    {
        public string lang { get; set; }
        public List<CoursesInner> courses { get; set; }
    }

    public class CoursesInner
    {
        public string name { get; set; }
        public string resources { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime RegistrationStartDate { get; set; }

    }


}
