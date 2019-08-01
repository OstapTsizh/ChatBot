using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace JsonEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {

        private readonly string _pathCourses = @"..\Bot.Core\DataFiles\Courses.json";

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

            foreach (var token in jsonModel)
            {
                if (token.lang == model.lang)
                {
                    token.courses.AddRange(model.courses);
                }
            }

            var newJson = JsonConvert.SerializeObject(jsonModel);

            System.IO.File.WriteAllText(_pathCourses, newJson);

        }

        // PUT: api/Courses
        [HttpPut]
        public void Put( [FromBody] Course[] model)
        {
            var newJson = JsonConvert.SerializeObject(model);

            System.IO.File.WriteAllText(_pathCourses, newJson);
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
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
    }


}
