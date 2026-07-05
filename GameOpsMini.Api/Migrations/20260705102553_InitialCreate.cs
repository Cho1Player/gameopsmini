using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GameOpsMini.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "monitored_servers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Host = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monitored_servers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "server_status_histories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MonitoredServerId = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FailureCount = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_server_status_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_server_status_histories_monitored_servers_MonitoredServerId",
                        column: x => x.MonitoredServerId,
                        principalTable: "monitored_servers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "monitored_servers",
                columns: new[] { "Id", "CreatedAt", "Host", "Name", "Port" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 7, 2, 0, 0, 0, 0, DateTimeKind.Utc), "127.0.0.1", "DummyGameServer-1", 7777 },
                    { 2, new DateTime(2026, 7, 2, 0, 0, 0, 0, DateTimeKind.Utc), "127.0.0.1", "DummyGameServer-2", 7778 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_monitored_servers_Host_Port",
                table: "monitored_servers",
                columns: new[] { "Host", "Port" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_server_status_histories_MonitoredServerId_CheckedAt",
                table: "server_status_histories",
                columns: new[] { "MonitoredServerId", "CheckedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "server_status_histories");

            migrationBuilder.DropTable(
                name: "monitored_servers");
        }
    }
}
