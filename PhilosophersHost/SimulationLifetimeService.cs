using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhilosophersHost.Configuration;

namespace PhilosophersHost.HostedServices
{
    public class SimulationLifetimeService : IHostedService
    {
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly ILogger<SimulationLifetimeService> _logger;
        private readonly SimulationOptions _options;
        private Timer? _timer;

        public SimulationLifetimeService(
            IHostApplicationLifetime appLifetime,
            ILogger<SimulationLifetimeService> logger,
            IOptions<SimulationOptions> options)
        {
            _appLifetime = appLifetime;
            _logger = logger;
            _options = options.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Симуляция запущена на {Duration} секунд.", _options.DurationSeconds);

            // Устанавливаем таймер на общее время симуляции
            _timer = new Timer(
                state => StopHost(),
                null,
                TimeSpan.FromSeconds(_options.DurationSeconds),
                Timeout.InfiniteTimeSpan // Не повторять
            );

            return Task.CompletedTask;
        }

        private void StopHost()
        {
            _logger.LogWarning("Время симуляции ({Duration} с) истекло. Завершение работы хоста...", _options.DurationSeconds);
            // Останавливаем хост, что приведет к отмене CancellationToken у всех BackgroundService
            _appLifetime.StopApplication(); 
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            _logger.LogInformation("Служба контроля жизненного цикла завершена.");
            return Task.CompletedTask;
        }
    }
}