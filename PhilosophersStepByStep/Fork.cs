using System.Diagnostics;

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

    public class ForkStateChangeEventArgs : EventArgs
    {
        public ForkState PreviousState { get; }
        public ForkState CurrentState { get; }
        public DateTime Timestamp { get; }

        public ForkStateChangeEventArgs(ForkState previousState, ForkState currentState, DateTime timestamp)
        {
            PreviousState = previousState;
            CurrentState = currentState;
            Timestamp = timestamp;
        }
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

        private readonly object _lock = new object();
        private Stopwatch _usageTimer = new Stopwatch();
        public delegate void ForkStateChangedHandler(object sender, ForkStateChangeEventArgs e);
        public event ForkStateChangedHandler? StateChanged;

        public Fork(int id, IMonitor monitor)
        {
            _id = id;
            _state = ForkState.Available;
            _holder = null;
            _monitor = monitor;
        }

        public int Id => _id;
        public ForkState State
        {
            get => _state;
            set
            {
                if(_state != value)
                {
                    var oldState = _state;
                    _state = value;
                    StateChanged?.Invoke(this, new ForkStateChangeEventArgs(oldState, value, DateTime.UtcNow));
                }
            }   
        }
        public bool IsAvailable => State == ForkState.Available;
        public Philosopher? Holder => _holder;

        /// <summary>
        /// Attempts to pick up the fork. Returns true if successful, false if already taken.
        /// </summary>
        /// <param name="philosopherId">ID of the philosopher trying to pick up the fork</param>
        /// <returns>True if fork was picked up successfully</returns>
        public bool TryPickUp(Philosopher philosopher)
        {
            lock(_lock)
            {
                if(State == ForkState.Available)
                {
                    State = ForkState.InUse;
                    _holder = philosopher;
                    _usageTimer.Start();
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Puts down the fork, making it available for other philosophers.
        /// </summary>
        /// <param name="philosopherId">ID of the philosopher putting down the fork</param>
        public void PutDown(Philosopher philosopher)
        {
            lock(_lock)
            {
                if(_holder == philosopher)
                {
                    State = ForkState.Available;
                    _holder = null;
                    _usageTimer.Stop();
                }
            }
        }

        /// <summary>
        /// Forces the fork to be available (for cleanup or reset scenarios).
        /// </summary>
        public void ForceRelease()
        {
            if (State == ForkState.InUse)
            {
                //_monitor.PrintForkForceRelease(forkId: _id, holder: _holder?.Name ?? "Null");
            }
            State = ForkState.Available;
            _holder = null;
        }

        public string GetStatusDescription()
        {
            switch (State)
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
