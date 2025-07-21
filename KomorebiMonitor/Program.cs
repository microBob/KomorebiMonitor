namespace KomorebiMonitor;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        var communicationManager = new CommunicationManager("localhost", 3003, "komorebi");

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        // Handle application exit to ensure the communication is closed properly.
        Application.ApplicationExit += (_, _) => communicationManager.OnApplicationExit();

        Application.Run(new Form1());
    }
}
