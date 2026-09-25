using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaitifyApi.Migrations
{
    /// <inheritdoc />
    public partial class QueueOpenedAtToNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ServedAt",
                table: "QueueEntries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "QueueOpeneddAt",
                table: "Businesses",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_BusinessQrCodeToken_Status",
                table: "QueueEntries",
                columns: new[] { "BusinessQrCodeToken", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QueueEntries_BusinessQrCodeToken_Status",
                table: "QueueEntries");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ServedAt",
                table: "QueueEntries",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "QueueOpeneddAt",
                table: "Businesses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
