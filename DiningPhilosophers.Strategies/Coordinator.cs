using System;
using System.Collections.Generic;
using PhilosophersStepByStep.Strategies;

namespace PhilosophersStepByStep.Coordinators
{
    public class Coordinator : ICoordinator
    {
        private readonly HashSet<int> _registeredPhilosophers = new();
        private readonly HashSet<int> _registeredForks = new();

        // Tracks which fork is currently held by which philosopher (forkId -> philosopherId)
        private readonly Dictionary<int, int> _forkHolders = new();

        // Philosophers currently waiting for forks
        private readonly HashSet<int> _waitingPhilosophers = new();
        
        // Count of how many times each philosopher has eaten
        private readonly Dictionary<int, int> _eatCounts = new();

        // Event raised when a philosopher can pick up a fork
        public event EventHandler<ForkEventArgs>? PickFork;

        public void RegisterPhilosopher(int philosopherId)
        {
            _registeredPhilosophers.Add(philosopherId);
            if (!_eatCounts.ContainsKey(philosopherId))
                _eatCounts[philosopherId] = 0;
        }

        public void RegisterFork(int forkId)
        {
            _registeredForks.Add(forkId);
        }

        public void RequestToEat(int philosopherId)
        {
            _waitingPhilosophers.Add(philosopherId);
            TryGrantForks(philosopherId);
        }

        public void ReleaseForks(int philosopherId)
        {
            // Remove philosopher from waiting set
            _waitingPhilosophers.Remove(philosopherId);

            // Collect forks that philosopher holds
            var forksToRelease = new List<int>();
            foreach (var holder in _forkHolders)
            {
                if (holder.Value == philosopherId)
                    forksToRelease.Add(holder.Key);
            }

            // Release forks
            foreach (var forkId in forksToRelease)
            {
                _forkHolders.Remove(forkId);
            }

            // Increase eat count for philosopher who finished eating
            if (_eatCounts.ContainsKey(philosopherId))
                _eatCounts[philosopherId]++;

            // Try to grant forks to other waiting philosophers
            foreach (var waitingPhilosopher in _waitingPhilosophers.ToList())
            {
                TryGrantForks(waitingPhilosopher);
            }
            DetectDeadlock();
        }

        private void TryGrantForks(int philosopherId)
        {
            // Get the minimum eat count among all philosophers
            int minEatCount = _eatCounts.Values.Min();

            // If this philosopher ate more times than current minimum, do not grant forks
            if (_eatCounts[philosopherId] > minEatCount)
            {
                return;
            }

            int leftFork = philosopherId;
            int rightFork = (philosopherId + 1) % _registeredPhilosophers.Count;

            // Check if both forks are free
            bool leftFree = !_forkHolders.ContainsKey(leftFork);
            bool rightFree = !_forkHolders.ContainsKey(rightFork);

            if (leftFree && rightFree)
            {
                // Assign forks to philosopher
                _forkHolders[leftFork] = philosopherId;
                _forkHolders[rightFork] = philosopherId;

                // Remove philosopher from waiting set
                _waitingPhilosophers.Remove(philosopherId);

                // Raise PickFork event for both forks
                PickFork?.Invoke(this, new ForkEventArgs(philosopherId, leftFork));
                PickFork?.Invoke(this, new ForkEventArgs(philosopherId, rightFork));
            }
        }

        public bool DetectDeadlock()
        {
            // Deadlock occurs if all philosophers are waiting and all forks are held
            if (_waitingPhilosophers.Count == 0)
                return false;

            if (_forkHolders.Count == _registeredForks.Count)
                return true;

            return false;
        }
    }
}
