using DiningPhilosophers.PhilosopherService;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IConnection>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<Program>>();
    var configuration = sp.GetRequiredService<IConfiguration>();
    var rabbitMqHost = configuration["RABBITMQ_HOST"] ?? "localhost";
    var factory = new ConnectionFactory() { HostName = rabbitMqHost, DispatchConsumersAsync = true };

    const int maxRetries = 50;
    const int delaySeconds = 5;
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            logger.LogInformation("Attempting to connect to RabbitMQ... (Attempt {AttemptNumber}/{MaxAttempts})", i + 1, maxRetries);
            return factory.CreateConnection();
        }
        catch (BrokerUnreachableException ex)
        {
            logger.LogWarning(ex, "Could not connect to RabbitMQ. Retrying in {Delay} seconds...", delaySeconds);
            Thread.Sleep(delaySeconds * 1000);
        }
    }
    throw new Exception("Could not connect to RabbitMQ after multiple retries.");
});

builder.Services.AddHostedService<PhilosopherWorker>();

var app = builder.Build();

app.MapGet("/", () => "Philosopher service is running.");

app.Run();
