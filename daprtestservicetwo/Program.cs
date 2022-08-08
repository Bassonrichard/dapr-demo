using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Dapr will send serialized event object vs. being raw CloudEvent
app.UseCloudEvents();

// needed for Dapr pub/sub routing
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

// Dapr subscription in [Topic] routes orders topic to this route
app.MapPost("/forecast", [Topic("pubsub", "forecast")] (List<WeatherForecast> weatherForecast) => {

    foreach (var forecast in weatherForecast)
    {
        Console.WriteLine("Forecast received: " + forecast);
    }

    return Results.Ok(weatherForecast);
})
.WithName("RecieveWeatherForecast");

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