using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DiningPhilosophers.Contracts;

namespace DiningPhilosophers.TableService.Services
{
    public class MetricsCollector : IMetricsCollector
    {
        private readonly int _totalPhilosophers;
        private readonly ConcurrentBag<string> _finishedPhilosophers = new ConcurrentBag<string>();
        private int _finishedCount = 0;
        private readonly Stopwatch _simulationTimer = Stopwatch.StartNew();

        // For philosopher state metrics
        private readonly ConcurrentDictionary<string, List<(string State, DateTime Timestamp)>> _philosopherHistory = new();

        // For fork utilization metrics
        private readonly ConcurrentDictionary<int, List<(bool IsTaken, DateTime Timestamp)>> _forkHistory = new();

        public MetricsCollector(int totalPhilosophers)
        {
            _totalPhilosophers = totalPhilosophers;
        }

        public void RecordPhilosopherStateChange(string philosopherId, string newState)
        {
            var history = _philosopherHistory.GetOrAdd(philosopherId, new List<(string, DateTime)>());
            lock (history)
            {
                history.Add((newState, DateTime.UtcNow));
            }
        }

        public void RecordForkUsage(int forkId, bool isTaken)
        {
            var history = _forkHistory.GetOrAdd(forkId, new List<(bool, DateTime)>());
            lock (history)
            {
                history.Add((isTaken, DateTime.UtcNow));
            }
        }

        public void RecordPhilosopherFinished(string philosopherId)
        {
            if (!_finishedPhilosophers.Contains(philosopherId))
            {
                _finishedPhilosophers.Add(philosopherId);
                Interlocked.Increment(ref _finishedCount);
                Console.WriteLine($"Philosopher '{philosopherId}' has finished. Total finished: {_finishedCount}/{_totalPhilosophers}");

                if (_finishedCount >= _totalPhilosophers)
                {
                    _simulationTimer.Stop();
                    PrintFinalMetrics();
                }
            }
        }

        private void PrintFinalMetrics()
        {
            Console.WriteLine("\n--- All philosophers have finished. Calculating final metrics... ---");
            var report = new FinalMetricsReport
            {
                TotalSimulationTime = _simulationTimer.Elapsed
            };

            // Calculate Philosopher Metrics
            foreach (var philosopherEntry in _philosopherHistory)
            {
                var philosopherId = philosopherEntry.Key;
                var history = philosopherEntry.Value;
                var metrics = new PhilosopherStateMetrics { PhilosopherId = philosopherId };

                for (int i = 0; i < history.Count - 1; i++)
                {
                    var duration = history[i + 1].Timestamp - history[i].Timestamp;
                    var state = history[i].State;
                    if (!metrics.StateDurations.ContainsKey(state))
                    {
                        metrics.StateDurations[state] = TimeSpan.Zero;
                    }
                    metrics.StateDurations[state] += duration;
                }
                report.PhilosopherMetrics.Add(metrics);
            }

            // Calculate Fork Metrics
            foreach (var forkEntry in _forkHistory)
            {
                var forkId = forkEntry.Key;
                var history = forkEntry.Value;
                var totalUsedTime = TimeSpan.Zero;

                for (int i = 0; i < history.Count - 1; i++)
                {
                    if (history[i].IsTaken)
                    {
                        totalUsedTime += history[i + 1].Timestamp - history[i].Timestamp;
                    }
                }
                
                var utilization = report.TotalSimulationTime.TotalMilliseconds > 0 
                    ? (totalUsedTime.TotalMilliseconds / report.TotalSimulationTime.TotalMilliseconds) * 100 
                    : 0;

                report.ForkMetrics.Add(new ForkMetrics { ForkId = forkId, UtilizationPercentage = utilization });
            }

            // Print Report
            Console.WriteLine($"\nTotal Simulation Time: {report.TotalSimulationTime:g}");
            Console.WriteLine("\n--- Philosopher State Durations ---");
            foreach (var pMetrics in report.PhilosopherMetrics.OrderBy(p => p.PhilosopherId))
            {
                Console.WriteLine($"\n  Philosopher: {pMetrics.PhilosopherId}");
                foreach (var duration in pMetrics.StateDurations)
                {
                    Console.WriteLine($"    - {duration.Key}: {duration.Value:g}");
                }
            }

            Console.WriteLine("\n--- Fork Utilization ---");
            foreach (var fMetrics in report.ForkMetrics.OrderBy(f => f.ForkId))
            {
                Console.WriteLine($"  Fork {fMetrics.ForkId}: {fMetrics.UtilizationPercentage:F2}%");
            }
        }
    }
}
