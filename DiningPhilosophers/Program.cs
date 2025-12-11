using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhilosophersStepByStep.Data;
using PhilosophersStepByStep.Strategies;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using DbModels = PhilosophersStepByStep.Data.Models;

namespace PhilosophersStepByStep
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = ConfigureServices();
            var monitor = new ConsoleMonitor();
            var dbLocker = new object();

            try
            {
                var config = LoadConfiguration(args);
                monitor.PrintInitialConfiguration(config.PhilosopherCount, config.Mode, config.NamesFilePath);

                var runId = GetNewRunId(serviceProvider);
                Console.WriteLine($"Simulation Run ID: {runId}");

                var table = new Table(monitor, config, new OrderedForkStrategy());
                
                // Subscribe to state changes to save snapshots
                table.StateChanged += (sender, e) => {
                    lock (dbLocker)
                    {
                        SaveSnapshot(serviceProvider, runId, table);
                        monitor.PrintSimStatus(table.GetSimulationSummary());
                    }
                };

                var cts = new CancellationTokenSource();

                var philosopherThreads = table.Philosophers
                    .Select(p => new Thread(() => p.Run(cts.Token)))
                    .ToList();

                SaveSnapshot(serviceProvider, runId, table);
                monitor.PrintSimStatus(table.GetSimulationSummary());

                philosopherThreads.ForEach(t => t.Start());
                Thread.Sleep(config.SimulationDuration);
                cts.Cancel();
                philosopherThreads.ForEach(t => t.Join());
                
                table.PrintFinalMetrics();
            }
            catch (Exception ex)
            {
                monitor.PrintSimRunningExceptionMessage(ex);
            }
        }

        private static int GetNewRunId(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PhilosophersDbContext>();
                return (dbContext.SimulationRuns.Max(r => (int?)r.RunId) ?? 0) + 1;
            }
        }

        private static IServiceProvider ConfigureServices()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            var services = new ServiceCollection();

            services.AddDbContext<PhilosophersDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            return services.BuildServiceProvider();
        }

        private static PhilosopherConfiguration LoadConfiguration(string[] args)
        {
            if (args.Length > 0 && File.Exists(args[0]))
            {
                return PhilosopherConfiguration.LoadFromFile(args[0]);
            }
            else
            {
                return PhilosopherConfiguration.CreateDefault();
            }
        }

        private static void SaveSnapshot(IServiceProvider serviceProvider, int runId, Table table)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PhilosophersDbContext>();

                var snapshot = new DbModels.SimulationRun
                {
                    RunId = runId,
                    Timestamp = DateTimeOffset.UtcNow
                };

                snapshot.PhilosopherStates = table.Philosophers.Select(p => new DbModels.PhilosopherState
                {
                    PhilosopherId = p.Id,
                    State = p.State.ToString()
                }).ToList();

                snapshot.ForkStates = table.Forks.Select(f => new DbModels.ForkState
                {
                    ForkId = f.Id,
                    IsTaken = f.IsTaken
                }).ToList();

                dbContext.SimulationRuns.Add(snapshot);
                dbContext.SaveChanges();
            }
        }
    }
}
