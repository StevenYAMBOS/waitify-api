using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaitifyApi.Migrations
{
    /// <inheritdoc />
    public partial class QueueEntriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QueueEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    Phone = table.Column<string>(type: "varchar(20)", nullable: false),
                    ClientName = table.Column<string>(type: "varchar(100)", nullable: true),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    EstimatedWaitTime = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", nullable: true),
                    CalledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ServedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualServiceTime = table.Column<int>(type: "integer", nullable: false),
                    SmsSentCount = table.Column<int>(type: "integer", nullable: false),
                    LastSmsSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueueEntries_businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "businesses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_BusinessId",
                table: "QueueEntries",
                column: "BusinessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QueueEntries");
        }
    }
}
