namespace DiningPhilosophers.Contracts
{
    // Philosopher -> Coordinator
    public class PermissionRequestEvent
    {
        public string PhilosopherId { get; set; } = string.Empty;
    }

    // Coordinator -> Philosopher
    public class PermissionGrantedEvent
    {
        public string PhilosopherId { get; set; } = string.Empty;
    }

    // Philosopher -> Coordinator
    public class ForksReleasedEvent
    {
        public string PhilosopherId { get; set; } = string.Empty;
    }
}
