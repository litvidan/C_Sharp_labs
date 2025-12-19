using DiningPhilosophers.TableService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var philosophersCountString = Environment.GetEnvironmentVariable("PHILOSOPHERS_COUNT");
if (!int.TryParse(philosophersCountString, out var philosophersCount) || philosophersCount < 2)
{
    philosophersCount = 5;
    Console.WriteLine($"PHILOSOPHERS_COUNT environment variable not set or invalid. Defaulting to {philosophersCount}.");
}

builder.Services.AddSingleton<IForkManager>(new ForkManager(philosophersCount));
builder.Services.AddSingleton<IMetricsCollector>(new MetricsCollector(philosophersCount));

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapHealthChecks("/health");

// Record initial state for forks
var metrics = app.Services.GetRequiredService<IMetricsCollector>();
for (int i = 0; i < philosophersCount; i++)
{
    metrics.RecordForkUsage(i, false);
}

app.Run();
