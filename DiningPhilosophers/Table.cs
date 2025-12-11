using PhilosophersStepByStep.Strategies;
using System.Diagnostics;

namespace PhilosophersStepByStep
{
    public class Table
    {
        private readonly List<Philosopher> _philosophers;
        private readonly List<Fork> _forks;
        private readonly int _philosopherCount;
        private readonly MetricsCalculator _metricsCalculator;

        private readonly IMonitor _monitor;
        private readonly int _simulationDuration;

        public event EventHandler? StateChanged;

        public Table(IMonitor monitor, PhilosopherConfiguration config, IForkStrategy? forkStrategy = null, int simulationDuration = 10000)
        {
            if (config.PhilosopherCount < 2)
                throw new ArgumentException("Must have at least 2 philosophers", nameof(config.PhilosopherCount));

            _philosopherCount = config.PhilosopherCount;
            _philosophers = new List<Philosopher>();
            _forks = new List<Fork>();
            _monitor = monitor;
            _simulationDuration = simulationDuration;
            InitializeTable(config.PhilosopherNames, forkStrategy ?? new OrderedForkStrategy());
            _metricsCalculator = new MetricsCalculator(this);
        }

        public int PhilosopherCount => _philosopherCount;
        public IReadOnlyList<Philosopher> Philosophers => _philosophers.AsReadOnly();
        public IReadOnlyList<Fork> Forks => _forks.AsReadOnly();

        private void InitializeTable(List<string> philosopherNames, IForkStrategy? forkStrategy = null)
        {
            _monitor.PrintTableSetup(_philosopherCount);

            for (int i = 0; i < _philosopherCount; i++)
            {
                var fork = new Fork(i, _monitor);
                fork.StateChanged += OnComponentStateChanged;
                _forks.Add(fork);
            }

            for (int i = 0; i < _philosopherCount; i++)
            {
                Fork leftFork = _forks[i];
                Fork rightFork = _forks[(i + 1) % _philosopherCount];

                string philosopherName = philosopherNames[i];
                var philosopher = new Philosopher(i, philosopherName, leftFork, rightFork, forkStrategy ?? new OrderedForkStrategy());
                philosopher.StateChanged += OnComponentStateChanged;
                _philosophers.Add(philosopher);
                _monitor.PrintSitBetween(philosopherName, i, PhilosopherCount);
            }
            _monitor.PrintTableSetupComplete();
        }

        private void OnComponentStateChanged(object? sender, EventArgs e)
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Reset()
        {
            foreach (var philosopher in _philosophers)
            {
                philosopher.Reset();
            }
            foreach (var fork in _forks)
            {
                fork.ForceRelease();
            }
        }

        public string GetSimulationSummary()
        {
            var thinkingCount = _philosophers.Count(p => p.State == PhilosopherState.Thinking);
            var hungryCount = _philosophers.Count(p => p.State == PhilosopherState.Hungry);
            var eatingCount = _philosophers.Count(p => p.State == PhilosopherState.Eating);
            var availableForks = _forks.Count(f => f.IsAvailable);

            return $"{thinkingCount} thinking, {hungryCount} hungry, {eatingCount} eating, {availableForks} forks available";
        }

        public void PrintFinalMetrics()
        {
            _metricsCalculator.CalculateFinalMetrics();
            _monitor.PrintMetrics(_metricsCalculator._totalMetrics);
        }
    }
}
