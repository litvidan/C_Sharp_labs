using PhilosophersStepByStep.Coordinators;
using PhilosophersStepByStep.Strategies;

namespace PhilosophersStepByStep
{
    class Program
    {
        static void Main(string[] args)
        {
            IMonitor monitor = new ConsoleMonitor();

            try
            {
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

                ICoordinator? coordinator = config.UseCoordinator ? new Coordinator() : null;

                var table = new Table(monitor, config, new DeadlockForkStrategy(), coordinator);

                switch (config.Mode)
                {
                    case "manual":
                        RunManualSimulation(table, monitor);
                        break;
                    case "auto":
                        RunAutoSimulation(table, monitor);
                        break;
                    default:
                        monitor.PrintUnknownMode(config.Mode);
                        RunManualSimulation(table, monitor);
                        break;
                }
            }
            catch (Exception ex)
            {
                monitor.PrintSimRunningExceptionMessage(ex);
            }
        }

        static void RunManualSimulation(Table table, IMonitor monitor)
        {
            monitor.PrintManualModePrerequisits();

            try
            {
                while (true)
                {
                    monitor.PrintManualModeRequest();
                    var input = Console.ReadLine();

                    if (string.IsNullOrEmpty(input))
                    {
                        table.ExecuteStep();
                    }
                    else if (int.TryParse(input, out int steps) && steps > 0)
                    {
                        for (int i = 0; i < steps; i++)
                        {
                            table.ExecuteStep();
                        }
                    }
                    else if (input.ToLower() == "q")
                    {
                        break;
                    }
                    else if (input.ToLower() == "r")
                    {
                        table.Reset();
                    }
                    else if (input.ToLower() == "s")
                    {
                        monitor.PrintSimStatus(table.GetSimulationSummary());
                    }
                    else
                    {
                        monitor.PrintInvalidOption();
                    }
                }
            }
            finally
            {
                table.PrintFinalMetrics();
            }

        }

        static void RunAutoSimulation(Table table, IMonitor monitor)
        {
            monitor.PrintAutoModePrerequisits();

            try
            {
                for (int step = 1; step <= 1000000; step++)
                {
                    table.ExecuteStep(false);
                    // Thread.Sleep(10);
                }
            }
            finally
            {
                table.PrintFinalMetrics();
            }
        }
    }
}
