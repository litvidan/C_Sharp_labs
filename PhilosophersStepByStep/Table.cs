using PhilosophersStepByStep.Strategies;

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

        private readonly ICoordinator? _coordinator;
        private readonly IMonitor _monitor;

        public Table(IMonitor monitor, PhilosopherConfiguration config, IForkStrategy? forkStrategy = null, ICoordinator? coordinator = null)
        {
            if (config.PhilosopherCount < 2)
                throw new ArgumentException("Must have at least 2 philosophers", nameof(config.PhilosopherCount));

            _philosopherCount = config.PhilosopherCount;
            _philosophers = new List<Philosopher>();
            _forks = new List<Fork>();
            _currentStep = 0;
            _coordinator = coordinator;
            _monitor = monitor;
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

            // Create philosophers, assign forks and register in coordinator
            for (int i = 0; i < _philosopherCount; i++)
            {
                Fork leftFork = _forks[i];
                Fork rightFork = _forks[(i + 1) % _philosopherCount]; // Circular arrangement
                
                _coordinator?.RegisterFork(leftFork.Id);
                _coordinator?.RegisterFork(rightFork.Id);

                string philosopherName = philosopherNames[i];
                var philosopher = new Philosopher(i, philosopherName, leftFork, rightFork, forkStrategy ?? new OrderedForkStrategy(), _coordinator);
                _philosophers.Add(philosopher);
                _coordinator?.RegisterPhilosopher(philosopher.Id);
                _monitor.PrintSitBetween(philosopherName, i, PhilosopherCount);
            }
            _monitor.PrintTableSetupComplete();
        }

        /// <summary>
        /// Executes one step of the simulation.
        /// </summary>
        /// <returns>True if any philosopher performed an action</returns>
        public void ExecuteStep(bool printSteps = false)
        {
            _currentStep++;

            foreach (var fork in _forks)
            {
                // Checking if the philosopher is using the fork to eat
                var isUsedToEat = fork.State == ForkState.InUse ? fork.Holder?.State == PhilosopherState.Eating : false;
                fork.UpdateMetrics(isUsedToEat);
            }

            if(printSteps) _monitor.PrintCurrentStepStatus(_currentStep, _philosophers, _forks, _metricsCalculator);

            // Execute one step for each philosopher
            foreach (var philosopher in _philosophers)
            {
                philosopher.ExecuteStep();
            }

            _metricsCalculator.UpdateMetrics();
            if (_metricsCalculator.DetectDeadlock())
            {
                _monitor.PrintDeadlockDetected();
                _monitor.PrintMetrics(_metricsCalculator._currentMetrics);
                return;
            }

            if (_currentStep % 1000 == 0)
            {
                _monitor.PrintMetrics(_metricsCalculator._currentMetrics);
                _metricsCalculator.ResetCurrentMetrics();
            }
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
