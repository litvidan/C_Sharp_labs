namespace PhilosophersHost.Configuration
{
    public class SimulationOptions
    {
        public const string Simulation = "Simulation"; // Ключ секции
        public string NamesFilePath { get; set; } = "philosopher_names.txt";

        public int DurationSeconds { get; set; } = 60;
        public int ThinkingTimeMin { get; set; } = 30;
        public int ThinkingTimeMax { get; set; } = 100;
        public int EatingTimeMin { get; set; } = 40;
        public int EatingTimeMax { get; set; } = 50;
        public int ForkAcquisitionTime { get; set; } = 20;
        public int DisplayUpdateInterval { get; set; } = 200;
    }
}