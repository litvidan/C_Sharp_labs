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
        private readonly ICoordinator? _coordinator;

        private PhilosopherState _state;
        private int _remainingTime; // Steps remaining in current state
        private Fork? _heldFork1;
        private Fork? _heldFork2;
        private int _attemptsToGetSecondFork; // Count attempts to get second fork

        public Philosopher(int id, string name, Fork leftFork, Fork rightFork, IForkStrategy forkStrategy, ICoordinator? coordinator = null)
        {
            _id = id;
            _name = name;
            _leftFork = leftFork;
            _rightFork = rightFork;
            _forkStrategy = forkStrategy ?? throw new ArgumentNullException(nameof(forkStrategy));
            _random = new Random();
            _state = PhilosopherState.Thinking;
            _remainingTime = 0;
            _heldFork1 = null;
            _heldFork2 = null;
            _attemptsToGetSecondFork = 0;
            _coordinator = coordinator;

            if (_coordinator != null)
            {
                _coordinator.PickFork += OnPickFork;
            }
        }

        public int Id => _id;
        public string Name => _name;
        public PhilosopherState State => _state;
        public int RemainingTime => _remainingTime;
        public Fork? HeldFork1 => _heldFork1;
        public Fork? HeldFork2 => _heldFork2;

        /// <summary>
        /// Executes one step of the philosopher's life cycle.
        /// </summary>
        /// <returns>True if the philosopher performed an action, false if waiting</returns>
        public void ExecuteStep()
        {
            switch (_state)
            {
                case PhilosopherState.Thinking:
                    ExecuteThinkingStep();
                    break;
                case PhilosopherState.Hungry:
                    if (_coordinator == null) ExecuteHungryStep();
                    else ExecuteHungryStep(_coordinator);
                    break;
                case PhilosopherState.Eating:
                    ExecuteEatingStep();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Executes a thinking step.
        /// </summary>
        private void ExecuteThinkingStep()
        {
            if (_remainingTime <= 0)
            {
                _remainingTime = _random.Next(1, 2); // Think for 3-7 steps
            }
            else
            {
                // Continue thinking
                _remainingTime--;
                if (_remainingTime <= 0)
                {
                    _state = PhilosopherState.Hungry;
                }
            }
        }

        /// <summary>
        /// Executes a hungry step - tries to acquire forks one by one using the fork strategy or delegating this to Coordinator.
        /// </summary>
        private void ExecuteHungryStep()
        {
            // If we don't have any forks, try to get the first one
            if (_heldFork1 == null)
            {
                // Use strategy to determine which fork to try first
                Fork firstFork = (Fork)_forkStrategy.GetFirstFork(_id, _leftFork, _rightFork, _leftFork.State == ForkState.InUse, _rightFork.State == ForkState.InUse);

                // Try to acquire the first fork
                if (firstFork.TryPickUp(this))
                {
                    _heldFork1 = firstFork;
                }
            }
            // If we have one fork, try to get the second one
            else
            {
                _attemptsToGetSecondFork++;

                // Use strategy to determine which fork to try second
                Fork secondFork = (Fork)_forkStrategy.GetSecondFork(_id, _leftFork, _rightFork, _heldFork1);

                // Try to acquire the second fork
                if (secondFork.TryPickUp(this))
                {
                    _heldFork2 = secondFork;
                    _state = PhilosopherState.Eating;
                    _remainingTime = _random.Next(1, 2); // Eat for 4-5 steps
                    _attemptsToGetSecondFork = 0; // Reset counter
                }
                else
                {
                    // Use strategy to determine if we should release the first fork
                    int maxAttempts = _forkStrategy.GetMaxAttempts(_id);
                    if (_forkStrategy.ShouldReleaseFirstFork(_id, _attemptsToGetSecondFork, maxAttempts))
                    {
                        _heldFork1?.PutDown(this);
                        _heldFork1 = null;
                        _attemptsToGetSecondFork = 0;
                    }
                }
            }
        }

        private bool ExecuteHungryStep(ICoordinator coordinator)
        {
            // request to eat
            coordinator.RequestToEat(_id);
            return false;
        }

        /// <summary>
        /// Executes an eating step.
        /// </summary>
        private void ExecuteEatingStep()
        {
            if (_remainingTime <= 0)
            {
                // Release both forks
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

                if (_coordinator != null) _coordinator.ReleaseForks(_id);
                _state = PhilosopherState.Thinking;
                _remainingTime = 0;
            }
            else
            {
                // Continue eating
                _remainingTime--;
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

            _state = PhilosopherState.Thinking;
            _remainingTime = 0;
            _attemptsToGetSecondFork = 0;
        }

        private void OnPickFork(object? sender, ForkEventArgs e)
        {
            if (e.PhilosopherId != _id)
                return;

            // Identify fork instance by fork id from event args
            Fork forkToPick = null!;
            if (_leftFork.Id == e.ForkId) forkToPick = _leftFork;
            else if (_rightFork.Id == e.ForkId) forkToPick = _rightFork;
            else return; // fork id does not match

            // If fork not held yet, try pick it up
            if (_heldFork1 != forkToPick && _heldFork2 != forkToPick)
            {
                if (forkToPick.TryPickUp(this))
                {
                    if (_heldFork1 == null)
                        _heldFork1 = forkToPick;
                    else if (_heldFork2 == null)
                    {
                        _heldFork2 = forkToPick;
                        _state = PhilosopherState.Eating;
                        _remainingTime = _random.Next(4, 5);
                        _attemptsToGetSecondFork = 0;
                    }
                }
            }
        }
    }
}
