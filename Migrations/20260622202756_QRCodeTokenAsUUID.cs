using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaitifyApi.Migrations
{
    /// <inheritdoc />
    public partial class QRCodeTokenAsUUID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QueueEntries_Businesses_BusinessId",
                table: "QueueEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_SmsLogs_Businesses_BusinessId",
                table: "SmsLogs");

            migrationBuilder.RenameColumn(
                name: "BusinessId",
                table: "SmsLogs",
                newName: "BusinessQrCodeToken");

            migrationBuilder.RenameIndex(
                name: "IX_SmsLogs_BusinessId",
                table: "SmsLogs",
                newName: "IX_SmsLogs_BusinessQrCodeToken");

            migrationBuilder.RenameColumn(
                name: "BusinessId",
                table: "QueueEntries",
                newName: "BusinessQrCodeToken");

            migrationBuilder.RenameIndex(
                name: "IX_QueueEntries_BusinessId_Status",
                table: "QueueEntries",
                newName: "IX_QueueEntries_BusinessQrCodeToken_Status");

            migrationBuilder.RenameIndex(
                name: "IX_QueueEntries_BusinessId",
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
                newName: "BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_SmsLogs_BusinessQrCodeToken",
                table: "SmsLogs",
                newName: "IX_SmsLogs_BusinessId");

            migrationBuilder.RenameColumn(
                name: "BusinessQrCodeToken",
                table: "QueueEntries",
                newName: "BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_QueueEntries_BusinessQrCodeToken_Status",
                table: "QueueEntries",
                newName: "IX_QueueEntries_BusinessId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_QueueEntries_BusinessQrCodeToken",
                table: "QueueEntries",
                newName: "IX_QueueEntries_BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_QueueEntries_Businesses_BusinessId",
                table: "QueueEntries",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SmsLogs_Businesses_BusinessId",
                table: "SmsLogs",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
