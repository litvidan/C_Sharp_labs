namespace PhilosophersStepByStep
{
    public class ConsoleMonitor : IMonitor
    {
        public void PrintInitialConfiguration(int philosopherCount, string mode, string? namesFilePath)
        {
            Console.WriteLine("=== Single-Threaded Dining Philosophers Simulation ===\n");
            Console.WriteLine($"Configuration:");
            Console.WriteLine($"- Philosophers: {philosopherCount}");
            Console.WriteLine($"- Mode: {mode}");
            Console.WriteLine($"- Names file: {(string.IsNullOrEmpty(namesFilePath) ? "None (using default names)" : namesFilePath)}");
            Console.WriteLine($"- Available modes: manual, auto\n");
        }

        public void PrintUnknownMode(string mode)
        {
            Console.WriteLine($"Unknown mode '{mode}'. Using manual mode.");
        }

        public void PrintSimRunningExceptionMessage(Exception ex)
        {
            Console.WriteLine($"Error running simulation: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");

        }

        public void PrintManualModePrerequisits()
        {
            Console.WriteLine("=== Manual Mode ===");
            Console.WriteLine("Controls:");
            Console.WriteLine("  Press Enter = execute next step");
            Console.WriteLine("  Type a number = run that many steps automatically");
            Console.WriteLine("  'q' = quit simulation");
            Console.WriteLine("  'r' = reset simulation");
            Console.WriteLine("  's' = show current status");
        }

        public void PrintManualModeRequest()
        {
            Console.Write("Press Enter for next step, or type a number: ");

        }
        public void PrintAutoModePrerequisits()
        {
            Console.WriteLine("=== Auto Mode ===");
            Console.WriteLine("Running simulation automatically...\n");
        }

        public void PrintSimStatus(string simSummary)
        {
            Console.WriteLine($"Current status: {simSummary}");
        }

        public void PrintInvalidOption()
        {
            Console.WriteLine("Invalid option. Press Enter for next step, type a number for multiple steps, or 'q' to quit.");
        }

        public void PrintConfigurationLoaded(int philosopherCount, int namesLoaded, string? source)
        {
            Console.WriteLine($"Configuration loaded: {philosopherCount} philosophers, {namesLoaded} names from {source ?? "default"}");
        }

        // Table class prints
        public void PrintTableSetup(int philosopherCount)
        {
            Console.WriteLine($"Setting up table with {philosopherCount} philosophers and {philosopherCount} forks...");
        }

        public void PrintTableSetupComplete()
        {
            Console.WriteLine("Table setup complete!\n");

        }

        public void PrintSitBetween(string name, int i, int count)
        {
            Console.WriteLine($"{name} sits between fork {i} (left) and fork {(i + 1) % count} (right)");
        }
        public void PrintCurrentStepStatus(int step, List<Philosopher> philosophers, List<Fork> forks, MetricsCalculator metricsCalculator)
        {
            Console.WriteLine($"\n===== STEP {step} =====\n");
            Console.WriteLine("\nPhilosophers: \n");
            foreach (var philosopher in philosophers)
            {
                string status = GetPhilosopherStatusDescription(philosopher);
                Console.WriteLine($"\t{philosopher.Name}: {status} (Eaten: {metricsCalculator._currentMetrics.PhilosopherEatCounts[philosopher.Name]}, Thought: {metricsCalculator._currentMetrics.PhilosopherThoughtCounts[philosopher.Name]})");
            }
            Console.WriteLine("\nForks:");
            foreach (var fork in forks)
            {
                Console.WriteLine($"\tFork {fork.Id}: {fork.GetStatusDescription()}");
            }
        }

        public void PrintNamesLoadingSuccess(int philosopherCount, string fileName)
        {
            Console.WriteLine($"Loaded {philosopherCount} philosopher names from file: {fileName}");
        }

        public void PrintNamesLoadingError(Exception ex, string fileName)
        {
            Console.WriteLine($"Warning: Could not read names from file '{fileName}': {ex.Message}");
            Console.WriteLine("Using default names instead.");
        }

        public void PrintDeadlockDetected()
        {
            Console.WriteLine("Deadlock detected!");
            Console.WriteLine("Deadlock detected!");
            Console.WriteLine("Deadlock detected!");
            Console.WriteLine("Deadlock detected!");
            Console.WriteLine("Deadlock detected!");
        }

        // Fork class prints
        public void PrintForkPickup(string picker, int forkId)
        {
            Console.WriteLine($"Philosopher {picker} picked up fork {forkId}");
        }

        public void PrintForkPickupFail(string picker, int forkId, string holder)
        {
            Console.WriteLine($"Philosopher {picker} failed to pick up fork {forkId} (already held by philosopher {holder})");
        }

        public void PrintForkPutdown(string putter, int forkId)
        {
            Console.WriteLine($"Philosopher {putter} put down fork {forkId}");
        }

        public void PrintForkPutdownFail(string putter, int forkId, string holder)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Warning: Philosopher {putter} tried to put down fork {forkId} but doesn't hold it (held by philosopher {holder})");
            Console.ResetColor();
        }

        public void PrintForkForceRelease(int forkId, string holder)
        {
            Console.WriteLine($"Force releasing fork {forkId} from philosopher {holder}");
        }

        public string GetPhilosopherStatusDescription(Philosopher philosopher)
        {
            switch (philosopher.State)
            {
                case PhilosopherState.Thinking:
                    return $"Thinking ({philosopher.RemainingTime} steps left)";
                case PhilosopherState.Hungry:
                    if (philosopher.HeldFork1 == null)
                        return "Hungry (trying to get first fork)";
                    else
                        return $"Hungry (holding one fork, trying to get second fork)";
                case PhilosopherState.Eating:
                    return $"Eating ({philosopher.RemainingTime} steps left)";
                default:
                    return "Unknown state";
            }
        }

        public void PrintMetrics(SimulationMetrics metrics)
        {
            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine($"METRICS (Last {metrics.Steps} steps)");
            Console.WriteLine(new string('=', 50));

            // Throughput
            Console.WriteLine("THROUGHPUT");
            var coeff = 1000.0 / metrics.Steps;
            foreach (var eaten in metrics.PhilosopherEatCounts)
            {
                Console.WriteLine($"  {eaten.Key}: {eaten.Value * coeff:F2} meals/1000 steps");
            }
            Console.WriteLine($"Average Throughput: {metrics.AverageEaten * coeff:F2} meals/1000 steps");

            // Waiting time
            Console.WriteLine("WAITING TIME");
            foreach (var hungry in metrics.PhilosopherHungryCounts)
            {
                Console.WriteLine($"  {hungry.Key}: {hungry.Value:F2} hungry/{metrics.Steps} steps");
            }
            Console.WriteLine($"Average Hungry: {metrics.AverageHungry:F2} hungry/1000 steps");

            var maxHungry = metrics.PhilosopherMaxHungryCounts.MaxBy(kvp => kvp.Value);
            if(maxHungry.Value != 0) Console.WriteLine($"Max Hungry: {maxHungry.Key}  {maxHungry.Value} hungry/{metrics.Steps} steps");

            // Utilization coefficient
            Console.WriteLine("FORK UTILIZATION");
            foreach (var fork in metrics.ForkMetrics)
            {
                var utilization = (fork.Value.InUseCounts / (double)metrics.Steps) * 100;
                Console.WriteLine($"  Fork {fork.Key}: {utilization:F2}% utilized");
            }
        }
    }
}