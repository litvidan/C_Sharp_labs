using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhilosophersHost.Configuration;
using PhilosophersHost.HostedServices;
using PhilosophersHost.Services;
using PhilosophersHost.Services.Metrics;
using PhilosophersHost.Strategies;

namespace PhilosophersHost
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<SimulationOptions>(
                        hostContext.Configuration.GetSection(SimulationOptions.Simulation));

                    var options = hostContext.Configuration
                                             .GetSection(SimulationOptions.Simulation)
                                             .Get<SimulationOptions>();
                                             
                    string namesFile = options.NamesFilePath;
                    string[] philosopherNames;

                    if (File.Exists(namesFile))
                    {
                        philosopherNames = File.ReadAllLines(namesFile)
                                               .Where(line => !string.IsNullOrWhiteSpace(line))
                                               .Select(line => line.Trim())
                                               .ToArray();

                        if (philosopherNames.Length == 0)
                        {
                            Console.WriteLine($"Файл {namesFile} пуст. Используются имена по умолчанию.");
                            philosopherNames = GetDefaultNames();
                        }
                        else
                        {
                            Console.WriteLine($"Загружено {philosopherNames.Length} философов из файла: {namesFile}.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Файл {namesFile} не найден. Используются имена по умолчанию.");
                        philosopherNames = GetDefaultNames();
                    }

                    int philosopherCount = philosopherNames.Length;

                    services.AddSingleton<IMetricsCollector, MetricsCollector>();
                    services.AddHostedService<MetricsMonitorService>();
                    services.AddSingleton<IForkManager>(provider => new ForkManager(philosopherCount, provider.GetRequiredService<IMetricsCollector>()));
                    services.AddSingleton<IPhilosopherStrategy, DeadlockPreventionStrategy>();
                    services.AddHostedService<SimulationLifetimeService>();


                    for (int i = 0; i < philosopherCount; i++)
                    {
                        string name = philosopherNames[i];
                        int id = i;
                        int leftForkId = i;
                        int rightForkId = (i + 1) % philosopherCount;

                        var identityOptions = new PhilosopherIdentityOptions
                        {
                            Name = name,
                            Id = id,
                            LeftForkId = leftForkId,
                            RightForkId = rightForkId
                        };

                        services.AddSingleton<IHostedService>(provider =>
                            ActivatorUtilities.CreateInstance<PhilosopherHostedService>(
                            provider,
                            Options.Create(identityOptions)
                            )
                        );
                    }
                    
                });

        private static string[] GetDefaultNames()
        {
            return new[] { "Платон", "Аристотель", "Сократ", "Декарт", "Кант" };
        }
    }
}