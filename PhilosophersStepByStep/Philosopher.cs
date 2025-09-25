using System;
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
        
        private int _eatCount;
        private int _thinkCount;
        private PhilosopherState _state;
        private int _remainingTime; // Steps remaining in current state
        private Fork? _heldFork1;
        private Fork? _heldFork2;
        private int _attemptsToGetSecondFork; // Count attempts to get second fork

        public Philosopher(int id, string name, Fork leftFork, Fork rightFork, IForkStrategy forkStrategy)
        {
            _id = id;
            _name = name;
            _leftFork = leftFork;
            _rightFork = rightFork;
            _forkStrategy = forkStrategy ?? throw new ArgumentNullException(nameof(forkStrategy));
            _random = new Random();
            _eatCount = 0;
            _thinkCount = 0;
            _state = PhilosopherState.Thinking;
            _remainingTime = 0;
            _heldFork1 = null;
            _heldFork2 = null;
            _attemptsToGetSecondFork = 0;
        }

        public int Id => _id;
        public string Name => _name;
        public int EatCount => _eatCount;
        public int ThinkCount => _thinkCount;
        public PhilosopherState State => _state;
        public int RemainingTime => _remainingTime;
        public Fork? HeldFork1 => _heldFork1;
        public Fork? HeldFork2 => _heldFork2;

        /// <summary>
        /// Executes one step of the philosopher's life cycle.
        /// </summary>
        /// <returns>True if the philosopher performed an action, false if waiting</returns>
        public bool ExecuteStep()
        {
            switch (_state)
            {
                case PhilosopherState.Thinking:
                    return ExecuteThinkingStep();
                case PhilosopherState.Hungry:
                    return ExecuteHungryStep();
                case PhilosopherState.Eating:
                    return ExecuteEatingStep();
                default:
                    return false;
            }
        }

        /// <summary>
        /// Executes a thinking step.
        /// </summary>
        private bool ExecuteThinkingStep()
        {
            if (_remainingTime <= 0)
            {
                // Start thinking
                _remainingTime = _random.Next(3, 10); // Think for 3-7 steps
                return true;
            }
            else
            {
                // Continue thinking
                _remainingTime--;
                if (_remainingTime <= 0)
                {
                    _thinkCount++;
                    _state = PhilosopherState.Hungry;
                }
                return true;
            }
        }

        /// <summary>
        /// Executes a hungry step - tries to acquire forks one by one using the fork strategy or delegating this to Coordinator.
        /// </summary>
        private bool ExecuteHungryStep()
        {
            // If we don't have any forks, try to get the first one
            if (_heldFork1 == null)
            {

                // Use strategy to determine which fork to try first
                Fork firstFork = (Fork)_forkStrategy.GetFirstFork(_id, _leftFork, _rightFork);

                // Try to acquire the first fork
                if (firstFork.TryPickUp(this))
                {
                    _heldFork1 = firstFork;
                    return true;
                }
                else
                {
                    return true;
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
                    _remainingTime = _random.Next(4, 5); // Eat for 2-5 steps
                    _attemptsToGetSecondFork = 0; // Reset counter
                    return true;
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
                    return true;
                }
            }
        }

        /// <summary>
        /// Executes an eating step.
        /// </summary>
        private bool ExecuteEatingStep()
        {
            if (_remainingTime <= 0)
            {
                // Finish eating
                _eatCount++;
                
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
                
                _state = PhilosopherState.Thinking;
                _remainingTime = 0;
                return true;
            }
            else
            {
                // Continue eating
                _remainingTime--;
                return true;
            }
        }

        /// <summary>
        /// Gets a description of the philosopher's current state.
        /// </summary>
        public string GetStatusDescription()
        {
            switch (_state)
            {
                case PhilosopherState.Thinking:
                    return $"Thinking ({_remainingTime} steps left)";
                case PhilosopherState.Hungry:
                    if (_heldFork1 == null)
                        return "Hungry (trying to get first fork)";
                    else
                        return $"Hungry (holding one fork, trying to get second fork - attempt {_attemptsToGetSecondFork})";
                case PhilosopherState.Eating:
                    return $"Eating ({_remainingTime} steps left)";
                default:
                    return "Unknown state";
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
            _eatCount = 0;
            _thinkCount = 0;
            _attemptsToGetSecondFork = 0;
        }
    }
}
