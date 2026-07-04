using System.Net;
using System.Net.Sockets;
using System.Text;

var portText = Environment.GetEnvironmentVariable("PORT") ?? "7777";

if (!int.TryParse(portText, out var port))
{
    Console.Error.WriteLine($"Invalid PORT value: {portText}");
    return;
}

var listener = new TcpListener(IPAddress.Any, port);
/// TcpListener는 지정한 포트에서 TCP 연결 요청을 수신하고, AcceptTcpClientAsync()로 연결을 비동기적으로 받음.

try
{
    listener.Start();

    Console.WriteLine($"Dummy TCP server started.");
    Console.WriteLine($"Listening on port {port}.");
    Console.WriteLine("Press Ctrl+C to stop.");

    while (true)
    {
        using var client = await listener.AcceptTcpClientAsync();

        Console.WriteLine(
            $"Client connected: {client.Client.RemoteEndPoint}");

        await using var stream = client.GetStream();

        var response = Encoding.UTF8.GetBytes("OK\n");
        await stream.WriteAsync(response);
    }
}
catch (SocketException ex)
{
    Console.Error.WriteLine($"TCP server error: {ex.Message}");
}
finally
{
    listener.Stop();
}