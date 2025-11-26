using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhilosophersHost.Configuration;
using PhilosophersHost.Services;

namespace PhilosophersHost.HostedServices
{
    public class PhilosopherHostedService : BackgroundService
    {
        private readonly ILogger<PhilosopherHostedService> _logger;
        private readonly IForkManager _forkManager;
        private readonly SimulationOptions _options;
        private readonly string _name;
        private readonly int _id;
        private readonly int _leftForkId;
        private readonly int _rightForkId;
        private readonly Random _random = new();

        public PhilosopherHostedService(
            ILogger<PhilosopherHostedService> logger,
            IForkManager forkManager,
            IOptions<SimulationOptions> simulationOptions,
            IOptions<PhilosopherIdentityOptions> identityOptions)
        {
            _logger = logger;
            _forkManager = forkManager;
            _options = simulationOptions.Value;
            _name = identityOptions.Value.Name;
            _id = identityOptions.Value.Id;
            _leftForkId = identityOptions.Value.LeftForkId;
            _rightForkId = identityOptions.Value.RightForkId;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {            
            _logger.LogInformation("{Name} (ID: {Id}) начинает жить.", _name, _id);

            await Task.Yield();

            while (!stoppingToken.IsCancellationRequested)
            {
                // 1. Думает
                await Think(stoppingToken);

                // 2. Пытается есть
                await Eat(stoppingToken);
            }

            _logger.LogInformation("{Name} (ID: {Id}) завершает работу.", _name, _id);
        }

        private async Task Think(CancellationToken stoppingToken)
        {
            int time = _random.Next(_options.ThinkingTimeMin, _options.ThinkingTimeMax);
            _logger.LogInformation("{Name} думает в течение {Time} мс...", _name, time);

            try
            {
                await Task.Delay(time, stoppingToken);
            }
            catch (TaskCanceledException) { }
        }

        private async Task Eat(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{Name} очень голоден и пытается взять вилки ({Left}) и ({Right})...", _name, _leftForkId, _rightForkId);

            // Блокирующий вызов (для примера): в реальном коде нужно использовать TryAcquire с таймаутом и отменой
            if (_forkManager.TryAcquireForks(_id, _leftForkId, _rightForkId, stoppingToken))
            {
                int time = _random.Next(_options.EatingTimeMin, _options.EatingTimeMax);
                _logger.LogInformation("🍴 {Name} начал есть, используя вилки ({Left}) и ({Right}), в течение {Time} мс...", _name, _leftForkId, _rightForkId, time);
                
                try
                {
                    await Task.Delay(time, stoppingToken);
                }
                catch (TaskCanceledException) { } // Прерывание во время еды

                _forkManager.ReleaseForks(_leftForkId, _rightForkId);
                _logger.LogInformation("🍽️ {Name} закончил есть и отпустил вилки.", _name);
            }
            else
            {
                _logger.LogWarning("{Name} не смог взять обе вилки и отступил.", _name);
                // Ждем немного, чтобы не сразу пытаться снова
                try
                {
                    await Task.Delay(_options.ForkAcquisitionTime, stoppingToken);
                }
                catch (TaskCanceledException) { }
            }
        }
    }
}