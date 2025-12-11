using System;

namespace PhilosophersStepByStep.Strategies
{
    /// <summary>
    /// Implements a fork acquisition strategy that always picks the left fork first.
    /// This leads to a deadlock because all philosophers try to take the same fork first.
    /// </summary>
    public class DeadlockForkStrategy : IForkStrategy
    {
        private const int DefaultMaxAttempts = int.MaxValue;
        private readonly Random _random = new Random();

        public object GetFirstFork(int philosopherId, object leftFork, object rightFork, bool isLeftBlocked = false, bool isRightBlocked = false)
        {
            if (isLeftBlocked) return rightFork;
            if (isRightBlocked) return leftFork;
            return _random.Next(0, 2) == 0 ? leftFork : rightFork;
        }

        public object GetSecondFork(int philosopherId, object leftFork, object rightFork, object heldFork)
        {
            if (heldFork == leftFork)
            {
                return rightFork;
            }
            else
            {
                return leftFork;
            }
        }

        public bool ShouldReleaseFirstFork(int philosopherId, int attemptsToGetSecondFork, int maxAttempts)
        {
            return false;
        }
        
        public int GetMaxAttempts(int philosopherId)
        {
            return DefaultMaxAttempts;
        }
    }
}
