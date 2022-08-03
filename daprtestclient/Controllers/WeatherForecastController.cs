using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;

namespace daprtestclient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private const string STORE_NAME = "statestore";

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly DaprClient _daprClient;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        [HttpGet("{city}", Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get(string city, [FromState(STORE_NAME, "city")] StateEntry<IEnumerable<WeatherForecast>> forecastState)
        {
            if (forecastState.Value is null)
            {
                try
                {
                    forecastState.Value = await _daprClient.InvokeMethodAsync<string, IEnumerable<WeatherForecast>>("daprtestserver", "weatherforecast", city);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while calling daprtestserver weatherforecast");
                    throw;
                }
              
            }

            return Ok(forecastState.Value);
        }

        [HttpPut("{city}", Name = "PostWeatherForecast")]
        public async Task<IActionResult> Post(string city, IEnumerable<WeatherForecast> weatherForecast)
        {
            await _daprClient.SaveStateAsync(STORE_NAME, city, weatherForecast);

            return Ok(weatherForecast);
        }


        [HttpPost(Name = "PublishWeatherForecast")]
        public async Task<IActionResult> Publish(IEnumerable<WeatherForecast> weatherForecast)
        {
            // Publish an event/message using Dapr PubSub
            await _daprClient.PublishEventAsync("pubsub", "forecast", weatherForecast);

            return Ok(weatherForecast);
        }
    }
}