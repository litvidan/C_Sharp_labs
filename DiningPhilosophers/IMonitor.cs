namespace PhilosophersStepByStep
{
    public interface IMonitor
    {
        // Program.cs
        void PrintInitialConfiguration(int philosopherCount, string mode, string? namesFilePath);
        void PrintUnknownMode(string mode);
        void PrintSimRunningExceptionMessage(Exception ex);
        void PrintManualModePrerequisits();
        void PrintManualModeRequest();
        void PrintAutoModePrerequisits();
        void PrintSimStatus(string simSummary);
        void PrintInvalidOption();
        void PrintConfigurationLoaded(int philosopherCount, int namesLoaded, string? source);

        // Table class prints
        void PrintTableSetup(int philosopherCount);
        void PrintTableSetupComplete();
        void PrintSitBetween(string name, int i, int count);
        void PrintCurrentStepStatus(int step, List<Philosopher> philosophers, List<Fork> forks, MetricsCalculator metricsCalculator);
        void PrintNamesLoadingSuccess(int philosopherCount, string fileName);
        void PrintNamesLoadingError(Exception ex, string fileName);
        void PrintDeadlockDetected();

        // Fork class prints
        void PrintForkPickup(string picker, int forkId);
        void PrintForkPickupFail(string picker, int forkId, string holder);
        void PrintForkPutdown(string putter, int forkId);
        void PrintForkPutdownFail(string putter, int forkId, string holder);
        void PrintForkForceRelease(int forkId, string holder);

        string GetPhilosopherStatusDescription(Philosopher philosopher);

        void PrintMetrics(SimulationMetrics metrics);
    }
}