namespace PhilosophersStepByStep.Strategies
{
    /// <summary>
    /// Implements an ordered fork acquisition strategy to prevent deadlocks.
    /// Even-numbered philosophers take left fork first, then right fork.
    /// Odd-numbered philosophers take right fork first, then left fork.
    /// </summary>
    public class OrderedForkStrategy : IForkStrategy
    {
        private const int DefaultMaxAttempts = 3;

        /// <summary>
        /// Determines which fork to try to acquire first for a given philosopher.
        /// </summary>
        /// <param name="philosopherId">ID of the philosopher</param>
        /// <param name="leftFork">Left fork available to the philosopher</param>
        /// <param name="rightFork">Right fork available to the philosopher</param>
        /// <returns>The fork to try to acquire first</returns>
        public object GetFirstFork(int philosopherId, object leftFork, object rightFork)
        {
            if (philosopherId % 2 == 0)
            {
                // Even-numbered philosophers: left fork first, then right fork
                return leftFork;
            }
            else
            {
                // Odd-numbered philosophers: right fork first, then left fork
                return rightFork;
            }
        }

        /// <summary>
        /// Determines which fork to try to acquire second for a given philosopher.
        /// </summary>
        /// <param name="philosopherId">ID of the philosopher</param>
        /// <param name="leftFork">Left fork available to the philosopher</param>
        /// <param name="rightFork">Right fork available to the philosopher</param>
        /// <param name="heldFork">The fork currently held by the philosopher</param>
        /// <returns>The fork to try to acquire second</returns>
        public object GetSecondFork(int philosopherId, object leftFork, object rightFork, object heldFork)
        {
            // Return the fork that is not currently held
            if (heldFork == leftFork)
            {
                return rightFork;
            }
            else
            {
                return leftFork;
            }
        }

        /// <summary>
        /// Determines whether a philosopher should release the first fork if the second fork is unavailable.
        /// </summary>
        /// <param name="philosopherId">ID of the philosopher</param>
        /// <param name="attemptsToGetSecondFork">Number of attempts to get the second fork</param>
        /// <param name="maxAttempts">Maximum number of attempts before releasing the first fork</param>
        /// <returns>True if the first fork should be released</returns>
        public bool ShouldReleaseFirstFork(int philosopherId, int attemptsToGetSecondFork, int maxAttempts)
        {
            return attemptsToGetSecondFork >= maxAttempts;
        }

        /// <summary>
        /// Gets the maximum number of attempts to get the second fork before releasing the first fork.
        /// </summary>
        /// <param name="philosopherId">ID of the philosopher</param>
        /// <returns>Maximum number of attempts</returns>
        public int GetMaxAttempts(int philosopherId)
        {
            return DefaultMaxAttempts;
        }
    }
}

