using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;

namespace KomorebiMonitor;

public partial class Form1 : Form
{
    #region Constants

    private const string KomorebiSocketAddress = "localhost";
    private const int KomorebiSocketPort = 3003;

    #endregion

    public Form1()
    {
        InitializeComponent();
        var listenerThread = new Thread(ListenForKomorebiEvents) { IsBackground = true };
        listenerThread.Start();
    }

    private static void ListenForKomorebiEvents()
    {
        using var pipeServer = new NamedPipeServerStream("komorebi", PipeDirection.In);
        Console.WriteLine("Waiting for Komorebi to connect...");
        pipeServer.WaitForConnection();
        Console.WriteLine("Komorebi connected.");
        
        while (true)
        {
            try
            {
                using var streamReader = new StreamReader(pipeServer, Encoding.UTF8);
                while (streamReader.ReadLine() is { } line)
                {
                    Console.WriteLine(line);
                    Console.WriteLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
