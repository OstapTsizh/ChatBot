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
    public class PlannedEventsController : ControllerBase
    {
        private readonly string _pathPlannedEvents = @"..\Bot.Core\DataFiles\PlannedEvents.json";

        // GET: api/PlannedEvents
        [HttpGet]
        public IEnumerable<PlannedEvent> Get()
        {
            var jsonModel = JsonConvert.DeserializeObject<PlannedEvent[]>(System.IO.File.ReadAllText(_pathPlannedEvents));
            return jsonModel;
        }

        // GET: api/PlannedEvents/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/PlannedEvents
        [HttpPost]
        public void Post([FromBody] PlannedEvent model)
        {
            var jsonModel = JsonConvert.DeserializeObject<PlannedEvent[]>(System.IO.File.ReadAllText(_pathPlannedEvents));

            foreach (var token in jsonModel)
            {
                if (token.lang == model.lang)
                {
                    token.events.AddRange(model.events);
                }
            }

            var newJson = JsonConvert.SerializeObject(jsonModel);

            System.IO.File.WriteAllText(_pathPlannedEvents, newJson);
        }

        // PUT: api/PlannedEvents/5
        [HttpPut]
        public void Put([FromBody] PlannedEvent[] model)
        {
            var newJson = JsonConvert.SerializeObject(model);

            System.IO.File.WriteAllText(_pathPlannedEvents, newJson);
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }



        public class PlannedEvent
        {
            public string lang { get; set; }
            public List<Event> events { get; set; }
        }

        public class Event
        {
            public string name { get; set; }
            public string[] resources { get; set; }
        }

    }
}
