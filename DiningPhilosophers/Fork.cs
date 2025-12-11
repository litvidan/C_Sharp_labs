using System.Diagnostics;

namespace PhilosophersStepByStep
{
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

    public class Fork
    {
        private readonly int _id;
        private ForkState _currentState;
        private Philosopher? _holder;
        private readonly IMonitor _monitor;

        private readonly object _lock = new object();
        private Stopwatch _usageTimer = new Stopwatch();
        public event EventHandler<ForkStateChangeEventArgs>? StateChanged;

        public Fork(int id, IMonitor monitor)
        {
            _id = id;
            _currentState = ForkState.Available;
            _holder = null;
            _monitor = monitor;
        }

        public int Id => _id;
        public ForkState CurrentState
        {
            get => _currentState;
            private set
            {
                if(_currentState != value)
                {
                    var oldState = _currentState;
                    _currentState = value;
                    StateChanged?.Invoke(this, new ForkStateChangeEventArgs(oldState, value, DateTime.UtcNow));
                }
            }   
        }
        public bool IsAvailable => CurrentState == ForkState.Available;
        public bool IsTaken => CurrentState == ForkState.InUse;
        public Philosopher? Holder => _holder;

        public bool TryPickUp(Philosopher philosopher)
        {
            lock(_lock)
            {
                if(CurrentState == ForkState.Available)
                {
                    CurrentState = ForkState.InUse;
                    _holder = philosopher;
                    _usageTimer.Start();
                    return true;
                }
                return false;
            }
        }

        public void PutDown(Philosopher philosopher)
        {
            lock(_lock)
            {
                if(_holder == philosopher)
                {
                    CurrentState = ForkState.Available;
                    _holder = null;
                    _usageTimer.Stop();
                }
            }
        }

        public void ForceRelease()
        {
            CurrentState = ForkState.Available;
            _holder = null;
        }

        public string GetStatusDescription()
        {
            switch (CurrentState)
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
