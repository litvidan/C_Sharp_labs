using System.Threading;

namespace PhilosophersHost.Services
{
    // Интерфейс для внедрения зависимостей
    public interface IForkManager
    {
        bool TryAcquireForks(int philosopherId, int leftForkId, int rightForkId, CancellationToken cancellationToken);
        void ReleaseForks(int leftForkId, int rightForkId);
    }

    public class ForkManager : IForkManager
    {
        private readonly SemaphoreSlim[] _forks;

        public ForkManager(int count)
        {
            // Каждая вилка представлена одним Семафором (доступен только одному потоку)
            _forks = new SemaphoreSlim[count];
            for (int i = 0; i < count; i++)
            {
                _forks[i] = new SemaphoreSlim(1, 1);
            }
        }

        public bool TryAcquireForks(int philosopherId, int leftForkId, int rightForkId, CancellationToken cancellationToken)
        {
            // Стратегия предотвращения Deadlock: всегда брать вилку с меньшим ID первой
            int firstFork = Math.Min(leftForkId, rightForkId);
            int secondFork = Math.Max(leftForkId, rightForkId);

            // Пытаемся взять первую вилку, ожидание с отменой
            if (!_forks[firstFork].Wait(TimeSpan.FromMilliseconds(50), cancellationToken))
            {
                return false; // Не удалось взять первую вилку
            }

            // Пытаемся взять вторую вилку, ожидание с отменой
            if (!_forks[secondFork].Wait(TimeSpan.FromMilliseconds(50), cancellationToken))
            {
                // Если не удалось взять вторую, освобождаем первую и возвращаем false
                _forks[firstFork].Release();
                return false;
            }

            return true; // Успешно взяты обе вилки
        }

        public void ReleaseForks(int leftForkId, int rightForkId)
        {
            // Освобождаем вилки
            _forks[leftForkId].Release();
            _forks[rightForkId].Release();
        }
    }
}