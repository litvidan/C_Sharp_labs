namespace PhilosophersStepByStep
{
    public class MetricsCalculator
    {
        private readonly Table _table;

        public SimulationMetrics _totalMetrics; // Total metrics across all steps
        public SimulationMetrics _currentMetrics; // Current metrics for the ongoing period


        public MetricsCalculator(Table table)
        {
            _table = table;

            // Initialization of metrics
            _totalMetrics = new SimulationMetrics();
            _currentMetrics = new SimulationMetrics();
            foreach (var philosopher in table.Philosophers)
            {
                _currentMetrics.PhilosopherEatCounts[philosopher.Name] = 0;
                _currentMetrics.PhilosopherHungryCounts[philosopher.Name] = 0;
                _currentMetrics.PhilosopherThoughtCounts[philosopher.Name] = 0;
                _currentMetrics.PhilosopherStreakHungryCounts[philosopher.Name] = 0;
                _currentMetrics.PhilosopherMaxHungryCounts[philosopher.Name] = 0;
                _totalMetrics.PhilosopherEatCounts[philosopher.Name] = 0;
                _totalMetrics.PhilosopherHungryCounts[philosopher.Name] = 0;
                _totalMetrics.PhilosopherThoughtCounts[philosopher.Name] = 0;
                _totalMetrics.PhilosopherStreakHungryCounts[philosopher.Name] = 0;
                _totalMetrics.PhilosopherMaxHungryCounts[philosopher.Name] = 0;
            }

            foreach (var fork in _table.Forks)
            {
                _currentMetrics.ForkMetrics[fork.Id] = new ForkMetrics
                {
                    InUseCounts = 0,
                    BlockedCounts = 0,
                    AvailableCounts = 0
                };
                _totalMetrics.ForkMetrics[fork.Id] = new ForkMetrics
                {
                    InUseCounts = 0,
                    BlockedCounts = 0,
                    AvailableCounts = 0
                };
            }


        }

        public void UpdateMetrics()
        {
            _totalMetrics.Steps++;
            _currentMetrics.Steps++;

            var totalEatenSum = 0.0;
            var currentEatenSum = 0.0;
            var totalHungrySum = 0.0;
            var currentHungrySum = 0.0;
            foreach (var philosopher in _table.Philosophers)
            {
                // Update philosopher eat counts
                if (philosopher.State == PhilosopherState.Eating)
                {
                    _totalMetrics.PhilosopherEatCounts[philosopher.Name] += 1;
                    // Check if we updating max hungry streak
                    var totalPreviousMaxHungry = _totalMetrics.PhilosopherMaxHungryCounts[philosopher.Name];
                    var totalStreakHungry = _totalMetrics.PhilosopherStreakHungryCounts[philosopher.Name];
                    _totalMetrics.PhilosopherMaxHungryCounts[philosopher.Name] = Math.Max(totalPreviousMaxHungry, totalStreakHungry);
                    _totalMetrics.PhilosopherStreakHungryCounts[philosopher.Name] = 0;

                    // Same actions with current metrics
                    _currentMetrics.PhilosopherEatCounts[philosopher.Name] += 1;
                    var currentPreviousMaxHungry = _currentMetrics.PhilosopherMaxHungryCounts[philosopher.Name];
                    var currentStreakHungry = _currentMetrics.PhilosopherStreakHungryCounts[philosopher.Name];
                    _currentMetrics.PhilosopherMaxHungryCounts[philosopher.Name] = Math.Max(currentPreviousMaxHungry, currentStreakHungry);
                    _currentMetrics.PhilosopherStreakHungryCounts[philosopher.Name] = 0;
                }
                totalEatenSum += _totalMetrics.PhilosopherEatCounts[philosopher.Name];
                currentEatenSum += _currentMetrics.PhilosopherEatCounts[philosopher.Name];


                if (philosopher.State == PhilosopherState.Hungry)
                {
                    _totalMetrics.PhilosopherHungryCounts[philosopher.Name]++;
                    _totalMetrics.PhilosopherStreakHungryCounts[philosopher.Name]++;

                    _currentMetrics.PhilosopherHungryCounts[philosopher.Name]++;
                    _currentMetrics.PhilosopherStreakHungryCounts[philosopher.Name]++;
                }
                totalHungrySum += _totalMetrics.PhilosopherHungryCounts[philosopher.Name];
                currentHungrySum += _currentMetrics.PhilosopherHungryCounts[philosopher.Name];
            }
            _totalMetrics.AverageEaten = totalEatenSum / _table.Philosophers.Count;
            _currentMetrics.AverageEaten = currentEatenSum / _table.Philosophers.Count;

            _totalMetrics.AverageHungry = totalHungrySum / _table.Philosophers.Count;
            _currentMetrics.AverageHungry = currentHungrySum / _table.Philosophers.Count;

            // Update forks utilization
            foreach (var fork in _table.Forks)
            {
                switch (fork.State)
                {
                    case ForkState.Available:
                        _currentMetrics.ForkMetrics[fork.Id].AvailableCounts++;
                        _totalMetrics.ForkMetrics[fork.Id].AvailableCounts++;
                        break;
                    case ForkState.InUse:
                        // Check if fork holder uses it
                        var holder = _table.Philosophers.FirstOrDefault(p => p.HeldFork1 == fork || p.HeldFork2 == fork);
                        if (holder != null && holder.State == PhilosopherState.Eating)
                        {
                            _currentMetrics.ForkMetrics[fork.Id].InUseCounts++;
                            _totalMetrics.ForkMetrics[fork.Id].InUseCounts++;
                        }
                        else
                        {
                            _currentMetrics.ForkMetrics[fork.Id].BlockedCounts++;
                            _totalMetrics.ForkMetrics[fork.Id].BlockedCounts++;
                        }
                        break;
                }
            }
        }

        public void ResetCurrentMetrics()
        {
            _currentMetrics = new SimulationMetrics();
            foreach (var philosopher in _table.Philosophers)
            {
                _currentMetrics.PhilosopherEatCounts[philosopher.Name] = 0;
                _currentMetrics.PhilosopherHungryCounts[philosopher.Name] = 0;
                _currentMetrics.PhilosopherThoughtCounts[philosopher.Name] = 0;
                _currentMetrics.PhilosopherStreakHungryCounts[philosopher.Name] = 0;
                _currentMetrics.PhilosopherMaxHungryCounts[philosopher.Name] = 0;
            }
            foreach (var fork in _table.Forks)
            {
                _currentMetrics.ForkMetrics[fork.Id] = new ForkMetrics
                {
                    InUseCounts = 0,
                    BlockedCounts = 0,
                    AvailableCounts = 0
                };
            }

        }

        public bool DetectDeadlock()
        {
            // Simple deadlock detection: if all philosophers are hungry and holding one fork, it's a deadlock
            return _table.Philosophers.All(p => p.State == PhilosopherState.Hungry && (p.HeldFork1 != null || p.HeldFork2 != null));
        }
    }
    
    public class SimulationMetrics
    {
        public int Steps { get; set; } = 0;
        public Dictionary<string, int> PhilosopherEatCounts { get; set; } = new(); // Total eat counts per philosopher
        public Dictionary<string, int> PhilosopherHungryCounts { get; set; } = new(); // Total hungry counts per philosopher
        public Dictionary<string, int> PhilosopherThoughtCounts { get; set; } = new(); // Total thought counts per philosopher
        public Dictionary<string, int> PhilosopherStreakHungryCounts { get; set; } = new(); // Max hungry streak per philosopher
        public Dictionary<string, int> PhilosopherMaxHungryCounts { get; set; } = new(); // Max hungry streak per philosopher
        public double AverageEaten { get; set; } = 0.0;
        public double AverageHungry { get; set; } = 0.0;
        public Dictionary<int, ForkMetrics> ForkMetrics { get; set; } = new();
    }

    public class ForkMetrics
    {
        public int InUseCounts { get; set; }
        public int AvailableCounts { get; set; }
        public int BlockedCounts { get; set; }
    }
}