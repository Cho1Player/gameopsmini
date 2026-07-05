using System.Text.Json;
using GameOpsMini.Api.Cache;
using GameOpsMini.Api.Data;
using GameOpsMini.Api.Entities;
using GameOpsMini.Shared.Models;
using GameOpsMini.Shared.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

var postgreSqlConnection =
    builder.Configuration.GetConnectionString("PostgreSql")
    ?? throw new InvalidOperationException(
        "PostgreSql connection string is missing.");

var redisConnection =
    builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException(
        "Redis connection string is missing.");

builder.Services.AddDbContext<GameOpsDbContext>(options =>
    options.UseNpgsql(postgreSqlConnection));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "GameOpsMini:";
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new
{
    Service = "GameOpsMini.Api",
    Status = "Running",
    Time = DateTime.UtcNow
}));

app.MapGet(
    "/api/servers",
    async (
        GameOpsDbContext dbContext,
        IDistributedCache cache,
        CancellationToken cancellationToken) =>
    {
        var monitoredServers = await dbContext.MonitoredServers
            .AsNoTracking()
            .OrderBy(server => server.Id)
            .ToListAsync(cancellationToken);

        var result = new List<ServerStatus>();

        foreach (var monitoredServer in monitoredServers)
        {
            var cacheKey =
                ServerStatusCacheKeys.GetServerStatusKey(monitoredServer.Id);

            var cachedJson = await cache.GetStringAsync(
                cacheKey,
                cancellationToken);

            ServerStatus? currentStatus = null;

            if (!string.IsNullOrWhiteSpace(cachedJson))
            {
                currentStatus =
                    JsonSerializer.Deserialize<ServerStatus>(cachedJson);
            }

            result.Add(currentStatus ?? new ServerStatus
            {
                Id = monitoredServer.Id,
                Name = monitoredServer.Name,
                Host = monitoredServer.Host,
                Port = monitoredServer.Port,
                State = ServerState.Unknown,
                LastCheckedAt = DateTime.MinValue,
                FailureCount = 0,
                Message = "No monitoring result is cached yet"
            });
        }

        return Results.Ok(result);
    });

app.MapGet(
    "/api/servers/{id:int}",
    async (
        int id,
        GameOpsDbContext dbContext,
        IDistributedCache cache,
        CancellationToken cancellationToken) =>
    {
        var monitoredServer = await dbContext.MonitoredServers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                server => server.Id == id,
                cancellationToken);

        if (monitoredServer is null)
        {
            return Results.NotFound(new
            {
                Message = $"Server id {id} not found"
            });
        }

        var cacheKey =
            ServerStatusCacheKeys.GetServerStatusKey(monitoredServer.Id);

        var cachedJson = await cache.GetStringAsync(
            cacheKey,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(cachedJson))
        {
            var cachedStatus =
                JsonSerializer.Deserialize<ServerStatus>(cachedJson);

            if (cachedStatus is not null)
            {
                return Results.Ok(cachedStatus);
            }
        }

        return Results.Ok(new ServerStatus
        {
            Id = monitoredServer.Id,
            Name = monitoredServer.Name,
            Host = monitoredServer.Host,
            Port = monitoredServer.Port,
            State = ServerState.Unknown,
            LastCheckedAt = DateTime.MinValue,
            FailureCount = 0,
            Message = "No monitoring result is cached yet"
        });
    });

app.MapPost(
    "/api/servers/{id:int}/status",
    async (
        int id,
        UpdateServerStatusRequest request,
        GameOpsDbContext dbContext,
        IDistributedCache cache,
        CancellationToken cancellationToken) =>
    {
        var monitoredServer = await dbContext.MonitoredServers
            .SingleOrDefaultAsync(
                server => server.Id == id,
                cancellationToken);

        if (monitoredServer is null)
        {
            return Results.NotFound(new
            {
                Message = $"Server id {id} not found"
            });
        }

        var checkedAt = request.LastCheckedAt == default
            ? DateTime.UtcNow
            : request.LastCheckedAt.ToUniversalTime();

        var history = new ServerStatusHistory
        {
            MonitoredServerId = monitoredServer.Id,
            State = request.State,
            CheckedAt = checkedAt,
            FailureCount = request.FailureCount,
            Message = request.Message
        };

        dbContext.ServerStatusHistories.Add(history);
        await dbContext.SaveChangesAsync(cancellationToken);

        var currentStatus = new ServerStatus
        {
            Id = monitoredServer.Id,
            Name = monitoredServer.Name,
            Host = monitoredServer.Host,
            Port = monitoredServer.Port,
            State = request.State,
            LastCheckedAt = checkedAt,
            FailureCount = request.FailureCount,
            Message = request.Message
        };

        var cacheKey =
            ServerStatusCacheKeys.GetServerStatusKey(monitoredServer.Id);

        var cacheJson = JsonSerializer.Serialize(currentStatus);

        await cache.SetStringAsync(
            cacheKey,
            cacheJson,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromMinutes(10)
            },
            cancellationToken);

        return Results.Ok(currentStatus);
    });

app.MapGet(
    "/api/servers/{id:int}/history",
    async (
        int id,
        GameOpsDbContext dbContext,
        CancellationToken cancellationToken) =>
    {
        var serverExists = await dbContext.MonitoredServers
            .AsNoTracking()
            .AnyAsync(
                server => server.Id == id,
                cancellationToken);

        if (!serverExists)
        {
            return Results.NotFound(new
            {
                Message = $"Server id {id} not found"
            });
        }

        var histories = await dbContext.ServerStatusHistories
            .AsNoTracking()
            .Where(history => history.MonitoredServerId == id)
            .OrderByDescending(history => history.CheckedAt)
            .Take(100)
            .Select(history => new
            {
                history.Id,
                ServerId = history.MonitoredServerId,
                history.State,
                history.CheckedAt,
                history.FailureCount,
                history.Message
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(histories);
    });

app.Run();