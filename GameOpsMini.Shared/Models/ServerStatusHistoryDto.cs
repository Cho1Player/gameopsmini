namespace GameOpsMini.Shared.Models;

public class ServerStatusHistoryDto
{
    public long Id { get; set; }

    public int ServerId { get; set; }

    public ServerState State { get; set; }

    public DateTime CheckedAt { get; set; }

    public int FailureCount { get; set; }

    public string? Message { get; set; }
}