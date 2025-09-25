using System;
using System.Collections.Generic;
using PhilosophersStepByStep.Strategies;

namespace PhilosophersStepByStep.Coordinators
{
    public class Coordinator : ICoordinator
    {
        private readonly HashSet<int> _registeredPhilosophers = new();
        private readonly HashSet<int> _registeredForks = new();

        // Какие вилки сейчас заняты (forkId -> philosopherId)
        private readonly Dictionary<int, int> _forkHolders = new();

        // Состояние ожидания философов (philosopherId -> ждёт разрешения)
        private readonly HashSet<int> _waitingPhilosophers = new();

        public event EventHandler<ForkAvailableEventArgs>? ForkAvailable;

        public void RegisterPhilosopher(int philosopherId)
        {
            _registeredPhilosophers.Add(philosopherId);
        }

        public void RegisterFork(int forkId)
        {
            _registeredForks.Add(forkId);
        }

        public void RequestToEat(int philosopherId)
        {
            if (!_registeredPhilosophers.Contains(philosopherId))
                throw new InvalidOperationException("Philosopher not registered.");

            // Философ хочет поесть - добавим в ожидание
            _waitingPhilosophers.Add(philosopherId);

            // Попробуем выдать вилки, если они свободны
            TryGrantForks(philosopherId);
        }

        public void ReleaseForks(int philosopherId)
        {
            // Убираем философа из ожидания (на всякий случай)
            _waitingPhilosophers.Remove(philosopherId);

            // Освобождаем вилки, которые философ держит
            var forksToRelease = new List<int>();
            foreach (var kvp in _forkHolders)
            {
                if (kvp.Value == philosopherId)
                    forksToRelease.Add(kvp.Key);
            }
            foreach (var forkId in forksToRelease)
            {
                _forkHolders.Remove(forkId);
            }

            // После освобождения вилок попробуем дать их другим философам
            foreach (var waitingPhilosopher in _waitingPhilosophers)
            {
                TryGrantForks(waitingPhilosopher);
            }
        }

        private void TryGrantForks(int philosopherId)
        {
            // Соседние вилки философа: левую - philosopherId, правую - (philosopherId + 1) % count

            int leftFork = philosopherId;
            int rightFork = (philosopherId + 1) % _registeredPhilosophers.Count;

            // Проверяем, свободны ли вилки
            bool leftFree = !_forkHolders.ContainsKey(leftFork);
            bool rightFree = !_forkHolders.ContainsKey(rightFork);

            if (leftFree && rightFree)
            {
                // Выдаем вилки философу
                _forkHolders[leftFork] = philosopherId;
                _forkHolders[rightFork] = philosopherId;

                // Убираем философа из очереди ожидания
                _waitingPhilosophers.Remove(philosopherId);

                // Опускаем события для обеих вилок
                ForkAvailable?.Invoke(this, new ForkAvailableEventArgs(philosopherId, leftFork));
                ForkAvailable?.Invoke(this, new ForkAvailableEventArgs(philosopherId, rightFork));
            }
            // Если невозможно, ничего не делаем (философ ждет)
        }

        public bool DetectDeadlock()
        {
            // Простой дедлок: все философы в ожидании, но вилки никому не выдаются
            // Если есть философы, которые хотят есть, и все вилки заняты, дедлок

            if (_waitingPhilosophers.Count == 0)
                return false;

            // Если количество занятых вилок = количество вилок (все заняты)
            if (_forkHolders.Count == _registeredForks.Count)
            {
                return true;
            }

            return false;
        }
    }
}
