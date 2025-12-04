using Microsoft.Extensions.Options;
using PhilosophersHost.Configuration;
using PhilosophersHost.Services;

namespace PhilosophersHost.Strategies
{
    public class DeadlockPreventionStrategy : IPhilosopherStrategy
    {
        private readonly IForkManager _forkManager;
        private const int ForkTimeout = 50;
        
        public DeadlockPreventionStrategy(IForkManager forkManager)
        {
            _forkManager = forkManager;
        }

        public async Task<bool> TryEatAsync(int philosopherId, int leftForkId, int rightForkId, CancellationToken cancellationToken)
        {
            int firstForkId = Math.Min(leftForkId, rightForkId);
            int secondForkId = Math.Max(leftForkId, rightForkId);

            if (!_forkManager.TryAcquireFork(firstForkId, ForkTimeout, cancellationToken))
            {
                return false;
            }

            if (!_forkManager.TryAcquireFork(secondForkId, ForkTimeout, cancellationToken))
            {
                _forkManager.ReleaseFork(firstForkId);
                return false;
            }

            return true;
        }

        public void StopEating(int leftForkId, int rightForkId)
        {
            _forkManager.ReleaseFork(leftForkId);
            _forkManager.ReleaseFork(rightForkId);
        }
    }
}