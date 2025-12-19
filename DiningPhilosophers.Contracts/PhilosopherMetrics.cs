namespace DiningPhilosophers.Contracts
{
    public class PhilosopherStateMetrics
    {
        public string PhilosopherId { get; set; } = string.Empty;
        public Dictionary<string, TimeSpan> StateDurations { get; set; } = new();
    }

    public class ForkMetrics
    {
        public int ForkId { get; set; }
        public double UtilizationPercentage { get; set; }
    }

    public class FinalMetricsReport
    {
        public List<PhilosopherStateMetrics> PhilosopherMetrics { get; set; } = new();
        public List<ForkMetrics> ForkMetrics { get; set; } = new();
        public TimeSpan TotalSimulationTime { get; set; }
    }
}
