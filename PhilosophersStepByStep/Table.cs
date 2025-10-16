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

        // Metrics
        private int _throughput;
        private int _avgEaten;
        private int _avgWaitingTime;
        private int _utilisationCoeff;
        private Dictionary<Philosopher, int> _eatenCountsPerStep = new();

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

            _throughput = 0;
            _avgEaten = 0;
            _avgWaitingTime = 0;
            _utilisationCoeff = 0;


            InitializeTable(config.PhilosopherNames, forkStrategy ?? new OrderedForkStrategy());
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
            _monitor.printTableSetup(_philosopherCount);

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
                _monitor.printSitBetween(philosopherName, i, PhilosopherCount);
            }
            _monitor.printTableSetupComplete();
        }

        /// <summary>
        /// Executes one step of the simulation.
        /// </summary>
        /// <returns>True if any philosopher performed an action</returns>
        public bool ExecuteStep()
        {
            _currentStep++;
            _monitor.printCurrentStepStatus(_currentStep, _philosophers, _forks);

            bool anyAction = false;

            // Execute one step for each philosopher
            foreach (var philosopher in _philosophers)
            {
                bool actionPerformed = philosopher.ExecuteStep();
                if (actionPerformed)
                {
                    anyAction = true;
                }
            }

            return anyAction;
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

        public void CalculateMetrics()
        {
            int eaten = 0;
            int eatenPerTick = 0;
            int sumEaten = 0;
            int[] eatenThisStep = new int[_philosophers.Count];

            // Calculate throughput
            // количество съеденного в единицу времени, по каждому философу и среднее.
            foreach (var phil in _philosophers)
            {
                eaten = phil.EatCount;
                sumEaten += eaten;

                // Доделать
                // eatenPerTick = eaten - _eatenPrevStep[]
            }

            _avgEaten = sumEaten / _philosopherCount;


        }
    }
}
