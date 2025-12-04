using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;
using PhilosophersHost.Services.Metrics;

namespace PhilosophersHost.Services
{
    public interface IForkManager
    {
        bool TryAcquireFork(int forkId, int millisecondsTimeout, CancellationToken cancellationToken);
        void ReleaseFork(int forkId);
    }

    public class ForkManager : IForkManager
    {
        private readonly SemaphoreSlim[] _forks;
        private readonly IMetricsCollector _metrics;
        private readonly ConcurrentDictionary<int, long> _acquisitionTimes = new();

        public ForkManager(int count, IMetricsCollector metrics)
        {
            _metrics = metrics;
            _forks = new SemaphoreSlim[count];
            for (int i = 0; i < count; i++)
            {
                // 1= Available, 0 = InUse
                _forks[i] = new SemaphoreSlim(1, 1);
            }
        }

        public bool TryAcquireFork(int forkId, int millisecondsTimeout, CancellationToken cancellationToken)
        {

            bool acquired = _forks[forkId].Wait(millisecondsTimeout, cancellationToken);

            if (acquired)
            {
                _acquisitionTimes[forkId] = Stopwatch.GetTimestamp();
            }

            return acquired;
        }

        public void ReleaseFork(int forkId)
        {
            if (_acquisitionTimes.TryRemove(forkId, out long startTimestamp))
            {
                long endTimestamp = Stopwatch.GetTimestamp();
                double elapsedMs = (endTimestamp - startTimestamp) * 1000.0 / Stopwatch.Frequency;
                _metrics.RecordForkUsage(forkId, elapsedMs);
            }
            _forks[forkId].Release();
        }
    }
}