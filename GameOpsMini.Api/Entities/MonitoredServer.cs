namespace GameOpsMini.Api.Entities;

public class MonitoredServer
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ServerStatusHistory> StatusHistories { get; set; }
        = new List<ServerStatusHistory>();
}