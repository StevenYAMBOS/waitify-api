using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaitifyApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBusinessQRCodeForId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QueueEntries_Businesses_QrCodeToken",
                table: "QueueEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_SmsLogs_Businesses_QrCodeToken",
                table: "SmsLogs");

            migrationBuilder.RenameColumn(
                name: "QrCodeToken",
                table: "SmsLogs",
                newName: "BusinessQrCodeToken");

            migrationBuilder.RenameIndex(
                name: "IX_SmsLogs_QrCodeToken",
                table: "SmsLogs",
                newName: "IX_SmsLogs_BusinessQrCodeToken");

            migrationBuilder.RenameColumn(
                name: "QrCodeToken",
                table: "QueueEntries",
                newName: "BusinessQrCodeToken");

            migrationBuilder.RenameIndex(
                name: "IX_QueueEntries_QrCodeToken_Status",
                table: "QueueEntries",
                newName: "IX_QueueEntries_BusinessQrCodeToken_Status");

            migrationBuilder.RenameIndex(
                name: "IX_QueueEntries_QrCodeToken",
                table: "QueueEntries",
                newName: "IX_QueueEntries_BusinessQrCodeToken");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QueueEntries_Businesses_BusinessQrCodeToken",
                table: "QueueEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_SmsLogs_Businesses_BusinessQrCodeToken",
                table: "SmsLogs");

            migrationBuilder.RenameColumn(
                name: "BusinessQrCodeToken",
                table: "SmsLogs",
                newName: "QrCodeToken");

            migrationBuilder.RenameIndex(
                name: "IX_SmsLogs_BusinessQrCodeToken",
                table: "SmsLogs",
                newName: "IX_SmsLogs_QrCodeToken");

            migrationBuilder.RenameColumn(
                name: "BusinessQrCodeToken",
                table: "QueueEntries",
                newName: "QrCodeToken");

            migrationBuilder.RenameIndex(
                name: "IX_QueueEntries_BusinessQrCodeToken_Status",
                table: "QueueEntries",
                newName: "IX_QueueEntries_QrCodeToken_Status");

            migrationBuilder.RenameIndex(
                name: "IX_QueueEntries_BusinessQrCodeToken",
                table: "QueueEntries",
                newName: "IX_QueueEntries_QrCodeToken");

            migrationBuilder.AddForeignKey(
                name: "FK_QueueEntries_Businesses_QrCodeToken",
                table: "QueueEntries",
                column: "QrCodeToken",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SmsLogs_Businesses_QrCodeToken",
                table: "SmsLogs",
                column: "QrCodeToken",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
