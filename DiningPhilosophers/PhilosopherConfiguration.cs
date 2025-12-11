namespace PhilosophersStepByStep
{
    public class PhilosopherConfiguration
    {
        public int PhilosopherCount { get; set; } = 5;
        public string Mode { get; set; } = "manual";
        public bool UseCoordinator { get; set; } = false;
        public string? NamesFilePath { get; set; }
        public List<string> PhilosopherNames { get; set; } = new();
        public int SimulationDuration { get; set; } = 10000;

        /// <summary>
        /// Load config from file
        /// </summary>
        public static PhilosopherConfiguration LoadFromFile(string filePath)
        {
            var config = new PhilosopherConfiguration();
            
            if (!File.Exists(filePath))
                return config;

            try
            {
                var configLines = File.ReadAllLines(filePath);
                foreach (var line in configLines)
                {
                    var parts = line.Split('=', 2);
                    if (parts.Length != 2) continue;
                    
                    var key = parts[0].Trim().ToLower();
                    var value = parts[1].Trim();

                    switch (key)
                    {
                        case "philosophercount":
                            if (int.TryParse(value, out var pc))
                                config.PhilosopherCount = pc;
                            break;
                        case "mode":
                            config.Mode = value.ToLower();
                            break;
                        case "usecoordinator":
                            config.UseCoordinator = value.ToLower() == "true";
                            break;
                        case "namesfilepath":
                            config.NamesFilePath = value;
                            break;
                        case "simulationduration":
                            if (int.TryParse(value, out var sd))
                                config.SimulationDuration = sd;
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(config.NamesFilePath))
                {
                    config.PhilosopherNames = LoadPhilosopherNames(config.NamesFilePath, config.PhilosopherCount);
                }
                else
                {
                    config.PhilosopherNames = GenerateDefaultNames(config.PhilosopherCount);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not load configuration: {ex.Message}");
            }

            return config;
        }

        /// <summary>
        /// Creates default configuration
        /// </summary>
        public static PhilosopherConfiguration CreateDefault()
        {
            var config = new PhilosopherConfiguration();
            config.PhilosopherNames = GenerateDefaultNames(config.PhilosopherCount);
            return config;
        }

        private static List<string> LoadPhilosopherNames(string namesFilePath, int requiredCount)
        {
            var names = new List<string>();

            if (File.Exists(namesFilePath))
            {
                try
                {
                    var lines = File.ReadAllLines(namesFilePath);
                    foreach (var line in lines)
                    {
                        var trimmedLine = line.Trim();
                        if (!string.IsNullOrEmpty(trimmedLine))
                        {
                            names.Add(trimmedLine);
                        }
                    }
                }
                catch (Exception)
                {
                    names.Clear();
                }
            }

            while (names.Count < requiredCount)
            {
                names.Add($"Philosopher {names.Count}");
            }

            if (names.Count > requiredCount)
            {
                names = names.Take(requiredCount).ToList();
            }

            return names;
        }

        private static List<string> GenerateDefaultNames(int count)
        {
            var names = new List<string>();
            for (int i = 0; i < count; i++)
            {
                names.Add($"Philosopher {i}");
            }
            return names;
        }
    }
}