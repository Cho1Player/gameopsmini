using GameOpsMini.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameOpsMini.Api.Data;

public class GameOpsDbContext : DbContext
{
    public GameOpsDbContext(DbContextOptions<GameOpsDbContext> options)
        : base(options)
    {
    }

    public DbSet<MonitoredServer> MonitoredServers =>
        Set<MonitoredServer>();

    public DbSet<ServerStatusHistory> ServerStatusHistories =>
        Set<ServerStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MonitoredServer>(entity =>
        {
            entity.ToTable("monitored_servers");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Host)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.Port)
                .IsRequired();

            entity.HasIndex(x => new { x.Host, x.Port })
                .IsUnique();

            entity.HasData(
                new MonitoredServer
                {
                    Id = 1,
                    Name = "DummyGameServer-1",
                    Host = "127.0.0.1",
                    Port = 7777,
                    CreatedAt = new DateTime(
                        2026, 7, 2, 0, 0, 0, DateTimeKind.Utc)
                },
                new MonitoredServer
                {
                    Id = 2,
                    Name = "DummyGameServer-2",
                    Host = "127.0.0.1",
                    Port = 7778,
                    CreatedAt = new DateTime(
                        2026, 7, 2, 0, 0, 0, DateTimeKind.Utc)
                });
        });

        modelBuilder.Entity<ServerStatusHistory>(entity =>
        {
            entity.ToTable("server_status_histories");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Message)
                .HasMaxLength(500);

            entity.HasIndex(x => new
            {
                x.MonitoredServerId,
                x.CheckedAt
            });

            entity.HasOne(x => x.MonitoredServer)
                .WithMany(x => x.StatusHistories)
                .HasForeignKey(x => x.MonitoredServerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}