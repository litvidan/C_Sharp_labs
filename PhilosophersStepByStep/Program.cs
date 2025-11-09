using PhilosophersStepByStep.Coordinators;
using PhilosophersStepByStep.Strategies;
using System.Diagnostics;

namespace PhilosophersStepByStep
{
    class Program
    {
        static void Main(string[] args)
        {
            IMonitor monitor = new ConsoleMonitor();

            try
            {
                // Config loading
                PhilosopherConfiguration config;

                if (args.Length > 0 && File.Exists(args[0]))
                {
                    config = PhilosopherConfiguration.LoadFromFile(args[0]);
                }
                else
                {
                    config = PhilosopherConfiguration.CreateDefault();
                }

                monitor.PrintInitialConfiguration(config.PhilosopherCount, config.Mode, config.NamesFilePath);

                // Start simulation
                var table = new Table(monitor, config, new DeadlockForkStrategy());
                var cts = new CancellationTokenSource();

                List<Task> philosopherTasks = new List<Task>();

                foreach (var philosopher in table.Philosophers)
                {
                    philosopherTasks.Add(Task.Run(() => philosopher.Run(cts.Token)));
                }

                var stopwatch = Stopwatch.StartNew();
                Console.WriteLine(config.SimulationDuration);
                while (stopwatch.ElapsedMilliseconds < config.SimulationDuration)
                {
                    monitor.PrintSimStatus(table.GetSimulationSummary());
                    Thread.Sleep(150); // Delay for a while before the next status update
                }

                cts.Cancel();
                Task.WaitAll(philosopherTasks.ToArray());
                table.PrintFinalMetrics();
            }
            catch (Exception ex)
            {
                monitor.PrintSimRunningExceptionMessage(ex);
            }
        }
        
    }
}
