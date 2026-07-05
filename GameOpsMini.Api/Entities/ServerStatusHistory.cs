using GameOpsMini.Shared.Models;

namespace GameOpsMini.Api.Entities;

public class ServerStatusHistory
{
    public long Id { get; set; }

    public int MonitoredServerId { get; set; }

    public ServerState State { get; set; }

    public DateTime CheckedAt { get; set; }

    public int FailureCount { get; set; }

    public string? Message { get; set; }

    public MonitoredServer MonitoredServer { get; set; } = null!;
}