using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhilosophersHost.Configuration;
using PhilosophersHost.Services.Metrics;
using System.Text;

namespace PhilosophersHost.HostedServices
{
    public class MetricsMonitorService : BackgroundService
    {
        private readonly IMetricsCollector _collector;
        private readonly SimulationOptions _options;
        // Мы не используем ILogger здесь, чтобы писать красивую таблицу прямо в Console,
        // но можно использовать и Logger, если нужно.
        
        public MetricsMonitorService(
            IMetricsCollector collector, 
            IOptions<SimulationOptions> options)
        {
            _collector = collector;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_options.DisplayUpdateInterval, stoppingToken);
                    PrintMetrics();
                }
                catch (TaskCanceledException) { }
            }
        }

        private void PrintMetrics()
        {
            // Очистка консоли может мерцать, лучше просто выводить блок
            // Console.Clear(); 
            
            var sb = new StringBuilder();
            sb.AppendLine("\n================= МЕТРИКИ СИМУЛЯЦИИ =================");
            
            sb.AppendLine("ФИЛОСОФЫ:");
            sb.AppendLine($"| {"ID",-3} | {"Имя",-12} | {"Думал (с)",-10} | {"Ждал (с)",-10} | {"Ел (с)",-10} |");
            sb.AppendLine("|-----|--------------|------------|------------|------------|");

            foreach (var p in _collector.GetPhilosopherMetrics())
            {
                sb.AppendLine($"| {p.Id,-3} | {p.Name,-12} | {TimeSpan.FromMilliseconds(p.TotalThinkingMs).TotalSeconds,-10:F2} | {TimeSpan.FromMilliseconds(p.TotalWaitingMs).TotalSeconds,-10:F2} | {TimeSpan.FromMilliseconds(p.TotalEatingMs).TotalSeconds,-10:F2} |");
            }

            sb.AppendLine("\nВИЛКИ (Время занятости):");
            sb.AppendLine($"| {"ID",-3} | {"Занята (с)",-10} |");
            sb.AppendLine("|-----|------------|");

            foreach (var f in _collector.GetForkMetrics())
            {
                 sb.AppendLine($"| {f.Id,-3} | {TimeSpan.FromMilliseconds(f.TotalBusyMs).TotalSeconds,-10:F2} |");
            }
            sb.AppendLine("======================================================\n");

            Console.WriteLine(sb.ToString());
        }
    }
}