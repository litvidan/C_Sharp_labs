using System;

namespace PhilosophersStepByStep
{
    /// <summary>
    /// Represents the possible states of a fork.
    /// </summary>
    public enum ForkState
    {
        Available,
        InUse
    }

    /// <summary>
    /// Represents a fork that philosophers can pick up and put down.
    /// Simple single-threaded implementation for step-by-step simulation.
    /// </summary>
    public class Fork
    {
        private readonly int _id;
        private ForkState _state;
        private Philosopher? _holder;
        private readonly IMonitor _monitor;

        public Fork(int id, IMonitor monitor)
        {
            _id = id;
            _state = ForkState.Available;
            _holder = null;
            _monitor = monitor;
        }

        public int Id => _id;
        public ForkState State => _state;
        public bool IsAvailable => _state == ForkState.Available;

        /// <summary>
        /// Attempts to pick up the fork. Returns true if successful, false if already taken.
        /// </summary>
        /// <param name="philosopherId">ID of the philosopher trying to pick up the fork</param>
        /// <returns>True if fork was picked up successfully</returns>
        public bool TryPickUp(Philosopher philosopher)
        {
            if (_state == ForkState.Available)
            {
                _state = ForkState.InUse;
                _holder = philosopher;
                _monitor.printForkPickup(picker: philosopher.Name, forkId: _id);
                return true;
            }
            else
            {
                _monitor.printForkPickupFail(picker: philosopher.Name, forkId: _id, holder: _holder?.Name ?? "NULL");
                return false;
            }
        }

        /// <summary>
        /// Puts down the fork, making it available for other philosophers.
        /// </summary>
        /// <param name="philosopherId">ID of the philosopher putting down the fork</param>
        public void PutDown(Philosopher philosopher)
        {
            if (_holder == philosopher)
            {
                _state = ForkState.Available;
                _holder = null;
                _monitor.printForkPutdown(putter: philosopher.Name, forkId: _id);
            }
            else
            {
                _monitor.printForkPutdownFail(putter: philosopher.Name, forkId: _id, holder: _holder?.Name ?? "Null");
            }
        }

        /// <summary>
        /// Forces the fork to be available (for cleanup or reset scenarios).
        /// </summary>
        public void ForceRelease()
        {
            if (_state == ForkState.InUse)
            {
                _monitor.printForkForceRelease(forkId: _id, holder: _holder?.Name ?? "Null");
            }
            _state = ForkState.Available;
            _holder = null;
        }

        public string GetStatusDescription()
        {
            switch (_state)
            {
                case ForkState.Available:
                    return "Available";
                case ForkState.InUse:
                    return $"InUse (used by {_holder?.Name})";
                default:
                    return "Unknown state";
            }
        }
    }
}
