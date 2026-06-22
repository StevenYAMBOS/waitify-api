using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaitifyApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQRCodeForId : Migration
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
                newName: "QrCodeToken");

            migrationBuilder.RenameIndex(
                name: "IX_SmsLogs_BusinessId",
                table: "SmsLogs",
                newName: "IX_SmsLogs_QrCodeToken");

            migrationBuilder.RenameColumn(
                name: "BusinessId",
                table: "QueueEntries",
                newName: "QrCodeToken");

            migrationBuilder.RenameIndex(
                name: "IX_QueueEntries_BusinessId_Status",
                table: "QueueEntries",
                newName: "IX_QueueEntries_QrCodeToken_Status");

            migrationBuilder.RenameIndex(
                name: "IX_QueueEntries_BusinessId",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                newName: "BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_SmsLogs_QrCodeToken",
                table: "SmsLogs",
                newName: "IX_SmsLogs_BusinessId");

            migrationBuilder.RenameColumn(
                name: "QrCodeToken",
                table: "QueueEntries",
                newName: "BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_QueueEntries_QrCodeToken_Status",
                table: "QueueEntries",
                newName: "IX_QueueEntries_BusinessId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_QueueEntries_QrCodeToken",
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
