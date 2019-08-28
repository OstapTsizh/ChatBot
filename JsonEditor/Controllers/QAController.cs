using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonEditor.ConfigurationModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace JsonEditor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QAController : ControllerBase
    {
        private readonly string _pathQAs;

        public QAController(IPathSettings ps)
        {
            _pathQAs = ps.PathQAs;
        }


        [HttpGet]
        public IActionResult Get()
        {
            var jsonModel = JsonConvert.DeserializeObject<QAModel[]>(System.IO.File.ReadAllText(_pathQAs));
            return Ok(jsonModel);
        }


        [HttpPut]
        public IActionResult Put([FromBody]QAModel[] model)
        {
            var newJson = JsonConvert.SerializeObject(model);

            System.IO.File.WriteAllText(_pathQAs, newJson);

            return Ok(newJson);
        }

        [HttpPost]
        public void Post([FromBody] QAModel model)
        {
            var jsonModel = JsonConvert.DeserializeObject<QAModel[]>(System.IO.File.ReadAllText(_pathQAs));

            foreach (var token in jsonModel)
            {
                if (token.lang == model.lang)
                {
                    token.qAs.AddRange(model.qAs);
                }
            }

            var newJson = JsonConvert.SerializeObject(jsonModel);

            System.IO.File.WriteAllText(_pathQAs, newJson);
        }




        public class QAModel
        {
            public string lang { get; set; }
            public List<QAInner> qAs { get; set; }
        }

        public class QAInner
        {
            public string question { get; set; }
            public string[] answer { get; set; }
        }

    }
}