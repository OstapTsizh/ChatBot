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
    public class MainMenuController : ControllerBase
    {
        private readonly string _pathMainMenu;

        public MainMenuController(IPathSettings ps)
        {
            _pathMainMenu = ps.PathMainMenu;
        }

        // GET: api/MainMenu
        [HttpGet]
        public IEnumerable<MainMenu> Get()
        {
            var jsonModel = JsonConvert.DeserializeObject<MainMenu[]>(System.IO.File.ReadAllText(_pathMainMenu));
            return jsonModel;
        }


        // POST: api/MainMenu
        [HttpPut]
        public void Put([FromBody] MainMenu[] model)
        {
            var newJson = JsonConvert.SerializeObject(model);

            System.IO.File.WriteAllText(_pathMainMenu, newJson);
        }


        // PUT: api/MainMenu
        [HttpPost]
        public void Post([FromBody] MainMenu model)
        {
            var jsonModel = JsonConvert.DeserializeObject<MainMenu[]>(System.IO.File.ReadAllText(_pathMainMenu));

            foreach (var token in jsonModel)
            {
                if (token.lang == model.lang)
                {
                    token.items.AddRange(model.items);
                }
            }

            var newJson = JsonConvert.SerializeObject(jsonModel);

            System.IO.File.WriteAllText(_pathMainMenu, newJson);
        }


    }



    public class MainMenu
    {
        public string lang { get; set; }
        public List<Item> items { get; set; }
    }

    public class Item
    {
        public string name { get; set; }
        public string dialog { get; set; }
        public string[] resources { get; set; }
    }

}