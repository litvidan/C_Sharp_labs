using System.Threading;

namespace PhilosophersHost.Strategies
{
    public interface IPhilosopherStrategy
    {
        Task<bool> TryEatAsync(int philosopherId, int leftForkId, int rightForkId, CancellationToken cancellationToken);
        void StopEating(int leftForkId, int rightForkId);
    }
}