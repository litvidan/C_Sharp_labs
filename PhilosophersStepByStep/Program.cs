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

                monitor.printInitialConfiguration(config.PhilosopherCount, config.Mode, config.NamesFilePath);

                ICoordinator? coordinator = config.UseCoordinator ? new Coordinator() : null;

                var table = new Table(monitor, config, new OrderedForkStrategy(), coordinator);

                switch (config.Mode)
                {
                    case "manual":
                        RunManualSimulation(table, monitor);
                        break;
                    case "auto":
                        RunAutoSimulation(table, monitor);
                        break;
                    default:
                        monitor.printUnknownMode(config.Mode);
                        RunManualSimulation(table, monitor);
                        break;
                }
            }
            catch (Exception ex)
            {
                monitor.printSimRunningExceptionMessage(ex);
            }
        }

        static void RunManualSimulation(Table table, IMonitor monitor)
        {
            monitor.printManualModePrerequisits();

            while (true)
            {
                monitor.printManualModeRequest();
                var input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    bool actionPerformed = table.ExecuteStep();
                }
                else if (int.TryParse(input, out int steps) && steps > 0)
                {
                    for (int i = 0; i < steps; i++)
                    {
                        table.ExecuteStep();
                        if (i < steps - 1)
                        {
                            System.Threading.Thread.Sleep(300);
                        }
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
                    monitor.printSimStatus(table.GetSimulationSummary());
                }
                else
                {
                    monitor.printInvalidOption();
                }
            }
        }

        static void RunAutoSimulation(Table table, IMonitor monitor)
        {
            monitor.printAutoModePrerequisits();


            for (int step = 1; step <= 1000000; step++)
            {
                table.ExecuteStep();

                if (step % 1000 == 0)
                {
                    // CalculateMetrics(table, step);
                    // monitor.PrintMetrics(metrics);
                }

                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
