using System;

namespace DiningPhilosophers
{
    /// <summary>
    /// Represents a philosopher who alternates between thinking and eating.
    /// Single-threaded step-by-step simulation with discrete time steps.
    /// </summary>
    public class Philosopher
    {
        private readonly int _id;
        private readonly Fork _leftFork;
        private readonly Fork _rightFork;
        private readonly Random _random;
        
        private int _eatCount;
        private int _thinkCount;
        private PhilosopherState _state;
        private int _remainingTime; // Steps remaining in current state
        private Fork? _heldFork1;
        private Fork? _heldFork2;

        public Philosopher(int id, Fork leftFork, Fork rightFork)
        {
            _id = id;
            _leftFork = leftFork;
            _rightFork = rightFork;
            _random = new Random();
            _eatCount = 0;
            _thinkCount = 0;
            _state = PhilosopherState.Thinking;
            _remainingTime = 0;
            _heldFork1 = null;
            _heldFork2 = null;
        }

        public int Id => _id;
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
                _remainingTime = _random.Next(3, 8); // Think for 3-7 steps
                Console.WriteLine($"Philosopher {_id} starts thinking for {_remainingTime} steps");
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
                    Console.WriteLine($"Philosopher {_id} finished thinking (total: {_thinkCount}) and is now hungry");
                }
                return true;
            }
        }

        /// <summary>
        /// Executes a hungry step - tries to acquire forks.
        /// </summary>
        private bool ExecuteHungryStep()
        {
            Console.WriteLine($"Philosopher {_id} is hungry and wants to eat");

            // Deadlock prevention: always acquire forks in a consistent order
            Fork firstFork, secondFork;
            
            if (_id % 2 == 0)
            {
                // Even-numbered philosophers: left fork first, then right fork
                firstFork = _leftFork;
                secondFork = _rightFork;
            }
            else
            {
                // Odd-numbered philosophers: right fork first, then left fork
                firstFork = _rightFork;
                secondFork = _leftFork;
            }

            // Try to acquire the first fork
            if (firstFork.TryPickUp(_id))
            {
                _heldFork1 = firstFork;
                
                // Try to acquire the second fork
                if (secondFork.TryPickUp(_id))
                {
                    _heldFork2 = secondFork;
                    _state = PhilosopherState.Eating;
                    _remainingTime = _random.Next(2, 6); // Eat for 2-5 steps
                    Console.WriteLine($"Philosopher {_id} acquired both forks and starts eating for {_remainingTime} steps");
                    return true;
                }
                else
                {
                    // Couldn't get second fork, release the first one
                    firstFork.PutDown(_id);
                    _heldFork1 = null;
                    Console.WriteLine($"Philosopher {_id} couldn't get second fork, released first fork");
                    return true;
                }
            }
            else
            {
                Console.WriteLine($"Philosopher {_id} couldn't get first fork, will try again next step");
                return true;
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
                Console.WriteLine($"Philosopher {_id} finished eating (total: {_eatCount})");
                
                // Release both forks
                if (_heldFork1 != null)
                {
                    _heldFork1.PutDown(_id);
                    _heldFork1 = null;
                }
                if (_heldFork2 != null)
                {
                    _heldFork2.PutDown(_id);
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
                if (_remainingTime <= 0)
                {
                    // Will finish eating next step
                    Console.WriteLine($"Philosopher {_id} is finishing eating...");
                }
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
                    return "Hungry (trying to get forks)";
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
        }
    }

    /// <summary>
    /// Represents the possible states of a philosopher.
    /// </summary>
    public enum PhilosopherState
    {
        Thinking,
        Hungry,
        Eating
    }
}
