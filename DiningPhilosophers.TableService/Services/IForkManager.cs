namespace DiningPhilosophers.TableService.Services
{
    public interface IForkManager
    {
        bool TryTakeFork(int forkId, string philosopherId);
        void ReleaseFork(int forkId);
    }
}
