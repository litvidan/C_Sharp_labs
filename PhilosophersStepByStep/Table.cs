using PhilosophersStepByStep.Strategies;
using System.Diagnostics;

namespace PhilosophersStepByStep
{
    /// <summary>
    /// Represents the dining table where philosophers sit and share forks.
    /// Manages the single-threaded step-by-step simulation.
    /// </summary>
    public class Table
    {
        private readonly List<Philosopher> _philosophers;
        private readonly List<Fork> _forks;
        private readonly int _philosopherCount;
        private int _currentStep;
        private readonly MetricsCalculator _metricsCalculator;

        private readonly IMonitor _monitor;
        private readonly int _simulationDuration;

        public Table(IMonitor monitor, PhilosopherConfiguration config, IForkStrategy? forkStrategy = null, int simulationDuration = 10000)
        {
            if (config.PhilosopherCount < 2)
                throw new ArgumentException("Must have at least 2 philosophers", nameof(config.PhilosopherCount));

            _philosopherCount = config.PhilosopherCount;
            _philosophers = new List<Philosopher>();
            _forks = new List<Fork>();
            _currentStep = 0;
            _monitor = monitor;
            _simulationDuration = simulationDuration;
            InitializeTable(config.PhilosopherNames, forkStrategy ?? new OrderedForkStrategy());
            _metricsCalculator = new MetricsCalculator(this);
        }

        public int PhilosopherCount => _philosopherCount;
        public IReadOnlyList<Philosopher> Philosophers => _philosophers.AsReadOnly();
        public IReadOnlyList<Fork> Forks => _forks.AsReadOnly();
        public int CurrentStep => _currentStep;

        /// <summary>
        /// Initializes the table with philosophers and forks.
        /// </summary>
        private void InitializeTable(List<string> philosopherNames, IForkStrategy? forkStrategy = null)
        {
            _monitor.PrintTableSetup(_philosopherCount);

            // Create forks
            for (int i = 0; i < _philosopherCount; i++)
            {
                _forks.Add(new Fork(i, _monitor));
            }

            // Create philosophers, assign forks and register
            for (int i = 0; i < _philosopherCount; i++)
            {
                Fork leftFork = _forks[i];
                Fork rightFork = _forks[(i + 1) % _philosopherCount]; // Circular arrangement
                

                string philosopherName = philosopherNames[i];
                var philosopher = new Philosopher(i, philosopherName, leftFork, rightFork, forkStrategy ?? new OrderedForkStrategy());
                _philosophers.Add(philosopher);
                _monitor.PrintSitBetween(philosopherName, i, PhilosopherCount);
            }
            _monitor.PrintTableSetupComplete();
        }


        /// <summary>
        /// Resets the simulation to initial state.
        /// </summary>
        public void Reset()
        {
            // Reset all philosophers
            foreach (var philosopher in _philosophers)
            {
                philosopher.Reset();
            }

            // Reset all forks
            foreach (var fork in _forks)
            {
                fork.ForceRelease();
            }

            _currentStep = 0;
        }

        /// <summary>
        /// Gets a summary of the current simulation state.
        /// </summary>
        public string GetSimulationSummary()
        {
            var thinkingCount = _philosophers.Count(p => p.State == PhilosopherState.Thinking);
            var hungryCount = _philosophers.Count(p => p.State == PhilosopherState.Hungry);
            var eatingCount = _philosophers.Count(p => p.State == PhilosopherState.Eating);
            var availableForks = _forks.Count(f => f.IsAvailable);

            return $"Step {_currentStep}: {thinkingCount} thinking, {hungryCount} hungry, {eatingCount} eating, {availableForks} forks available";
        }

        public void PrintFinalMetrics()
        {
            var metrics = _metricsCalculator._totalMetrics;
            _monitor.PrintMetrics(metrics);
        }
    
    }
}
