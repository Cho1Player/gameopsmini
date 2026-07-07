using System.Net;
using System.Net.Http.Json;
using GameOpsMini.Shared.Models;

namespace GameOpsMini.Dashboard.Services;

public class GameOpsApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GameOpsApiClient> _logger;

    public GameOpsApiClient(
        IHttpClientFactory httpClientFactory,
        ILogger<GameOpsApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ServerStatus>>
        GetServersAsync(
            CancellationToken cancellationToken = default)
    {
        try
        {
            var client =
                _httpClientFactory.CreateClient("GameOpsApi");

            var servers =
                await client.GetFromJsonAsync<List<ServerStatus>>(
                    "/api/servers",
                    cancellationToken);

            return servers ?? [];
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new GameOpsApiException(
                "API 요청 시간이 초과되었습니다.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Failed to load server list.");

            throw new GameOpsApiException(
                "API 서버에 연결할 수 없습니다.",
                ex);
        }
    }

    public async Task<ServerStatus?>
        GetServerAsync(
            int id,
            CancellationToken cancellationToken = default)
    {
        try
        {
            var client =
                _httpClientFactory.CreateClient("GameOpsApi");

            using var response = await client.GetAsync(
                $"/api/servers/{id}",
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<ServerStatus>(
                    cancellationToken);
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new GameOpsApiException(
                "API 요청 시간이 초과되었습니다.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Failed to load server {ServerId}.",
                id);

            throw new GameOpsApiException(
                "API 서버에 연결할 수 없습니다.",
                ex);
        }
    }

    public async Task<IReadOnlyList<ServerStatusHistoryDto>>
        GetServerHistoryAsync(
            int id,
            CancellationToken cancellationToken = default)
    {
        try
        {
            var client =
                _httpClientFactory.CreateClient("GameOpsApi");

            using var response = await client.GetAsync(
                $"/api/servers/{id}/history",
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return [];
            }

            response.EnsureSuccessStatusCode();

            var histories = await response.Content
                .ReadFromJsonAsync<List<ServerStatusHistoryDto>>(
                    cancellationToken);

            return histories ?? [];
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new GameOpsApiException(
                "API 요청 시간이 초과되었습니다.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Failed to load history for server {ServerId}.",
                id);

            throw new GameOpsApiException(
                "API 서버에 연결할 수 없습니다.",
                ex);
        }
    }
}