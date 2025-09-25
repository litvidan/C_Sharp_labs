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

        private readonly ICoordinator? _coordinator;
        private readonly IMonitor _monitor;

        public Table(IMonitor monitor, int philosopherCount = 5, string? namesFilePath = null, IForkStrategy? forkStrategy = null, ICoordinator? coordinator = null)
        {
            if (philosopherCount < 2)
                throw new ArgumentException("Must have at least 2 philosophers", nameof(philosopherCount));

            _philosopherCount = philosopherCount;
            _philosophers = new List<Philosopher>();
            _forks = new List<Fork>();
            _currentStep = 0;
            _coordinator = coordinator;
            _monitor = monitor;

            InitializeTable(namesFilePath, forkStrategy ?? new OrderedForkStrategy());
        }

        public int PhilosopherCount => _philosopherCount;
        public IReadOnlyList<Philosopher> Philosophers => _philosophers.AsReadOnly();
        public IReadOnlyList<Fork> Forks => _forks.AsReadOnly();
        public int CurrentStep => _currentStep;

        /// <summary>
        /// Initializes the table with philosophers and forks.
        /// </summary>
        private void InitializeTable(string? namesFilePath = null, IForkStrategy? forkStrategy = null)
        {
            _monitor.printTableSetup(_philosopherCount);
            // Read philosopher names from file if provided
            var philosopherNames = ReadPhilosopherNames(namesFilePath);

            // Create forks
            for (int i = 0; i < _philosopherCount; i++)
            {
                _forks.Add(new Fork(i, _monitor));
            }

            // Create philosophers and assign forks
            for (int i = 0; i < _philosopherCount; i++)
            {
                Fork leftFork = _forks[i];
                Fork rightFork = _forks[(i + 1) % _philosopherCount]; // Circular arrangement
                
                string philosopherName = philosopherNames[i];
                var philosopher = new Philosopher(i, philosopherName, leftFork, rightFork, forkStrategy ?? new OrderedForkStrategy());
                _philosophers.Add(philosopher);
                _monitor.printSitBetween(philosopherName, i, PhilosopherCount);
            }
            _monitor.printTableSetupComplete();
        }

        /// <summary>
        /// Reads philosopher names from a file or generates default names.
        /// </summary>
        /// <param name="namesFilePath">Path to the file containing philosopher names</param>
        /// <returns>List of philosopher names</returns>
        private List<string> ReadPhilosopherNames(string? namesFilePath)
        {
            var names = new List<string>();
            
            if (!string.IsNullOrEmpty(namesFilePath) && File.Exists(namesFilePath))
            {
                try
                {
                    var lines = File.ReadAllLines(namesFilePath);
                    foreach (var line in lines)
                    {
                        var trimmedLine = line.Trim();
                        if (!string.IsNullOrEmpty(trimmedLine))
                        {
                            names.Add(trimmedLine);
                        }
                    }
                    _monitor.printNamesLoadingSuccess(philosopherCount: names.Count, fileName: namesFilePath);
                }
                catch (Exception ex)
                {
                    _monitor.printNamesLoadingError(ex: ex, fileName: namesFilePath);
                    names.Clear();
                }
            }
            
            // If we don't have enough names from file, generate default names
            while (names.Count < _philosopherCount)
            {
                names.Add($"Philosopher {names.Count}");
            }
            
            // If we have more names than needed, take only what we need
            if (names.Count > _philosopherCount)
            {
                names = names.Take(_philosopherCount).ToList();
            }
            
            return names;
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
    }
}
