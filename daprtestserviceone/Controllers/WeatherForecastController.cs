using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;

namespace daprtestserviceone.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private const string STATE_STORE_NAME = "statestore";
        private const string PUBSUB_NAME = "pubsub";

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly DaprClient _daprClient;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        /// <summary>
        /// This endpoint checks the state using the dapr state store.
        /// If there is nothing in the state store for the given key (city)
        /// it will invoke the endpoint on service two called weatherforecast.
        /// This service will then return the forecast. 
        /// </summary>
        /// <param name="city">The city you're requesting the forecast of</param>
        /// <returns>The weatherforecast for that specific city</returns>
        [HttpGet("{city}", Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get(string city, [FromState(STATE_STORE_NAME, "city")] StateEntry<IEnumerable<WeatherForecast>> forecastState)
        {
            if (forecastState.Value is null)
            {
                try
                {
                    forecastState.Value = await _daprClient.InvokeMethodAsync<string, IEnumerable<WeatherForecast>>("daprtestservicetwo", "weatherforecast", city);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while calling daprtestservicetwo weatherforecast");
                    throw;
                }

            }

            return Ok(forecastState.Value);
        }


        /// <summary>
        /// This service saves or updates the state for a given city.
        /// The state value is set to the weatherForecast given.
        /// </summary>
        /// <param name="city">The city you want to save or update the state of.</param>
        /// <param name="weatherForecast">The forecast you want to save of update the state with.</param>
        /// <returns>The updated forecast.</returns>
        [HttpPut("{city}", Name = "PostWeatherForecast")]
        public async Task<IActionResult> Post(string city, IEnumerable<WeatherForecast> weatherForecast)
        {
            await _daprClient.SaveStateAsync(STATE_STORE_NAME, city, weatherForecast);

            return Ok(weatherForecast);
        }

        /// <summary>
        /// Publishes the given forecast to the forecast topic.
        /// </summary>
        /// <param name="weatherForecast">The forecast you want to publish to the topic.</param>
        /// <returns>The published forecast value.</returns>
        [HttpPost(Name = "PublishWeatherForecast")]
        public async Task<IActionResult> Publish(IEnumerable<WeatherForecast> weatherForecast)
        {
            await _daprClient.PublishEventAsync(PUBSUB_NAME, "forecast", weatherForecast);

            return Ok(weatherForecast);
        }
    }
}