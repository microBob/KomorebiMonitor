using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using KomorebiMonitor.SocketModel;

namespace KomorebiMonitor;

public class CommunicationManager
{
    #region Properties

    private readonly string _komorebiNamedPipeName;

    private readonly NamedPipeServerStream _namedPipeServer;
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
        _namedPipeServer = new NamedPipeServerStream(_komorebiNamedPipeName, PipeDirection.In);
        var pipeThread = new Thread(ListenForKomorebiEvents) { IsBackground = true };
        pipeThread.Start();

        // Start a socket connection to the Komorebi server to send commands.
        using var client = new TcpClient();
        client.Connect(komorebiSocketAddress, komorebiSocketPort);
        Console.WriteLine("Connected to Komorebi socket server.");
        _networkStream = client.GetStream();
        RequestKomorebiSubscribeToNamedPipe();

        while (true)
        {
            try
            {
                if (_networkStream.DataAvailable)
                {
                    var buffer = new byte[4096];
                    int bytesRead = _networkStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"Received from Komorebi socket: {response}");
                    }
                }
                Thread.Sleep(100); // avoid busy loop
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading from socket: {ex.Message}");
            }
        }
    }

    private void ListenForKomorebiEvents()
    {
        _namedPipeServer.WaitForConnection();
        Console.WriteLine("Komorebi connected to named pipe.");
        
        using var streamReader = new StreamReader(_namedPipeServer, Encoding.UTF8);
        while (true)
        {
            try
            {
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
        _namedPipeServer.Dispose();
        _networkStream.Dispose();
    }
}
