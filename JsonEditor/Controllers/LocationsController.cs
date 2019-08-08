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
    public class LocationsController : ControllerBase
    {
        private readonly string _pathLocations = @"..\Bot.Core\DataFiles\Locations.json";

        // GET: api/Locations
        [HttpGet]
        public IEnumerable<Location> Get()
        {
            var jsonModel = JsonConvert.DeserializeObject<Location[]>(System.IO.File.ReadAllText(_pathLocations));
            return jsonModel;
        }

        
        // POST: api/Locations
        [HttpPost]
        public void Post([FromBody] Location model)
        {



        }

        // PUT: api/Locations
        [HttpPut]
        public void Put([FromBody] Location[] model)
        {
            var newJson = JsonConvert.SerializeObject(model);

            System.IO.File.WriteAllText(_pathLocations, newJson);
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {

        }
    }
    

    public class Location
    {
        public string lang { get; set; }
        public LocationModel model { get; set; }
    }

    public class LocationModel
    {
        public string country { get; set; }
        public string[] cities { get; set; }
    }
}
