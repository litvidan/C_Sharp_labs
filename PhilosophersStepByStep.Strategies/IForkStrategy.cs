namespace PhilosophersStepByStep.Strategies
{
    /// <summary>
    /// Defines the strategy for acquiring and releasing forks by philosophers.
    /// </summary>
    public interface IForkStrategy
    {
        /// <summary>
        /// Determines which fork to try to acquire first for a given philosopher.
        /// </summary>
        /// <param name="philosopherId">ID of the philosopher</param>
        /// <param name="leftFork">Left fork available to the philosopher</param>
        /// <param name="rightFork">Right fork available to the philosopher</param>
        /// <returns>The fork to try to acquire first</returns>
        object GetFirstFork(int philosopherId, object leftFork, object rightFork);

        /// <summary>
        /// Determines which fork to try to acquire second for a given philosopher.
        /// </summary>
        /// <param name="philosopherId">ID of the philosopher</param>
        /// <param name="leftFork">Left fork available to the philosopher</param>
        /// <param name="rightFork">Right fork available to the philosopher</param>
        /// <param name="heldFork">The fork currently held by the philosopher</param>
        /// <returns>The fork to try to acquire second</returns>
        object GetSecondFork(int philosopherId, object leftFork, object rightFork, object heldFork);

        /// <summary>
        /// Determines whether a philosopher should release the first fork if the second fork is unavailable.
        /// </summary>
        /// <param name="philosopherId">ID of the philosopher</param>
        /// <param name="attemptsToGetSecondFork">Number of attempts to get the second fork</param>
        /// <param name="maxAttempts">Maximum number of attempts before releasing the first fork</param>
        /// <returns>True if the first fork should be released</returns>
        bool ShouldReleaseFirstFork(int philosopherId, int attemptsToGetSecondFork, int maxAttempts);

        /// <summary>
        /// Gets the maximum number of attempts to get the second fork before releasing the first fork.
        /// </summary>
        /// <param name="philosopherId">ID of the philosopher</param>
        /// <returns>Maximum number of attempts</returns>
        int GetMaxAttempts(int philosopherId);
    }
}
