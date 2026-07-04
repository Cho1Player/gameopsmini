namespace GameOpsMini.Shared.Models;

public enum ServerState
{
    Unknown = 0,
    Up = 1,
    Down = 2
}

public class ServerStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public ServerState State { get; set; } = ServerState.Unknown;
    public DateTime LastCheckedAt { get; set; }
    public int FailureCount { get; set; }
    public string? Message { get; set; }
}