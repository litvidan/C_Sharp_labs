namespace DiningPhilosophers.Contracts
{
    public class StateChangeRequest
    {
        public string PhilosopherId { get; set; } = string.Empty;
        public string NewState { get; set; } = string.Empty; // e.g., "Thinking", "Eating", "Hungry"
    }
}
