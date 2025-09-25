namespace PhilosophersStepByStep
{
    public class ConsoleMonitor : IMonitor
    {
        public void printInitialConfiguration(int philosopherCount, string mode, string? namesFilePath)
        {
            Console.WriteLine("=== Single-Threaded Dining Philosophers Simulation ===\n");
            Console.WriteLine($"Configuration:");
            Console.WriteLine($"- Philosophers: {philosopherCount}");
            Console.WriteLine($"- Mode: {mode}");
            Console.WriteLine($"- Names file: {(string.IsNullOrEmpty(namesFilePath) ? "None (using default names)" : namesFilePath)}");
            Console.WriteLine($"- Available modes: manual, auto\n");
        }

        public void printUnknownMode(string mode)
        {
            Console.WriteLine($"Unknown mode '{mode}'. Using manual mode.");
        }

        public void printSimRunningExceptionMessage(Exception ex)
        {
            Console.WriteLine($"Error running simulation: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");

        }


        public void printManualModePrerequisits()
        {
            Console.WriteLine("=== Manual Mode ===");
            Console.WriteLine("Controls:");
            Console.WriteLine("  Press Enter = execute next step");
            Console.WriteLine("  Type a number = run that many steps automatically");
            Console.WriteLine("  'q' = quit simulation");
            Console.WriteLine("  'r' = reset simulation");
            Console.WriteLine("  's' = show current status");
        }

        public void printManualModeRequest()
        {
                Console.Write("Press Enter for next step, or type a number: ");
            
        }
        public void printAutoModePrerequisits()
        {
            Console.WriteLine("=== Auto Mode ===");
            Console.WriteLine("Running simulation automatically with 1-second delays...\n");
        }

        public void printSimStatus(string simSummary)
        {
            Console.WriteLine($"Current status: {simSummary}");
        }

        public void printInvalidOption()
        {
            Console.WriteLine("Invalid option. Press Enter for next step, type a number for multiple steps, or 'q' to quit.");    
        }

        // Table class prints
        public void printTableSetup(int philosopherCount)
        {
            Console.WriteLine($"Setting up table with {philosopherCount} philosophers and {philosopherCount} forks...");
        }

        public void printTableSetupComplete()
        {
            Console.WriteLine("Table setup complete!\n");

        }

        public void printSitBetween(string name, int i, int count)
        {
            Console.WriteLine($"{name} sits between fork {i} (left) and fork {(i + 1) % count} (right)");
        }
        public void printCurrentStepStatus(int step, List<Philosopher> philosophers, List<Fork> forks)
        {
            Console.WriteLine($"\n===== STEP {step} =====\n");
            Console.WriteLine("\nPhilosophers: \n");
            foreach (var philosopher in philosophers)
            {
                Console.WriteLine($"\t{philosopher.Name}: {philosopher.GetStatusDescription()} (Eaten: {philosopher.EatCount}, Thought: {philosopher.ThinkCount})");
            }
            Console.WriteLine("\nForks:");
            foreach (var fork in forks)
            {
                Console.WriteLine($"\tFork {fork.Id}: {fork.GetStatusDescription()}");
            }
        }

        public void printNamesLoadingSuccess(int philosopherCount, string fileName)
        {
            Console.WriteLine($"Loaded {philosopherCount} philosopher names from file: {fileName}");            
        }

        public void printNamesLoadingError(Exception ex, string fileName)
        {
            Console.WriteLine($"Warning: Could not read names from file '{fileName}': {ex.Message}");
            Console.WriteLine("Using default names instead.");
        }

        // Fork class prints
        public void printForkPickup(string picker, int forkId)
        {
            Console.WriteLine($"Philosopher {picker} picked up fork {forkId}");
        }

        public void printForkPickupFail(string picker, int forkId, string holder)
        {
            Console.WriteLine($"Philosopher {picker} failed to pick up fork {forkId} (already held by philosopher {holder})");
        }

        public void printForkPutdown(string putter, int forkId)
        {
            Console.WriteLine($"Philosopher {putter} put down fork {forkId}");
        }

        public void printForkPutdownFail(string putter, int forkId, string holder)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Warning: Philosopher {putter} tried to put down fork {forkId} but doesn't hold it (held by philosopher {holder})");
            Console.ResetColor();
        }

        public void printForkForceRelease(int forkId, string holder)
        {
            Console.WriteLine($"Force releasing fork {forkId} from philosopher {holder}");
        }

    }
}