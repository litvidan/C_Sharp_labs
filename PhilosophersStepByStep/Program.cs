namespace PhilosophersStepByStep
{
    class Program
    {
        static void Main(string[] args)
        {
            IMonitor monitor = new ConsoleMonitor();

            try
            {
                int philosopherCount = 5;

                string mode = "manual";
                if (args.Length > 0)
                {
                    mode = args[0].ToLower();
                }

                string? namesFilePath = null;
                if (args.Length > 1)
                {
                    namesFilePath = args[1];
                }

                monitor.printInitialConfiguration(philosopherCount, mode, namesFilePath);

                var table = new Table(monitor, philosopherCount, namesFilePath);

                switch (mode)
                {
                    case "manual":
                        RunManualSimulation(table, monitor);
                        break;
                    case "auto":
                        RunAutoSimulation(table, monitor);
                        break;
                    default:
                        monitor.printUnknownMode(mode);
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
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
