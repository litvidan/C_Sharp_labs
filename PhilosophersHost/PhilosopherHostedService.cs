using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhilosophersHost.Configuration;
using PhilosophersHost.Services;
using PhilosophersHost.Services.Metrics;
using PhilosophersHost.Strategies;

namespace PhilosophersHost.HostedServices
{
    static class EventIds
    {
        public static readonly EventId Thinking = new EventId(1, "ThinkingStarted");
        public static readonly EventId Hungry = new EventId(2, "EatingStarted");
        public static readonly EventId Eating = new EventId(3, "Eating");
        public static readonly EventId Finished = new EventId(4, "Finished");
        public static readonly EventId ForksAcquireFailed = new EventId(5, "ForksAcquireFailed");
        public static readonly EventId Start = new EventId(6, "Started");
        public static readonly EventId Stopping = new EventId(7, "Stopping");
        public static readonly EventId Stopped = new EventId(8, "Stopped");
    }

    public class PhilosopherHostedService : BackgroundService
    {
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly ILogger<PhilosopherHostedService> _logger;
        private readonly IPhilosopherStrategy _strategy;
        private readonly SimulationOptions _options;
        private readonly IMetricsCollector _metrics;
        private readonly string _name;
        private readonly int _id;
        private readonly int _leftForkId;
        private readonly int _rightForkId;
        private readonly Random _random = new();

        public PhilosopherHostedService(
            IHostApplicationLifetime appLifetime,
            ILogger<PhilosopherHostedService> logger,
            IForkManager forkManager,
            IOptions<SimulationOptions> simulationOptions,
            IOptions<PhilosopherIdentityOptions> identityOptions,
            IPhilosopherStrategy strategy,
            IMetricsCollector metrics) 
        {
            _appLifetime = appLifetime;
            _logger = logger;
            _strategy = strategy;
            _options = simulationOptions.Value;
            _metrics = metrics;

            _name = identityOptions.Value.Name;
            _id = identityOptions.Value.Id;
            _leftForkId = identityOptions.Value.LeftForkId;
            _rightForkId = identityOptions.Value.RightForkId;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);            

            await Task.Yield();
            
            while (!stoppingToken.IsCancellationRequested)
            {
                await Think(stoppingToken);
                await Eat(stoppingToken);
            }
        }

        private async Task Think(CancellationToken stoppingToken)
        {
            int time = _random.Next(_options.ThinkingTimeMin, _options.ThinkingTimeMax);
            _logger.LogInformation(EventIds.Thinking, "🧠 {Name} думает в течение {Time} мс...", _name, time);

            var sw = Stopwatch.StartNew();

            try
            {
                await Task.Delay(time, stoppingToken);
            }
            catch (TaskCanceledException) { }
            finally 
            {
                sw.Stop();
                _metrics.RecordPhilosopherTime(_id, _name, PhilosopherState.Thinking, sw.Elapsed.TotalMilliseconds);
            }
        }

        private async Task Eat(CancellationToken stoppingToken)
        {
            _logger.LogInformation(EventIds.Hungry, "🍴 {Name} очень голоден и пытается взять вилки ({Left}) и ({Right})...", _name, _leftForkId, _rightForkId);

            var sw  = Stopwatch.StartNew();

            bool success = await _strategy.TryEatAsync(
                _id,
                _leftForkId, 
                _rightForkId, 
                stoppingToken);

            _metrics.RecordPhilosopherTime(_id, _name, PhilosopherState.Waiting, sw.Elapsed.TotalMilliseconds);

            if (success)
            {
                int time = _random.Next(_options.EatingTimeMin, _options.EatingTimeMax);
                _logger.LogInformation(EventIds.Eating, "😋 {Name} начал есть, используя вилки ({Left}) и ({Right}), в течение {Time} мс...", _name, _leftForkId, _rightForkId, time);
                
                sw.Restart();
                try { await Task.Delay(time, stoppingToken); }
                catch (TaskCanceledException) { }
                finally
                {
                    _metrics.RecordPhilosopherTime(_id, _name, PhilosopherState.Eating, sw.Elapsed.TotalMilliseconds);
                    _strategy.StopEating(_leftForkId, _rightForkId);
                    _logger.LogInformation(EventIds.Finished, "🏁 {Name} закончил есть и отпустил вилки.", _name);
                }              
            }
            else
            {
                _logger.LogWarning(EventIds.ForksAcquireFailed, "😒 {Name} не смог взять обе вилки и отступил.", _name);
                
                sw.Restart(); 
                        
                try
                {
                    await Task.Delay(_options.ForkAcquisitionTime, stoppingToken);
                }
                catch (TaskCanceledException) { }
                finally
                {
                    _metrics.RecordPhilosopherTime(_id, _name, PhilosopherState.Waiting, sw.Elapsed.TotalMilliseconds);
                }
            }
        }

        private void OnStarted()
        {
            _logger.LogInformation(EventIds.Start, "👴 {Name} (ID: {Id}) начинает жить.", _name, _id);
        }

        private void OnStopping()
        {
            _logger.LogInformation(EventIds.Stopping, "🛑 {Name} (ID: {Id}) попросили покинуть стол.", _name, _id);
        }

        private void OnStopped()
        {
            _logger.LogInformation(EventIds.Stopped, "✅ {Name} (ID: {Id}) покинул стол.", _name, _id);
        }
    }
}