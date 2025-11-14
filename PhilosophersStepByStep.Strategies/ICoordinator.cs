using System;

namespace PhilosophersStepByStep.Strategies
{
    public class ForkEventArgs : EventArgs
    {
        public int PhilosopherId { get; }
        public int ForkId { get; }

        public ForkEventArgs(int philosopherId, int forkId)
        {
            PhilosopherId = philosopherId;
            ForkId = forkId;
        }
    }
    public interface ICoordinator
    {
        public event EventHandler<ForkEventArgs>? PickFork;
        /// <summary>
        /// Регистрирует философа, чтобы координатор его отслеживал.
        /// </summary>
        /// <param name="philosopherId">Идентификатор философа</param>
        void RegisterPhilosopher(int philosopherId);

        /// <summary>
        /// Регистрирует вилку для управления ее состоянием.
        /// </summary>
        /// <param name="forkId">Идентификатор вилки</param>
        void RegisterFork(int forkId);

        /// <summary>
        /// Философ вызывает этот метод, чтобы заявить о желании поесть.
        /// </summary>
        /// <param name="philosopherId">Идентификатор философа</param>
        void RequestToEat(int philosopherId);

        /// <summary>
        /// Освобождение вилок, которыми владеет философ.
        /// </summary>
        /// <param name="philosopherId">Идентификатор философа</param>
        void ReleaseForks(int philosopherId);

        /// <summary>
        /// Событие, которое координатор вызывает, когда философу можно взять вилку.
        /// Аргументом передается идентификатор философа и вилки.
        /// </summary>

        /// <summary>
        /// Проверка, обнаружен ли дедлок.
        /// </summary>
        /// <returns>True если дедлок есть</returns>
        bool DetectDeadlock();
    }

}
