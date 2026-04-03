using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaitifyApi.Migrations
{
    /// <inheritdoc />
    public partial class QueueEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "queue_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    business_id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone = table.Column<string>(type: "varchar(20)", nullable: false),
                    client_name = table.Column<string>(type: "varchar(100)", nullable: true),
                    position = table.Column<int>(type: "integer", nullable: false),
                    estimated_wait_time = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "varchar(50)", nullable: true),
                    called_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    served_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    actual_service_time = table.Column<int>(type: "integer", nullable: false),
                    sms_sent_count = table.Column<int>(type: "integer", nullable: false),
                    last_sms_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queue_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_queue_entries_businesses_business_id",
                        column: x => x.business_id,
                        principalTable: "businesses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_queue_entries_business_id",
                table: "queue_entries",
                column: "business_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "queue_entries");
        }
    }
}
