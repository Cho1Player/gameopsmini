using GameOpsMini.Shared.Models;
using GameOpsMini.Api.Data;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddSingleton<List<ServerStatus>>(_ =>
[
    new ServerStatus
    {
        Id = 1,
        Name = "DummyGameServer-1",
        Host = "127.0.0.1",
        Port = 7777,
        State = ServerState.Unknown,
        LastCheckedAt = DateTime.UtcNow,
        FailureCount = 0,
        Message = "Not checked yet"
    },
    new ServerStatus
    {
        Id = 2,
        Name = "DummyGameServer-2",
        Host = "127.0.0.1",
        Port = 7778,
        State = ServerState.Unknown,
        LastCheckedAt = DateTime.UtcNow,
        FailureCount = 0,
        Message = "Not checked yet"
    }
]);

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new
{
    Service = "GameOpsMini.Api",
    Status = "Running",
    Time = DateTime.UtcNow
}));

app.MapGet("/api/servers", (List<ServerStatus> servers) =>
{
    return Results.Ok(servers);
});

app.MapGet("/api/servers/{id:int}", (int id, List<ServerStatus> servers) =>
{
    var server = servers.FirstOrDefault(x => x.Id == id);

    return server is null
        ? Results.NotFound(new { Message = $"Server id {id} not found" })
        : Results.Ok(server);
});

app.MapPost("/api/servers/{id:int}/status", (int id, ServerStatus updatedStatus, List<ServerStatus> servers) =>
{
    var server = servers.FirstOrDefault(x => x.Id == id);

    if (server is null)
    {
        return Results.NotFound(new { Message = $"Server id {id} not found" });
    }

    server.State = updatedStatus.State;
    server.LastCheckedAt = DateTime.UtcNow;
    server.FailureCount = updatedStatus.FailureCount;
    server.Message = updatedStatus.Message;

    return Results.Ok(server);
});

app.Run();