using System.Text;
using System.Text.Json;
using DiningPhilosophers.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;

namespace DiningPhilosophers.CoordinatorService
{
    public class CoordinatorWorker : BackgroundService
    {
        private readonly ILogger<CoordinatorWorker> _logger;
        private readonly IConnection _rabbitConnection;
        
        private readonly ConcurrentDictionary<int, bool> _forks = new();
        private readonly ConcurrentDictionary<string, int> _philosopherEatCounts = new();
        private readonly ConcurrentQueue<PermissionRequestEvent> _waitingQueue = new();
        private readonly object _lock = new object();

        private const string RequestQueueName = "permission_requests";
        private const string ReleaseQueueName = "forks_released";
        private const string GrantExchangeName = "permission_grants";

        public CoordinatorWorker(ILogger<CoordinatorWorker> logger, IConnection rabbitConnection, IConfiguration config)
        {
            _logger = logger;
            _rabbitConnection = rabbitConnection;
            var philosopherCount = int.Parse(config["PHILOSOPHERS_COUNT"] ?? "5");
            for (int i = 0; i < philosopherCount; i++)
            {
                _forks.TryAdd(i, false);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var requestChannel = _rabbitConnection.CreateModel();
            using var releaseChannel = _rabbitConnection.CreateModel();
            
            SetupRabbitMq(requestChannel, releaseChannel);

            var requestConsumer = new AsyncEventingBasicConsumer(requestChannel);
            requestConsumer.Received += (sender, args) =>
            {
                var request = Deserialize<PermissionRequestEvent>(args.Body.ToArray());
                if (request != null)
                {
                    _logger.LogInformation("Received permission request from {PhilosopherId} for forks {Left} and {Right}.", request.PhilosopherId, request.LeftForkId, request.RightForkId);
                    lock (_lock)
                    {
                        ProcessRequests(request);
                    }
                }
                return Task.CompletedTask;
            };

            var releaseConsumer = new AsyncEventingBasicConsumer(releaseChannel);
            releaseConsumer.Received += (sender, args) =>
            {
                var releaseEvent = Deserialize<ForksReleasedEvent>(args.Body.ToArray());
                if (releaseEvent != null)
                {
                    _logger.LogInformation("Received forks released event from {PhilosopherId} for forks {Left} and {Right}.", releaseEvent.PhilosopherId, releaseEvent.LeftForkId, releaseEvent.RightForkId);
                    lock (_lock)
                    {
                        _forks[releaseEvent.LeftForkId] = false;
                        _forks[releaseEvent.RightForkId] = false;
                        ProcessRequests(null);
                    }
                }
                return Task.CompletedTask;
            };

            requestChannel.BasicConsume(RequestQueueName, autoAck: true, consumer: requestConsumer);
            releaseChannel.BasicConsume(ReleaseQueueName, autoAck: true, consumer: releaseConsumer);

            _logger.LogInformation("Coordinator is running and waiting for events.");
            await Task.Delay(-1, stoppingToken);
        }

        private void ProcessRequests(PermissionRequestEvent? newRequest)
        {
            if (newRequest != null) _waitingQueue.Enqueue(newRequest);

            int count = _waitingQueue.Count;
            for (int i = 0; i < count; i++)
            {
                if (!_waitingQueue.TryDequeue(out var request)) continue;

                var minEatCount = _philosopherEatCounts.Any() ? _philosopherEatCounts.Values.Min() : 0;
                var currentEatCount = _philosopherEatCounts.GetValueOrDefault(request.PhilosopherId, 0);

                if (!_forks[request.LeftForkId] && !_forks[request.RightForkId] && currentEatCount <= minEatCount)
                {
                    _forks[request.LeftForkId] = true;
                    _forks[request.RightForkId] = true;
                    _philosopherEatCounts[request.PhilosopherId] = currentEatCount + 1;
                    _logger.LogInformation("Granting permission to {PhilosopherId} (Eat count: {Count}).", request.PhilosopherId, currentEatCount + 1);
                    PublishGrant(request.PhilosopherId);
                }
                else
                {
                    _waitingQueue.Enqueue(request);
                }
            }
        }

        private void PublishGrant(string philosopherId)
        {
            using var channel = _rabbitConnection.CreateModel();
            channel.ExchangeDeclare(GrantExchangeName, ExchangeType.Direct);
            var grantEvent = new PermissionGrantedEvent { PhilosopherId = philosopherId };
            var grantMessage = JsonSerializer.Serialize(grantEvent);
            var grantBody = Encoding.UTF8.GetBytes(grantMessage);
            channel.BasicPublish(GrantExchangeName, routingKey: philosopherId, body: grantBody);
        }

        private T? Deserialize<T>(byte[] body) => JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(body));

        private void SetupRabbitMq(IModel requestChannel, IModel releaseChannel)
        {
            requestChannel.QueueDeclare(RequestQueueName, durable: false, exclusive: false, autoDelete: false);
            releaseChannel.QueueDeclare(ReleaseQueueName, durable: false, exclusive: false, autoDelete: false);
        }
    }
}
