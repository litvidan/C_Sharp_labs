using System;
using System.Collections.Generic;
using System.Linq;

namespace DiningPhilosophers
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
        private bool _isRunning;

        public Table(int philosopherCount = 5)
        {
            if (philosopherCount < 2)
                throw new ArgumentException("Must have at least 2 philosophers", nameof(philosopherCount));

            _philosopherCount = philosopherCount;
            _philosophers = new List<Philosopher>();
            _forks = new List<Fork>();
            _currentStep = 0;
            _isRunning = false;

            InitializeTable();
        }

        public int PhilosopherCount => _philosopherCount;
        public IReadOnlyList<Philosopher> Philosophers => _philosophers.AsReadOnly();
        public IReadOnlyList<Fork> Forks => _forks.AsReadOnly();
        public int CurrentStep => _currentStep;
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Initializes the table with philosophers and forks.
        /// </summary>
        private void InitializeTable()
        {
            Console.WriteLine($"Setting up table with {_philosopherCount} philosophers and {_philosopherCount} forks...");

            // Create forks
            for (int i = 0; i < _philosopherCount; i++)
            {
                _forks.Add(new Fork(i));
            }

            // Create philosophers and assign forks
            for (int i = 0; i < _philosopherCount; i++)
            {
                Fork leftFork = _forks[i];
                Fork rightFork = _forks[(i + 1) % _philosopherCount]; // Circular arrangement
                
                var philosopher = new Philosopher(i, leftFork, rightFork);
                _philosophers.Add(philosopher);
                
                Console.WriteLine($"Philosopher {i} sits between fork {i} (left) and fork {(i + 1) % _philosopherCount} (right)");
            }

            Console.WriteLine("Table setup complete!\n");
        }

        /// <summary>
        /// Executes one step of the simulation.
        /// </summary>
        /// <returns>True if any philosopher performed an action</returns>
        public bool ExecuteStep()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                Console.WriteLine($"=== Starting Step-by-Step Simulation ===");
                Console.WriteLine($"Philosophers: {_philosopherCount}");
                Console.WriteLine($"Forks: {_philosopherCount}");
                Console.WriteLine("Strategy: Ordered fork acquisition to prevent deadlock\n");
            }

            _currentStep++;
            Console.WriteLine($"\n--- Step {_currentStep} ---");
            
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
            
            // Print current status
            PrintCurrentStatus();
            
            return anyAction;
        }

        /// <summary>
        /// Runs the simulation for a specified number of steps.
        /// </summary>
        /// <param name="maxSteps">Maximum number of steps to run</param>
        public void RunSimulation(int maxSteps = 50)
        {
            Console.WriteLine($"Running simulation for {maxSteps} steps...\n");
            
            for (int step = 1; step <= maxSteps; step++)
            {
                ExecuteStep();
                
                // Optional: Add a pause between steps for readability
                Console.WriteLine("Press Enter to continue to next step (or 'q' to quit)...");
                var input = Console.ReadLine();
                if (input?.ToLower() == "q")
                {
                    Console.WriteLine("Simulation stopped by user.");
                    break;
                }
            }
            
            PrintStatistics();
        }

        /// <summary>
        /// Runs the simulation automatically without user interaction.
        /// </summary>
        /// <param name="maxSteps">Maximum number of steps to run</param>
        public void RunSimulationAuto(int maxSteps = 50)
        {
            Console.WriteLine($"Running automatic simulation for {maxSteps} steps...\n");
            
            for (int step = 1; step <= maxSteps; step++)
            {
                ExecuteStep();
                
                // Small delay for readability
                System.Threading.Thread.Sleep(500);
            }
            
            PrintStatistics();
        }

        /// <summary>
        /// Prints simulation statistics.
        /// </summary>
        public void PrintStatistics()
        {
            Console.WriteLine("\n=== Simulation Statistics ===");
            
            int totalEats = 0;
            int totalThinks = 0;
            
            for (int i = 0; i < _philosophers.Count; i++)
            {
                var philosopher = _philosophers[i];
                Console.WriteLine($"Philosopher {i}: Ate {philosopher.EatCount} times, Thought {philosopher.ThinkCount} times");
                totalEats += philosopher.EatCount;
                totalThinks += philosopher.ThinkCount;
            }
            
            Console.WriteLine($"\nTotal: {totalEats} eating sessions, {totalThinks} thinking sessions");
            Console.WriteLine($"Average eats per philosopher: {(double)totalEats / _philosopherCount:F2}");
            Console.WriteLine($"Average thinks per philosopher: {(double)totalThinks / _philosopherCount:F2}");
            
            // Check for potential starvation
            var minEats = _philosophers.Min(p => p.EatCount);
            var maxEats = _philosophers.Max(p => p.EatCount);
            var difference = maxEats - minEats;
            
            Console.WriteLine($"\nStarvation Analysis:");
            Console.WriteLine($"Min eats: {minEats}, Max eats: {maxEats}, Difference: {difference}");
            
            if (difference > maxEats * 0.5)
            {
                Console.WriteLine("⚠️  Potential starvation detected - some philosophers ate significantly less!");
            }
            else
            {
                Console.WriteLine("✅ No significant starvation detected");
            }
        }

        /// <summary>
        /// Gets the current status of all philosophers and forks.
        /// </summary>
        public void PrintCurrentStatus()
        {
            Console.WriteLine("\n--- Current Status ---");
            
            Console.WriteLine("Philosophers:");
            foreach (var philosopher in _philosophers)
            {
                Console.WriteLine($"  Philosopher {philosopher.Id}: {philosopher.GetStatusDescription()} (Eaten: {philosopher.EatCount}, Thought: {philosopher.ThinkCount})");
            }
            
            Console.WriteLine("Forks:");
            foreach (var fork in _forks)
            {
                string status = fork.IsAvailable ? "Available" : $"Held by Philosopher {fork.HolderId}";
                Console.WriteLine($"  Fork {fork.Id}: {status}");
            }
        }

        /// <summary>
        /// Resets the simulation to initial state.
        /// </summary>
        public void Reset()
        {
            Console.WriteLine("Resetting simulation...");
            
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
            _isRunning = false;
            
            Console.WriteLine("Simulation reset complete.\n");
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
