namespace PhilosophersHost.Services.Metrics
{
    public enum PhilosopherState
    {
        Thinking,
        Waiting,
        Eating
    }

    public interface IMetricsCollector
    {
        void RecordPhilosopherTime(int philosopherId, string philosopherName, PhilosopherState state, double milliseconds);
        
        void RecordForkUsage(int forkId, double milliseconds);
        
        IEnumerable<PhilosopherMetricData> GetPhilosopherMetrics();
        IEnumerable<ForkMetricData> GetForkMetrics();
    }

    public class PhilosopherMetricData
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double TotalThinkingMs { get; set; }
        public double TotalWaitingMs { get; set; }
        public double TotalEatingMs { get; set; }
    }

    public class ForkMetricData
    {
        public int Id { get; set; }
        public double TotalBusyMs { get; set; }
    }
}