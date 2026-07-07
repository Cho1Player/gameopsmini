using System.Net;
using System.Net.Http.Json;
using GameOpsMini.Shared.Models;

namespace GameOpsMini.Dashboard.Services;

public class GameOpsApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public GameOpsApiClient(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<ServerStatus>>
        GetServersAsync(
            CancellationToken cancellationToken = default)
    {
        var client =
            _httpClientFactory.CreateClient("GameOpsApi");

        var servers =
            await client.GetFromJsonAsync<List<ServerStatus>>(
                "/api/servers",
                cancellationToken);

        return servers ?? [];
    }

    public async Task<ServerStatus?>
        GetServerAsync(
            int id,
            CancellationToken cancellationToken = default)
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

    public async Task<IReadOnlyList<ServerStatusHistoryDto>>
        GetServerHistoryAsync(
            int id,
            CancellationToken cancellationToken = default)
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
}