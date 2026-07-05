using GameOpsMini.Shared.Models;

namespace GameOpsMini.Shared.Requests;

public class UpdateServerStatusRequest
{
    public ServerState State { get; set; }

    public DateTime LastCheckedAt { get; set; }

    public int FailureCount { get; set; }

    public string? Message { get; set; }
}