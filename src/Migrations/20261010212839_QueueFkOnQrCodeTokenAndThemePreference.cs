using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaitifyApi.Migrations
{
    /// <inheritdoc />
    public partial class QueueFkOnQrCodeTokenAndThemePreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QueueEntries_Businesses_BusinessQrCodeToken",
                table: "QueueEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_SmsLogs_Businesses_BusinessQrCodeToken",
                table: "SmsLogs");

            migrationBuilder.AddColumn<string>(
                name: "ThemePreference",
                table: "Users",
                type: "varchar(10)",
                nullable: false,
                defaultValue: "System");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Businesses_QrCodeToken",
                table: "Businesses",
                column: "QrCodeToken");

            migrationBuilder.AddForeignKey(
                name: "FK_QueueEntries_Businesses_BusinessQrCodeToken",
                table: "QueueEntries",
                column: "BusinessQrCodeToken",
                principalTable: "Businesses",
                principalColumn: "QrCodeToken",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SmsLogs_Businesses_BusinessQrCodeToken",
                table: "SmsLogs",
                column: "BusinessQrCodeToken",
                principalTable: "Businesses",
                principalColumn: "QrCodeToken",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QueueEntries_Businesses_BusinessQrCodeToken",
                table: "QueueEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_SmsLogs_Businesses_BusinessQrCodeToken",
                table: "SmsLogs");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Businesses_QrCodeToken",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "ThemePreference",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_QueueEntries_Businesses_BusinessQrCodeToken",
                table: "QueueEntries",
                column: "BusinessQrCodeToken",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SmsLogs_Businesses_BusinessQrCodeToken",
                table: "SmsLogs",
                column: "BusinessQrCodeToken",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
