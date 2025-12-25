using System.Text;
using System.Text.Json;
using DiningPhilosophers.Contracts;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.Http.Json;

namespace DiningPhilosophers.PhilosopherService
{
    public class PhilosopherWorker : BackgroundService
    {
        private readonly ILogger<PhilosopherWorker> _logger;
        private readonly string _philosopherName;
        private readonly string _philosopherId;
        private readonly int _leftForkId;
        private readonly int _rightForkId;
        private readonly Uri _tableServiceBaseUrl;
        private readonly TimeSpan _simulationDuration;
        private readonly HttpClient _httpClient;
        private readonly IConnection _rabbitConnection;
        private readonly Random _random = new();

        private TaskCompletionSource<bool> _permissionGrantedTcs = new();

        private const string RequestQueueName = "permission_requests";
        private const string ReleaseQueueName = "forks_released";
        private const string GrantExchangeName = "permission_grants";

        public PhilosopherWorker(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<PhilosopherWorker> logger, IConnection rabbitConnection)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _rabbitConnection = rabbitConnection;

            _philosopherName = configuration["PHILOSOPHER_NAME"] ?? "Unknown";
            _philosopherId = configuration["PHILOSOPHER_ID"] ?? Guid.NewGuid().ToString();
            _leftForkId = int.Parse(configuration["LEFT_FORK_ID"] ?? "0");
            _rightForkId = int.Parse(configuration["RIGHT_FORK_ID"] ?? "1");
            _tableServiceBaseUrl = new Uri(configuration["TABLE_SERVICE_URL"] ?? "http://localhost:8080");
            _simulationDuration = TimeSpan.FromMinutes(double.Parse(configuration["SIMULATION_DURATION_MINUTES"] ?? "1"));
            _httpClient.BaseAddress = _tableServiceBaseUrl;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var consumerChannel = _rabbitConnection.CreateModel();
            SetupRabbitMqConsumer(consumerChannel);
            
            var startTime = DateTime.UtcNow;
            await ReportStateChange("Thinking", stoppingToken);

            while (!stoppingToken.IsCancellationRequested && (DateTime.UtcNow - startTime) < _simulationDuration)
            {
                _logger.LogInformation("{Name} is thinking.", _philosopherName);
                await Task.Delay(_random.Next(2000, 5000), stoppingToken);

                await ReportStateChange("Hungry", stoppingToken);
                _logger.LogInformation("{Name} is hungry and requesting permission to eat.", _philosopherName);
                
                _permissionGrantedTcs = new TaskCompletionSource<bool>();
                PublishEvent(RequestQueueName, new PermissionRequestEvent 
                { 
                    PhilosopherId = _philosopherId,
                    LeftForkId = _leftForkId,
                    RightForkId = _rightForkId
                });
                
                await _permissionGrantedTcs.Task;

                if (stoppingToken.IsCancellationRequested) break;

                _logger.LogInformation("{Name} received permission. Taking forks.", _philosopherName);
                if (await TryTakeFork(_leftForkId, stoppingToken) && await TryTakeFork(_rightForkId, stoppingToken))
                {
                    await ReportStateChange("Eating", stoppingToken);
                    await Task.Delay(_random.Next(2000, 5000), stoppingToken);
                }
                else
                {
                    _logger.LogError("{Name} failed to take forks even with permission!", _philosopherName);
                }

                await ReleaseFork(_leftForkId, stoppingToken);
                await ReleaseFork(_rightForkId, stoppingToken);
                PublishEvent(ReleaseQueueName, new ForksReleasedEvent 
                { 
                    PhilosopherId = _philosopherId,
                    LeftForkId = _leftForkId,
                    RightForkId = _rightForkId
                });
                _logger.LogInformation("{Name} released forks and notified coordinator.", _philosopherName);

                await ReportStateChange("Thinking", stoppingToken);
            }

            _logger.LogInformation("{Name} has finished.", _philosopherName);
            await _httpClient.PostAsJsonAsync("api/metrics/finish", new TakeForkRequest { PhilosopherId = _philosopherId }, stoppingToken);
        }

        private void SetupRabbitMqConsumer(IModel channel)
        {
            channel.ExchangeDeclare(GrantExchangeName, ExchangeType.Direct);
            var queueName = $"{_philosopherId}_grant_queue";
            channel.QueueDeclare(queueName, durable: false, exclusive: true, autoDelete: true);
            channel.QueueBind(queueName, GrantExchangeName, routingKey: _philosopherId);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += (sender, args) =>
            {
                var grantEvent = JsonSerializer.Deserialize<PermissionGrantedEvent>(Encoding.UTF8.GetString(args.Body.ToArray()));
                if (grantEvent?.PhilosopherId == _philosopherId)
                {
                    _logger.LogInformation("{Name} received grant event.", _philosopherName);
                    _permissionGrantedTcs.TrySetResult(true);
                }
                return Task.CompletedTask;
            };
            channel.BasicConsume(queueName, autoAck: true, consumer: consumer);
        }

        private void PublishEvent<T>(string routingKey, T data)
        {
            using var channel = _rabbitConnection.CreateModel();
            var message = JsonSerializer.Serialize(data);
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "", routingKey: routingKey, body: body);
        }

        private async Task<bool> TryTakeFork(int forkId, CancellationToken token)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/forks/{forkId}/take", new TakeForkRequest { PhilosopherId = _philosopherId }, token);
            return response.IsSuccessStatusCode;
        }

        private async Task ReleaseFork(int forkId, CancellationToken token)
        {
            await _httpClient.PostAsync($"api/forks/{forkId}/release", null, token);
        }

        private async Task ReportStateChange(string newState, CancellationToken token)
        {
            _logger.LogInformation("{Name} is now {State}", _philosopherName, newState);
            var request = new StateChangeRequest { PhilosopherId = _philosopherId, NewState = newState };
            await _httpClient.PostAsJsonAsync("api/metrics/statechange", request, token);
        }
    }
}
