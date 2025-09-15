using System;

namespace DiningPhilosophers
{
    /// <summary>
    /// Represents a fork that philosophers can pick up and put down.
    /// Simple single-threaded implementation for step-by-step simulation.
    /// </summary>
    public class Fork
    {
        private readonly int _id;
        private bool _isAvailable;
        private int? _holderId; // Which philosopher is currently holding this fork

        public Fork(int id)
        {
            _id = id;
            _isAvailable = true;
            _holderId = null;
        }

        public int Id => _id;
        public bool IsAvailable => _isAvailable;
        public int? HolderId => _holderId;

        /// <summary>
        /// Attempts to pick up the fork. Returns true if successful, false if already taken.
        /// </summary>
        /// <param name="philosopherId">ID of the philosopher trying to pick up the fork</param>
        /// <returns>True if fork was picked up successfully</returns>
        public bool TryPickUp(int philosopherId)
        {
            if (_isAvailable)
            {
                _isAvailable = false;
                _holderId = philosopherId;
                Console.WriteLine($"Philosopher {philosopherId} picked up fork {_id}");
                return true;
            }
            else
            {
                Console.WriteLine($"Philosopher {philosopherId} failed to pick up fork {_id} (already held by philosopher {_holderId})");
                return false;
            }
        }

        /// <summary>
        /// Puts down the fork, making it available for other philosophers.
        /// </summary>
        /// <param name="philosopherId">ID of the philosopher putting down the fork</param>
        public void PutDown(int philosopherId)
        {
            if (_holderId == philosopherId)
            {
                _isAvailable = true;
                _holderId = null;
                Console.WriteLine($"Philosopher {philosopherId} put down fork {_id}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Warning: Philosopher {philosopherId} tried to put down fork {_id} but doesn't hold it (held by philosopher {_holderId})");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Forces the fork to be available (for cleanup or reset scenarios).
        /// </summary>
        public void ForceRelease()
        {
            if (!_isAvailable)
            {
                Console.WriteLine($"Force releasing fork {_id} from philosopher {_holderId}");
            }
            _isAvailable = true;
            _holderId = null;
        }
    }
}
