using System.Collections.Concurrent;

namespace PhilosophersHost.Services.Metrics
{
    public class MetricsCollector : IMetricsCollector
    {
        private readonly ConcurrentDictionary<int, PhilosopherMetricData> _philosopherData = new();
        private readonly ConcurrentDictionary<int, double> _forkData = new();

        public void RecordPhilosopherTime(int philosopherId, string philosopherName, PhilosopherState state, double milliseconds)
        {
            _philosopherData.AddOrUpdate(philosopherId,
                id => 
                {
                    var data = new PhilosopherMetricData { Id = id, Name = philosopherName };
                    UpdateState(data, state, milliseconds);
                    return data;
                },
                (id, data) =>
                {
                    UpdateState(data, state, milliseconds);
                    return data;
                });
        }

        private void UpdateState(PhilosopherMetricData data, PhilosopherState state, double ms)
        {
            lock (data)
            {
                switch (state)
                {
                    case PhilosopherState.Thinking: data.TotalThinkingMs += ms; break;
                    case PhilosopherState.Waiting: data.TotalWaitingMs += ms; break;
                    case PhilosopherState.Eating: data.TotalEatingMs += ms; break;
                }
            }
        }

        public void RecordForkUsage(int forkId, double milliseconds)
        {
            _forkData.AddOrUpdate(forkId, milliseconds, (id, current) => current + milliseconds);
        }

        public IEnumerable<PhilosopherMetricData> GetPhilosopherMetrics()
        {
            return _philosopherData.Values.OrderBy(p => p.Id).ToList();
        }

        public IEnumerable<ForkMetricData> GetForkMetrics()
        {
            return _forkData.Select(kvp => new ForkMetricData { Id = kvp.Key, TotalBusyMs = kvp.Value })
                            .OrderBy(f => f.Id)
                            .ToList();
        }
    }
}