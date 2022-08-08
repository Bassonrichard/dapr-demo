using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Adds swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Adds cloud events to deserialise the payloads coming from dapr in the cloud events format.
// You can read more about it in this repo:https://github.com/cloudevents/spec/tree/v1.0
app.UseCloudEvents();

// This sets up the pub/sub routing for the services.
app.MapSubscribeHandler();

if (app.Environment.IsDevelopment()) 
{ 
    app.UseSwagger();
    app.UseSwaggerUI();
}


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

const string STATE_STORE_NAME = "statestore";
const string PUBSUB_NAME = "pubsub";

/// <summary>
/// This endpoint serves as a webhook endpoint for dapr to send requests to when it receives pub/sub events.
/// It will listen on the pubsub service with the name as set in PUBSUB_NAME on the topic forecast.
/// This endpoint just logs the data when received.
/// </summary>
app.MapPost("/forecast", [Topic(PUBSUB_NAME, "forecast")] (List<WeatherForecast> weatherForecast) => {

    foreach (var forecast in weatherForecast)
    {
        Console.WriteLine("Forecast received: " + forecast);
    }

    return Results.Ok(weatherForecast);
})
.WithName("RecieveWeatherForecast");


/// <summary>
/// This endpoint allows you to send the city and receive the generated weather forecast for that city.
/// It will check the state to see if it already has data for that city then return it if it does.
/// If it does not have data for the city it will generate it, save it in the state and return it.
/// This endpoint is called by dapetestserviceone when getting the forecast data.
/// </summary>
app.MapPost("/weatherforecast", async ([FromBody]string city) =>
{
    using var client = new DaprClientBuilder().Build();
    var forecastState = await client.GetStateAsync<IEnumerable<WeatherForecast>>(STATE_STORE_NAME, city);

    if (forecastState is null)
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
          new WeatherForecast
          (
              DateTime.Now.AddDays(index),
              Random.Shared.Next(-20, 55),
              summaries[Random.Shared.Next(summaries.Length)]
          ))
          .ToArray();

        await client.SaveStateAsync<IEnumerable<WeatherForecast>>(STATE_STORE_NAME, city, forecast);

        forecastState = forecast;
    }
    
    return forecastState;
})
.WithName("GetWeatherForecast");

app.Run();

public record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}