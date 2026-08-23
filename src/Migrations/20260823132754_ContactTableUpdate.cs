using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaitifyApi.Migrations
{
    /// <inheritdoc />
    public partial class ContactTableUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QueueEntries_BusinessQrCodeToken_Status",
                table: "QueueEntries");

            migrationBuilder.AddColumn<bool>(
                name: "Checked",
                table: "Contacts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Checked",
                table: "Contacts");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_BusinessQrCodeToken_Status",
                table: "QueueEntries",
                columns: new[] { "BusinessQrCodeToken", "Status" });
        }
    }
}
