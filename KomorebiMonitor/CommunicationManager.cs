using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using KomorebiMonitor.SocketModel;

namespace KomorebiMonitor;

public class CommunicationManager
{
    #region Properties

    private readonly string _komorebiNamedPipeName;

    private readonly NetworkStream _networkStream;

    #endregion

    public CommunicationManager(
        string komorebiSocketAddress,
        int komorebiSocketPort,
        string komorebiNamedPipeName
    )
    {
        _komorebiNamedPipeName = komorebiNamedPipeName;

        // Start a background thread to listen for Komorebi events on the named pipe.
        var pipeThread = new Thread(ListenForKomorebiEvents) { IsBackground = true };
        pipeThread.Start();

        // Start a socket connection to the Komorebi server to send commands.
        using var client = new TcpClient();
        client.Connect(komorebiSocketAddress, komorebiSocketPort);
        Console.WriteLine("Connected to Komorebi socket server.");
        _networkStream = client.GetStream();
        RequestKomorebiSubscribeToNamedPipe();
    }

    private void ListenForKomorebiEvents()
    {
        using var pipeServer = new NamedPipeServerStream(_komorebiNamedPipeName, PipeDirection.In);
        Console.WriteLine("Waiting for Komorebi to connect to named pipe...");
        pipeServer.WaitForConnection();
        Console.WriteLine("Komorebi connected to named pipe.");

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

    private void RequestKomorebiSubscribeToNamedPipe()
    {
        // Create message to subscribe to named pipe.
        var message = new SocketMessage
        {
            Content = new Content { String = _komorebiNamedPipeName },
            Type = TypeEnum.AddSubscriberPipe,
        };
        Console.WriteLine(message.ToJson());
        var data = Encoding.UTF8.GetBytes(message.ToJson());

        _networkStream.Write(data, 0, data.Length);
        Console.WriteLine("Sent named pipe message");
    }

    public void OnApplicationExit()
    {
        _networkStream.Dispose();
    }
}
