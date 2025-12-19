using DiningPhilosophers.Contracts;

namespace DiningPhilosophers.TableService.Services
{
    public interface IMetricsCollector
    {
        void RecordPhilosopherStateChange(string philosopherId, string newState);
        void RecordForkUsage(int forkId, bool isTaken);
        void RecordPhilosopherFinished(string philosopherId);
    }
}
