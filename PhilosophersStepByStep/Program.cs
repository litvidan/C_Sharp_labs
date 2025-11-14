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
                var table = new Table(monitor, config, new OrderedForkStrategy());
                var cts = new CancellationTokenSource();

                List<Thread> philosopherThreads = new List<Thread>();

                foreach (var philosopher in table.Philosophers)
                {
                    Thread thread = new Thread(() => philosopher.Run(cts.Token));
                    philosopherThreads.Add(thread);
                    thread.Start();
                }

                var stopwatch = Stopwatch.StartNew();
                Console.WriteLine(config.SimulationDuration);
                while (stopwatch.ElapsedMilliseconds < config.SimulationDuration)
                {
                    monitor.PrintSimStatus(table.GetSimulationSummary());
                    Thread.Sleep(150);
                }

                cts.Cancel();
                foreach (var thread in philosopherThreads)
                {
                    thread.Join();
                }
                table.PrintFinalMetrics();
            }
            catch (Exception ex)
            {
                monitor.PrintSimRunningExceptionMessage(ex);
            }
        }
        
    }
}
