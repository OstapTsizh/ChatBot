using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonEditor.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {
        private readonly string _pathMainMenu = @"..\Bot.Core\DataFiles\MainMenu.json";


        [HttpPost]
        [Route("MainMenu")]
        public IActionResult MainMenu([FromBody]MainMenuModel model)
        {
            var jsonModel = JsonConvert.DeserializeObject<MainMenuModel[]>(System.IO.File.ReadAllText(_pathMainMenu));
            
            foreach (var token in jsonModel)
            {
                if( token.lang == model.lang)
                {
                    token.items.AddRange(model.items);
                }
            }

            var newJson = JsonConvert.SerializeObject(jsonModel);

            System.IO.File.WriteAllText(_pathMainMenu,  newJson);

            return Ok(newJson);
        }

        [HttpGet("[action]")]
        public IActionResult MainMenu()
        {
            var jsonModel = JsonConvert.DeserializeObject<MainMenuModel[]>(System.IO.File.ReadAllText(_pathMainMenu));
            return Ok(jsonModel);
        }



        public class MainMenuModel
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







        private static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet("[action]")]
        public IEnumerable<WeatherForecast> WeatherForecasts()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                DateFormatted = DateTime.Now.AddDays(index).ToString("d"),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            });
        }

        public class WeatherForecast
        {
            public string DateFormatted { get; set; }
            public int TemperatureC { get; set; }
            public string Summary { get; set; }

            public int TemperatureF
            {
                get
                {
                    return 32 + (int)(TemperatureC / 0.5556);
                }
            }
        }
    }
}
