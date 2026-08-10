using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaitifyApi.Migrations
{
    /// <inheritdoc />
    public partial class FKOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_businesses_users_OwnerId1",
                table: "businesses");

            migrationBuilder.DropIndex(
                name: "IX_businesses_OwnerId1",
                table: "businesses");

            migrationBuilder.DropColumn(
                name: "OwnerId1",
                table: "businesses");

            migrationBuilder.AlterColumn<string>(
                name: "owner_id",
                table: "businesses",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "logo",
                table: "businesses",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_businesses_owner_id",
                table: "businesses",
                column: "owner_id");

            migrationBuilder.AddForeignKey(
                name: "FK_businesses_users_owner_id",
                table: "businesses",
                column: "owner_id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_businesses_users_owner_id",
                table: "businesses");

            migrationBuilder.DropIndex(
                name: "IX_businesses_owner_id",
                table: "businesses");

            migrationBuilder.DropColumn(
                name: "logo",
                table: "businesses");

            migrationBuilder.AlterColumn<Guid>(
                name: "owner_id",
                table: "businesses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId1",
                table: "businesses",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_businesses_OwnerId1",
                table: "businesses",
                column: "OwnerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_businesses_users_OwnerId1",
                table: "businesses",
                column: "OwnerId1",
                principalTable: "users",
                principalColumn: "Id");
        }
    }
}
