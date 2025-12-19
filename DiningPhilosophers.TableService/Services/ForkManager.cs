using System.Collections.Concurrent;

namespace DiningPhilosophers.TableService.Services
{
    public class ForkManager : IForkManager
    {
        private readonly ConcurrentDictionary<int, string?> _forks;

        public ForkManager(int philosophersCount)
        {
            _forks = new ConcurrentDictionary<int, string?>();
            for (int i = 0; i < philosophersCount; i++)
            {
                _forks.TryAdd(i, null);
            }
        }

        public bool TryTakeFork(int forkId, string philosopherId)
        {
            return _forks.TryUpdate(forkId, philosopherId, null);
        }

        public void ReleaseFork(int forkId)
        {
            if (_forks.ContainsKey(forkId))
            {
                _forks[forkId] = null;
            }
        }
    }
}
