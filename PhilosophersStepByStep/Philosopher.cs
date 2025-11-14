using System;
using PhilosophersStepByStep.Coordinators;
using PhilosophersStepByStep.Strategies;

namespace PhilosophersStepByStep
{

    /// <summary>
    /// Represents the possible states of a philosopher.
    /// </summary>
    public enum PhilosopherState
    {
        Thinking,
        Hungry,
        Eating
    }

    public class PhilosopherStateChangeEventArgs : EventArgs
    {
        public PhilosopherState PreviousState { get; }
        public PhilosopherState CurrentState { get; }
        public DateTime Timestamp { get; }

        public PhilosopherStateChangeEventArgs(PhilosopherState previousState, PhilosopherState currentState, DateTime timestamp)
        {
            PreviousState = previousState;
            CurrentState = currentState;
            Timestamp = timestamp;
        }
    }

    /// <summary>
    /// Represents a philosopher who alternates between thinking and eating.
    /// Single-threaded step-by-step simulation with discrete time steps.
    /// </summary>
    public class Philosopher
    {
        private readonly int _id;
        private readonly string _name;
        private readonly Fork _leftFork;
        private readonly Fork _rightFork;
        private readonly Random _random;
        private readonly IForkStrategy _forkStrategy;

        private PhilosopherState _state;
        private Fork? _heldFork1;
        private Fork? _heldFork2;
        private int _attemptsToGetSecondFork; // Count attempts to get second fork

        public delegate void PhilosopherStateChangedHandler(object sender, PhilosopherStateChangeEventArgs e);
        public event PhilosopherStateChangedHandler? StateChanged;

        public Philosopher(int id, string name, Fork leftFork, Fork rightFork, IForkStrategy forkStrategy)
        {
            _id = id;
            _name = name;
            _leftFork = leftFork;
            _rightFork = rightFork;
            _forkStrategy = forkStrategy ?? throw new ArgumentNullException(nameof(forkStrategy));
            _random = new Random();
            State = PhilosopherState.Thinking;
            _heldFork1 = null;
            _heldFork2 = null;
            _attemptsToGetSecondFork = 0;
        }

        public int Id => _id;
        public string Name => _name;
        public Fork? HeldFork1 => _heldFork1;
        public Fork? HeldFork2 => _heldFork2;
        public PhilosopherState State
        {
            get => _state;
            set
            {
                if(_state != value)
                {
                    var oldState = _state;
                    _state = value;
                    StateChanged?.Invoke(this, new PhilosopherStateChangeEventArgs(oldState, value, DateTime.UtcNow));
                }
            }
        }

        /// <summary>
        /// Runs the philosopher's lifecycle in a loop until cancellation is requested.
        /// The philosopher alternates between thinking, becoming hungry and trying to acquire forks,
        /// eating for a random amount of time, and then releasing the forks.
        /// </summary>
        /// <param name="token">CancellationToken to stop the simulation gracefully.</param>
        public void Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // Thinking
                int thinkingTime = _random.Next(30, 100); // Thinking time between 30 and 100 ms
                State = PhilosopherState.Thinking;
                Thread.Sleep(thinkingTime);

                // Hungry
                State = PhilosopherState.Hungry;
                TryAcquireForks();

                // Eating  
                int eatingTime = _random.Next(40, 50); // Eating time between 40 and 50 ms
                State = PhilosopherState.Eating;
                Thread.Sleep(eatingTime);

                ReleaseForks();
            }
        }

        /// <summary>
        /// Attempts to acquire both forks according to the fork strategy.
        /// Tries to pick up the first fork, then the second fork.
        /// Releases the first fork if the second cannot be acquired after several attempts,
        /// according to the strategy.
        /// </summary>
        /// <returns>True if both forks were successfully acquired; otherwise, false.</returns>
        private bool TryAcquireForks()
        {
            if (_heldFork1 == null)
            {
                Fork firstFork = (Fork)_forkStrategy.GetFirstFork(_id, _leftFork, _rightFork, _leftFork.State == ForkState.InUse, _rightFork.State == ForkState.InUse);

                if (firstFork.TryPickUp(this))
                {
                    _heldFork1 = firstFork;
                    _attemptsToGetSecondFork = 0;
                    Thread.Sleep(20); // Fork picking 20 ms
                }
                else
                {
                    return false;
                }
            }

            _attemptsToGetSecondFork++;

            Fork secondFork = (Fork)_forkStrategy.GetSecondFork(_id, _leftFork, _rightFork, _heldFork1);

            if (secondFork.TryPickUp(this))
            {
                _heldFork2 = secondFork;
                Thread.Sleep(20); // Fork picking 20 ms
                return true;
            }
            else
            {
                int maxAttempts = _forkStrategy.GetMaxAttempts(_id);
                if (_forkStrategy.ShouldReleaseFirstFork(_id, _attemptsToGetSecondFork, maxAttempts))
                {
                    _heldFork1.PutDown(this);
                    _heldFork1 = null;
                    _attemptsToGetSecondFork = 0;
                }
                return false;
            }
        }

        /// <summary>
        /// Releases both forks currently held by the philosopher.
        /// Sets held fork references to null after releasing.
        /// </summary>
        public void ReleaseForks()
        {
            if (_heldFork1 != null)
            {
                _heldFork1.PutDown(this);
                _heldFork1 = null;
            }
            if (_heldFork2 != null)
            {
                _heldFork2.PutDown(this);
                _heldFork2 = null;
            }
        }

        /// <summary>
        /// Resets the philosopher to initial state.
        /// </summary>
        public void Reset()
        {
            // Release any held forks
            if (_heldFork1 != null)
            {
                _heldFork1.ForceRelease();
                _heldFork1 = null;
            }
            if (_heldFork2 != null)
            {
                _heldFork2.ForceRelease();
                _heldFork2 = null;
            }

            State = PhilosopherState.Thinking;
            _attemptsToGetSecondFork = 0;
        }
    }
}
