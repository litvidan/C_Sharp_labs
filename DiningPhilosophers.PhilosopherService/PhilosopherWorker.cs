using DiningPhilosophers.Contracts;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Json;

namespace DiningPhilosophers.PhilosopherService
{
    public class PhilosopherWorker : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PhilosopherWorker> _logger;
        private readonly string _philosopherName;
        private readonly string _philosopherId;
        private readonly int _leftForkId;
        private readonly int _rightForkId;
        private readonly Uri _tableServiceBaseUrl;
        private readonly TimeSpan _simulationDuration;
        private readonly Random _random = new Random();
        private HttpClient _httpClient = null!;

        public PhilosopherWorker(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<PhilosopherWorker> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            _philosopherName = configuration["PHILOSOPHER_NAME"] ?? "Unknown";
            _philosopherId = configuration["PHILOSOPHER_ID"] ?? Guid.NewGuid().ToString();
            _leftForkId = int.Parse(configuration["LEFT_FORK_ID"] ?? "0");
            _rightForkId = int.Parse(configuration["RIGHT_FORK_ID"] ?? "1");
            _tableServiceBaseUrl = new Uri(configuration["TABLE_SERVICE_URL"] ?? "http://localhost:8080");
            _simulationDuration = TimeSpan.FromMinutes(double.Parse(configuration["SIMULATION_DURATION_MINUTES"] ?? "1"));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var startTime = DateTime.UtcNow;
            _httpClient = _httpClientFactory.CreateClient();
            _httpClient.BaseAddress = _tableServiceBaseUrl;

            // Report initial state
            await ReportStateChange("Thinking", stoppingToken);

            while (!stoppingToken.IsCancellationRequested && (DateTime.UtcNow - startTime) < _simulationDuration)
            {
                // 1. Thinking
                _logger.LogInformation("{Name} is thinking.", _philosopherName);
                await Task.Delay(_random.Next(2000, 5000), stoppingToken);

                // 2. Hungry
                await ReportStateChange("Hungry", stoppingToken);
                bool hasBothForks = false;
                while (!hasBothForks && !stoppingToken.IsCancellationRequested)
                {
                    hasBothForks = await TryAcquireForksStrategically(stoppingToken);
                    if (!hasBothForks)
                    {
                        await Task.Delay(_random.Next(200, 700), stoppingToken);
                    }
                }

                if (stoppingToken.IsCancellationRequested) break;

                // 3. Eating
                await ReportStateChange("Eating", stoppingToken);
                await Task.Delay(_random.Next(2000, 5000), stoppingToken);

                // 4. Release forks and transition back to thinking
                await ReleaseBothForks(stoppingToken);
                await ReportStateChange("Thinking", stoppingToken);
            }

            _logger.LogInformation("{Name} has finished.", _philosopherName);
            await _httpClient.PostAsJsonAsync("api/metrics/finish", new TakeForkRequest { PhilosopherId = _philosopherId }, stoppingToken);
        }

        private async Task<bool> TryAcquireForksStrategically(CancellationToken token)
        {
            int firstForkId = Math.Min(_leftForkId, _rightForkId);
            int secondForkId = Math.Max(_leftForkId, _rightForkId);

            _logger.LogInformation("{Name} trying to take first fork {ForkId}.", _philosopherName, firstForkId);
            if (await TryTakeFork(firstForkId, token))
            {
                _logger.LogInformation("{Name} trying to take second fork {ForkId}.", _philosopherName, secondForkId);
                if (await TryTakeFork(secondForkId, token))
                {
                    return true;
                }
                else
                {
                    _logger.LogWarning("{Name} failed to get second fork {SecondForkId}, releasing first fork {FirstForkId}.", _philosopherName, secondForkId, firstForkId);
                    await ReleaseFork(firstForkId, token);
                    return false;
                }
            }
            _logger.LogInformation("{Name} failed to get first fork {ForkId}.", _philosopherName, firstForkId);
            return false;
        }

        private async Task<bool> TryTakeFork(int forkId, CancellationToken token)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/forks/{forkId}/take", new TakeForkRequest { PhilosopherId = _philosopherId }, token);
            return response.IsSuccessStatusCode;
        }

        private async Task ReleaseFork(int forkId, CancellationToken token)
        {
            await _httpClient.PostAsync($"api/forks/{forkId}/release", null, token);
            _logger.LogInformation("{Name} released fork {ForkId}.", _philosopherName, forkId);
        }

        private async Task ReleaseBothForks(CancellationToken token)
        {
            await ReleaseFork(_leftForkId, token);
            await ReleaseFork(_rightForkId, token);
        }

        private async Task ReportStateChange(string newState, CancellationToken token)
        {
            _logger.LogInformation("{Name} is now {State}", _philosopherName, newState);
            var request = new StateChangeRequest { PhilosopherId = _philosopherId, NewState = newState };
            await _httpClient.PostAsJsonAsync("api/metrics/statechange", request, token);
        }
    }
}
