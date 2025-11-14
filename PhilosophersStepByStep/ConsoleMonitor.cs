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
            // This is stepless version
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
                    return "Thinking";
                    //return $"Thinking ({philosopher.RemainingTime} steps left)";
                case PhilosopherState.Hungry:
                    if (philosopher.HeldFork1 == null)
                        return "Hungry (trying to get first fork)";
                    else
                        return $"Hungry (holding one fork, trying to get second fork)";
                case PhilosopherState.Eating:
                    return "Eating";
                    //return $"Eating ({philosopher.RemainingTime} steps left)";
                default:
                    return "Unknown state";
            }
        }

        public void PrintMetrics(SimulationMetrics metrics)
        {
            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine($"METRICS (Simulation duration: {metrics.duration.ElapsedMilliseconds} ms)");
            Console.WriteLine(new string('=', 50));

            // Throughput (meals per millisecond)
            Console.WriteLine("THROUGHPUT (meals/ms):");
            foreach (var eaten in metrics.PhilosopherEatThroughput)
            {
                Console.WriteLine($"  {eaten.Key}: {eaten.Value:F5} meals/ms");
            }
            Console.WriteLine($"Average Throughput: {metrics.AverageEatThroughput:F5} meals/ms");

            // Waiting time (hungry time, ms)
            Console.WriteLine("\nWAITING TIME (ms):");
            foreach (var philosopher in metrics.PhilosopherHungryDuration)
            {
                Console.WriteLine($"  {philosopher.Key}: {philosopher.Value} ms hungry");
            }
            Console.WriteLine($"Average Hungry Time: {metrics.AverageHungryTimeMs:F2} ms");
            Console.WriteLine($"Max Hungry Time: {metrics.MaxHungryTimeMs:F2} ms (Philosopher: {metrics.MaxHungryPhilosopher})");

            // Fork utilization (% of time in use)
            Console.WriteLine("\nFORK UTILIZATION (%):");
            foreach (var forkUtil in metrics.ForkUtilizationPercent)
            {
                Console.WriteLine($"  Fork {forkUtil.Key}: {forkUtil.Value:F2}% utilized");
            }
        }
    }
}