namespace PhilosophersHost.Configuration
{
    public class PhilosopherIdentityOptions
    {
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; }
        public int LeftForkId { get; set; }
        public int RightForkId { get; set; }
    }
}