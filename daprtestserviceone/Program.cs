var builder = WebApplication.CreateBuilder(args);

// Adds dapr to the service controllers.
builder.Services.AddControllers().AddDapr();

// Adds swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adds the dapr client so you can inject it into your services.
builder.Services.AddDaprClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
