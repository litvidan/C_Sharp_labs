using System.Diagnostics;

namespace PhilosophersStepByStep
{
    /// <summary>
    /// Calculates and accumulates simulation metrics for the philosophers and forks.  
    /// Subscribes to state changes and records events for further statistical analysis.
    /// </summary>
    public class MetricsCalculator
    {
        private readonly Table _table;

        public SimulationMetrics _totalMetrics;

        private DateTime simulationStartTime;

        /// <summary>
        /// Initializes a new instance of the MetricsCalculator class.
        /// Sets up tracking for all philosophers and forks on the table and subscribes to their state changes.
        /// </summary>
        /// <param name="table">Table instance containing philosophers and forks.</param>
        public MetricsCalculator(Table table)
        {
            _table = table;
            _totalMetrics = new SimulationMetrics();

            simulationStartTime = DateTime.UtcNow;

            foreach (var philosopher in table.Philosophers)
            {
                _totalMetrics.PhilosopherEatDuration[philosopher.Name] = 0;
                _totalMetrics.PhilosopherHungryDuration[philosopher.Name] = 0;

                _totalMetrics.philosopherStateStartTimes[philosopher.Name] = DateTime.UtcNow;
                _totalMetrics.PhilosopherHungryDuration[philosopher.Name] = 0;

                philosopher.StateChanged += Philosopher_StateChanged;
            }

            foreach (var fork in _table.Forks)
            {
                _totalMetrics.forkStateStartTimes[fork.Id] = DateTime.UtcNow;
                _totalMetrics.forkInUseDurationMs[fork.Id] = 0;

                fork.StateChanged += Fork_StateChanged;
            }
        }

        /// <summary>
        /// Handles philosopher state change events.
        /// Updates metrics such as hungry and eating durations based on state transitions.
        /// </summary>
        private void Philosopher_StateChanged(object sender, PhilosopherStateChangeEventArgs e)
        {
            var philosopherStateStartTimes = _totalMetrics.philosopherStateStartTimes;
            var philosopher = (Philosopher)sender;
            if (!philosopherStateStartTimes.ContainsKey(philosopher.Name))
                philosopherStateStartTimes[philosopher.Name] = e.Timestamp;

            var duration = (e.Timestamp - philosopherStateStartTimes[philosopher.Name]).TotalMilliseconds;

            if (e.PreviousState == PhilosopherState.Hungry)
            {
                _totalMetrics.PhilosopherHungryDuration[philosopher.Name] += (long)duration;
                _totalMetrics.PhilosopherHungryDuration[philosopher.Name] += (long)duration;
            }

            if (e.PreviousState == PhilosopherState.Eating)
            {
                _totalMetrics.PhilosopherEatDuration[philosopher.Name] += (long)duration;
            }

            _totalMetrics.philosopherStateStartTimes[philosopher.Name] = e.Timestamp;
        }

        /// <summary>
        /// Handles fork state change events.
        /// Tracks and accumulates fork usage durations when the state changes to or from InUse.
        /// </summary>
        private void Fork_StateChanged(object sender, ForkStateChangeEventArgs e)
        {
            var fork = (Fork)sender;
            if (!_totalMetrics.forkStateStartTimes.ContainsKey(fork.Id))
                _totalMetrics.forkStateStartTimes[fork.Id] = e.Timestamp;

            var duration = (e.Timestamp - _totalMetrics.forkStateStartTimes[fork.Id]).TotalMilliseconds;

            switch (e.PreviousState)
            {
                case ForkState.InUse:
                    _totalMetrics.forkInUseDurationMs[fork.Id] += (long)duration;
                    break;
            }

            _totalMetrics.forkStateStartTimes[fork.Id] = e.Timestamp;
        }

        /// <summary>
        /// Calculates and finalizes all simulation metrics.
        /// Computes average eating throughput, average hungry time, max hungry time, and fork utilization.
        /// </summary>
        public void CalculateFinalMetrics()
        {
            var totalSimulationTimeMs = (DateTime.UtcNow - simulationStartTime).TotalMilliseconds;

            foreach (var philosopher in _table.Philosophers)
            {
                var eatDuration = _totalMetrics.PhilosopherEatDuration[philosopher.Name];
                _totalMetrics.PhilosopherEatThroughput[philosopher.Name] = (double)eatDuration / totalSimulationTimeMs;
            }
            _totalMetrics.AverageEatThroughput = _totalMetrics.PhilosopherEatThroughput.Values.Average();

            double totalHungryTime = _totalMetrics.PhilosopherHungryDuration.Values.Sum();
            _totalMetrics.AverageHungryTimeMs = totalHungryTime / _table.Philosophers.Count;

            var maxHungry = _totalMetrics.PhilosopherHungryDuration.OrderByDescending(kvp => kvp.Value).First();
            _totalMetrics.MaxHungryTimeMs = maxHungry.Value;
            _totalMetrics.MaxHungryPhilosopher = maxHungry.Key;

            foreach (var fork in _table.Forks)
            {
                long inUseTime = _totalMetrics.forkInUseDurationMs[fork.Id];
                double utilization = 100.0 * inUseTime / totalSimulationTimeMs;
                _totalMetrics.ForkUtilizationPercent[fork.Id] = utilization;
            }
        }

        /// <summary>
        /// Detects deadlock state in the current simulation.
        /// Returns true if all philosophers are hungry and each is holding a fork.
        /// </summary>
        /// <returns>True if a deadlock is detected, otherwise false.</returns>
        public bool DetectDeadlock()
        {
            // Simple deadlock detection: if all philosophers are hungry and holding one fork, it's a deadlock
            return _table.Philosophers.All(p => p.State == PhilosopherState.Hungry && (p.HeldFork1 != null || p.HeldFork2 != null));
        }
    }
    
    /// <summary>
    /// Stores simulation metrics for philosophers and forks, as well as aggregate statistics.
    /// </summary>
    public class SimulationMetrics
    {
        public Stopwatch duration { get; set; } = Stopwatch.StartNew();


        public Dictionary<string, DateTime> philosopherStateStartTimes = new();
        public Dictionary<string, long> PhilosopherEatDuration { get; set; } = new();
        public Dictionary<string, long> PhilosopherHungryDuration { get; set; } = new();
        public Dictionary<string, double> PhilosopherEatThroughput { get; set; } = new();

        public double AverageEatThroughput { get; set; } = 0.0;
        public double AverageHungryTimeMs { get; set; } = 0.0;
        public double MaxHungryTimeMs { get; set; } = 0.0;
        public string MaxHungryPhilosopher { get; set; } = "";

        public Dictionary<int, long> forkInUseDurationMs = new();
        public Dictionary<int, DateTime> forkStateStartTimes = new();
        public Dictionary<int, double> ForkUtilizationPercent { get; set; } = new();
    }

}