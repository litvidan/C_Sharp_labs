using System.Text;
using System.Text.Json;
using DiningPhilosophers.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DiningPhilosophers.CoordinatorService
{
    public class CoordinatorWorker : BackgroundService
    {
        private readonly ILogger<CoordinatorWorker> _logger;
        private readonly IConnection _rabbitConnection;
        private readonly SemaphoreSlim _eatingSemaphore = new(1, 1);

        private const string RequestQueueName = "permission_requests";
        private const string ReleaseQueueName = "forks_released";
        private const string GrantExchangeName = "permission_grants";

        public CoordinatorWorker(ILogger<CoordinatorWorker> logger, IConnection rabbitConnection)
        {
            _logger = logger;
            _rabbitConnection = rabbitConnection;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Create a separate channel for each consumer. This is crucial for thread safety.
            using var requestChannel = _rabbitConnection.CreateModel();
            using var releaseChannel = _rabbitConnection.CreateModel();

            // Setup queues and exchanges
            requestChannel.QueueDeclare(RequestQueueName, durable: false, exclusive: false, autoDelete: false);
            releaseChannel.QueueDeclare(ReleaseQueueName, durable: false, exclusive: false, autoDelete: false);
            requestChannel.ExchangeDeclare(GrantExchangeName, ExchangeType.Direct);

            // --- Consumer for Permission Requests ---
            var requestConsumer = new AsyncEventingBasicConsumer(requestChannel);
            requestConsumer.Received += async (sender, args) =>
            {
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var request = JsonSerializer.Deserialize<PermissionRequestEvent>(message);
                if (request != null)
                {
                    _logger.LogInformation("Received permission request from {PhilosopherId}. Waiting for semaphore...", request.PhilosopherId);
                    await _eatingSemaphore.WaitAsync(stoppingToken);
                    _logger.LogInformation("Semaphore acquired for {PhilosopherId}. Granting permission.", request.PhilosopherId);

                    var grantEvent = new PermissionGrantedEvent { PhilosopherId = request.PhilosopherId };
                    var grantMessage = JsonSerializer.Serialize(grantEvent);
                    var grantBody = Encoding.UTF8.GetBytes(grantMessage);
                    // Use the same channel to publish the reply
                    requestChannel.BasicPublish(GrantExchangeName, routingKey: request.PhilosopherId, body: grantBody);
                }
            };
            requestChannel.BasicConsume(RequestQueueName, autoAck: true, consumer: requestConsumer);


            // --- Consumer for Fork Releases ---
            var releaseConsumer = new AsyncEventingBasicConsumer(releaseChannel);
            releaseConsumer.Received += (sender, args) =>
            {
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var releaseEvent = JsonSerializer.Deserialize<ForksReleasedEvent>(message);
                if (releaseEvent != null)
                {
                    _logger.LogInformation("Received forks released event from {PhilosopherId}. Releasing semaphore.", releaseEvent.PhilosopherId);
                    _eatingSemaphore.Release();
                }
                return Task.CompletedTask;
            };
            releaseChannel.BasicConsume(ReleaseQueueName, autoAck: true, consumer: releaseConsumer);

            _logger.LogInformation("Coordinator is running and waiting for events.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
            
            _logger.LogInformation("Coordinator service is stopping.");
        }
    }
}
