using System;
using PhilosophersStepByStep.Coordinators;
using PhilosophersStepByStep.Strategies;

namespace PhilosophersStepByStep
{
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
        private int _attemptsToGetSecondFork;

        public event EventHandler<PhilosopherStateChangeEventArgs>? StateChanged;

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
            private set
            {
                if(_state != value)
                {
                    var oldState = _state;
                    _state = value;
                    StateChanged?.Invoke(this, new PhilosopherStateChangeEventArgs(oldState, value, DateTime.UtcNow));
                }
            }
        }

        public void Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                State = PhilosopherState.Thinking;
                Thread.Sleep(_random.Next(30, 100));

                State = PhilosopherState.Hungry;
                while (!TryAcquireForks())
                {
                    if (token.IsCancellationRequested) return;
                    Thread.Sleep(10);
                }
                
                State = PhilosopherState.Eating;
                Thread.Sleep(_random.Next(40, 50));

                ReleaseForks();
            }
        }

        public bool TryAcquireForks()
        {
            if (_heldFork1 == null)
            {
                Fork firstFork = (Fork)_forkStrategy.GetFirstFork(_id, _leftFork, _rightFork, _leftFork.IsTaken, _rightFork.IsTaken);

                if (firstFork.TryPickUp(this))
                {
                    _heldFork1 = firstFork;
                    _attemptsToGetSecondFork = 0;
                    Thread.Sleep(20);
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
                Thread.Sleep(20);
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

        public void Reset()
        {
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
