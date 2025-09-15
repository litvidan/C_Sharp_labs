using DiningPhilosophers;

namespace PhilosophersStepByStep
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Single-Threaded Dining Philosophers Simulation ===\n");
            
            try
            {
                // Get number of philosophers from command line or use default
                int philosopherCount = 5;
                if (args.Length > 0 && int.TryParse(args[0], out int parsedCount) && parsedCount >= 2)
                {
                    philosopherCount = parsedCount;
                }
                
                // Get simulation mode from command line or use default
                string mode = "manual";
                if (args.Length > 1)
                {
                    mode = args[1].ToLower();
                }
                
                Console.WriteLine($"Configuration:");
                Console.WriteLine($"- Philosophers: {philosopherCount}");
                Console.WriteLine($"- Mode: {mode}");
                Console.WriteLine($"- Available modes: manual, auto\n");
                
                // Create the table
                var table = new Table(philosopherCount);
                
                // Run simulation based on mode
                switch (mode)
                {
                    case "manual":
                        RunManualSimulation(table);
                        break;
                    case "auto":
                        RunAutoSimulation(table);
                        break;
                    default:
                        Console.WriteLine($"Unknown mode '{mode}'. Using manual mode.");
                        RunManualSimulation(table);
                        break;
                }
                
                Console.WriteLine("\n=== Simulation Complete ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running simulation: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        
        /// <summary>
        /// Runs the simulation with manual control - user controls each step.
        /// </summary>
        static void RunManualSimulation(Table table)
        {
            Console.WriteLine("=== Manual Mode ===");
            Console.WriteLine("Controls:");
            Console.WriteLine("  Press Enter = execute next step");
            Console.WriteLine("  Type a number = run that many steps automatically");
            Console.WriteLine("  'q' = quit simulation");
            Console.WriteLine("  'r' = reset simulation");
            Console.WriteLine("  's' = show current status");
            Console.WriteLine("  'stats' = show statistics\n");
            
            while (true)
            {
                Console.Write("Press Enter for next step, or type a number: ");
                var input = Console.ReadLine();
                
                if (string.IsNullOrEmpty(input))
                {
                    // Execute one step
                    bool actionPerformed = table.ExecuteStep();
                    if (!actionPerformed)
                    {
                        Console.WriteLine("No actions performed this step.");
                    }
                }
                else if (int.TryParse(input, out int steps) && steps > 0)
                {
                    // Execute multiple steps
                    Console.WriteLine($"Running {steps} steps automatically...\n");
                    for (int i = 0; i < steps; i++)
                    {
                        table.ExecuteStep();
                        if (i < steps - 1)
                        {
                            System.Threading.Thread.Sleep(300); // Small delay for readability
                        }
                    }
                }
                else if (input.ToLower() == "q")
                {
                    Console.WriteLine("Quitting simulation...");
                    break;
                }
                else if (input.ToLower() == "r")
                {
                    table.Reset();
                }
                else if (input.ToLower() == "s")
                {
                    Console.WriteLine($"Current status: {table.GetSimulationSummary()}");
                }
                else if (input.ToLower() == "stats")
                {
                    table.PrintStatistics();
                }
                else
                {
                    Console.WriteLine("Invalid input. Press Enter for next step, type a number for multiple steps, or 'q' to quit.");
                }
            }
        }
        
        /// <summary>
        /// Runs the simulation automatically with a delay between steps.
        /// </summary>
        static void RunAutoSimulation(Table table)
        {
            Console.WriteLine("=== Auto Mode ===");
            Console.WriteLine("Running simulation automatically with 1-second delays...\n");
            
            for (int step = 1; step <= 20; step++)
            {
                Console.WriteLine($"\n--- Executing Step {step} ---");
                table.ExecuteStep();
                
                if (step < 20)
                {
                    Console.WriteLine("Waiting 1 second before next step...");
                    System.Threading.Thread.Sleep(1000);
                }
            }
            
            table.PrintStatistics();
        }
        
    }
}