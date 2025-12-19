using DiningPhilosophers.PhilosopherService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddHostedService<PhilosopherWorker>();

var app = builder.Build();

app.MapGet("/", () => "Philosopher service is running.");

app.Run();
