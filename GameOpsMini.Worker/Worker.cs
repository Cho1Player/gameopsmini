using System.Net.Sockets;
using System.Net.Http.Json;
using GameOpsMini.Shared.Models;

namespace GameOpsMini.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public Worker(
        ILogger<Worker> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = _configuration.GetValue<int>("Monitor:IntervalSeconds", 5);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckServersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Server monitoring failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }

    private async Task CheckServersAsync(CancellationToken cancellationToken)
    {
        var apiBaseUrl = _configuration["Api:BaseUrl"] ?? "http://localhost:5000";
        var httpClient = _httpClientFactory.CreateClient();

        var servers = await httpClient.GetFromJsonAsync<List<ServerStatus>>(
            $"{apiBaseUrl}/api/servers",
            cancellationToken);

        if (servers is null)
        {
            _logger.LogWarning("Server list is empty");
            return;
        }

        foreach (var server in servers)
        {
            var isUp = await CheckTcpPortAsync(server.Host, server.Port, cancellationToken);

            server.State = isUp ? ServerState.Up : ServerState.Down;
            server.LastCheckedAt = DateTime.UtcNow;
            server.FailureCount = isUp ? 0 : server.FailureCount + 1;
            server.Message = isUp
                ? "TCP port is reachable"
                : "TCP port is not reachable";

            await httpClient.PostAsJsonAsync(
                $"{apiBaseUrl}/api/servers/{server.Id}/status",
                server,
                cancellationToken);

            _logger.LogInformation(
                "Checked {Name} {Host}:{Port} => {State}",
                server.Name,
                server.Host,
                server.Port,
                server.State);
        }
    }

    private static async Task<bool> CheckTcpPortAsync(
        string host,
        int port,
        CancellationToken cancellationToken)
    {
        try
        {
            using var client = new TcpClient();

            var connectTask = client.ConnectAsync(host, port, cancellationToken).AsTask();
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);

            return completedTask == connectTask && client.Connected;
        }
        catch
        {
            return false;
        }
    }
}