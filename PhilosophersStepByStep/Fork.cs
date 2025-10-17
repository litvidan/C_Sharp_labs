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

        public int TotalUsedSteps { get; private set; }
        public int TotalBlockedSteps { get; private set; }
        public int TotalAvailableSteps { get; private set; }

        public Fork(int id, IMonitor monitor)
        {
            _id = id;
            _state = ForkState.Available;
            _holder = null;
            _monitor = monitor;

            TotalUsedSteps = 0;
            TotalBlockedSteps = 0;
            TotalAvailableSteps = 0;
        }

        public int Id => _id;
        public ForkState State => _state;
        public bool IsAvailable => _state == ForkState.Available;
        public Philosopher? Holder => _holder;

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
                // _monitor.PrintForkPickup(picker: philosopher.Name, forkId: _id);
                return true;
            }
            else
            {
                // _monitor.PrintForkPickupFail(picker: philosopher.Name, forkId: _id, holder: _holder?.Name ?? "NULL");
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
                //_monitor.PrintForkPutdown(putter: philosopher.Name, forkId: _id); // This should be in monitor
            }
            else
            {
                //_monitor.PrintForkPutdownFail(putter: philosopher.Name, forkId: _id, holder: _holder?.Name ?? "Null");
            }
        }

        /// <summary>
        /// Forces the fork to be available (for cleanup or reset scenarios).
        /// </summary>
        public void ForceRelease()
        {
            if (_state == ForkState.InUse)
            {
                //_monitor.PrintForkForceRelease(forkId: _id, holder: _holder?.Name ?? "Null");
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

        public void UpdateMetrics(bool philosoperEating)
        {
            if (_state == ForkState.Available)
            {
                TotalAvailableSteps++;
            }
            else if (_state == ForkState.InUse && philosoperEating)
            {
                TotalUsedSteps++;
            }
            else if (_state == ForkState.InUse && !philosoperEating)
            {
                TotalBlockedSteps++;
            }
        }
    }
}
