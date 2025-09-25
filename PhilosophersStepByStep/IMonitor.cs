namespace PhilosophersStepByStep
{
    public interface IMonitor
    {
        // Program.cs
        void printInitialConfiguration(int philosopherCount, string mode, string? namesFilePath);
        void printUnknownMode(string mode);
        void printSimRunningExceptionMessage(Exception ex);
        void printManualModePrerequisits();
        void printManualModeRequest();
        void printAutoModePrerequisits();
        void printSimStatus(string simSummary);
        void printInvalidOption();

        // Table class prints
        void printTableSetup(int philosopherCount);
        void printTableSetupComplete();
        void printSitBetween(string name, int i, int count);
        void printCurrentStepStatus(int step, List<Philosopher> philosophers, List<Fork> forks);
        void printNamesLoadingSuccess(int philosopherCount, string fileName);
        void printNamesLoadingError(Exception ex, string fileName);

        // Fork class prints
        void printForkPickup(string picker, int forkId);
        void printForkPickupFail(string picker, int forkId, string holder);
        void printForkPutdown(string putter, int forkId);
        void printForkPutdownFail(string putter, int forkId, string holder);
        void printForkForceRelease(int forkId, string holder);
    }

}