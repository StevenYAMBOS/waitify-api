using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaitifyApi.Migrations
{
    /// <inheritdoc />
    public partial class PascalCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_roles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_users_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_users_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_roles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_users_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_users_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_businesses_users_owner_id",
                table: "businesses");

            migrationBuilder.DropForeignKey(
                name: "FK_QueueEntries_businesses_BusinessId",
                table: "QueueEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_roles",
                table: "roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_businesses",
                table: "businesses");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "roles",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "businesses",
                newName: "Businesses");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "Users",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Users",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "trial_ends_at",
                table: "Users",
                newName: "TrialEndsAt");

            migrationBuilder.RenameColumn(
                name: "subsription_status",
                table: "Users",
                newName: "SubsriptionStatus");

            migrationBuilder.RenameColumn(
                name: "subsription_id",
                table: "Users",
                newName: "SubsriptionId");

            migrationBuilder.RenameColumn(
                name: "profile_picture",
                table: "Users",
                newName: "ProfilePicture");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "Users",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "last_login",
                table: "Users",
                newName: "LastLogin");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "Users",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Businesses",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "logo",
                table: "Businesses",
                newName: "Logo");

            migrationBuilder.RenameColumn(
                name: "country",
                table: "Businesses",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "city",
                table: "Businesses",
                newName: "City");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "Businesses",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Businesses",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "zip_code",
                table: "Businesses",
                newName: "ZipCode");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Businesses",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "sms_notifications_enabled",
                table: "Businesses",
                newName: "SmsNotificationsEnabled");

            migrationBuilder.RenameColumn(
                name: "qr_code_token",
                table: "Businesses",
                newName: "QrCodeToken");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                table: "Businesses",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "owner_id",
                table: "Businesses",
                newName: "OwnerId");

            migrationBuilder.RenameColumn(
                name: "opening_hours",
                table: "Businesses",
                newName: "OpeningHours");

            migrationBuilder.RenameColumn(
                name: "max_queue_size",
                table: "Businesses",
                newName: "MaxQueueSize");

            migrationBuilder.RenameColumn(
                name: "is_queue_paused",
                table: "Businesses",
                newName: "IsQueuePaused");

            migrationBuilder.RenameColumn(
                name: "is_queue_active",
                table: "Businesses",
                newName: "IsQueueActive");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "Businesses",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "custom_message",
                table: "Businesses",
                newName: "CustomMessage");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Businesses",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "client_timeout_minutes",
                table: "Businesses",
                newName: "ClientTimeoutMinutes");

            migrationBuilder.RenameColumn(
                name: "business_type",
                table: "Businesses",
                newName: "BusinessType");

            migrationBuilder.RenameColumn(
                name: "average_service_time",
                table: "Businesses",
                newName: "AverageServiceTime");

            migrationBuilder.RenameColumn(
                name: "auto_advance_enabled",
                table: "Businesses",
                newName: "AutoAdvanceEnabled");

            migrationBuilder.RenameIndex(
                name: "IX_businesses_owner_id",
                table: "Businesses",
                newName: "IX_Businesses_OwnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Businesses",
                table: "Businesses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_Roles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_Users_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_Users_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_Roles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_Users_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_Users_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_Users_OwnerId",
                table: "Businesses",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QueueEntries_Businesses_BusinessId",
                table: "QueueEntries",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_Roles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_Users_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_Users_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_Roles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_Users_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_Users_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_Users_OwnerId",
                table: "Businesses");

            migrationBuilder.DropForeignKey(
                name: "FK_QueueEntries_Businesses_BusinessId",
                table: "QueueEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Businesses",
                table: "Businesses");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "roles");

            migrationBuilder.RenameTable(
                name: "Businesses",
                newName: "businesses");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "users",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "users",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TrialEndsAt",
                table: "users",
                newName: "trial_ends_at");

            migrationBuilder.RenameColumn(
                name: "SubsriptionStatus",
                table: "users",
                newName: "subsription_status");

            migrationBuilder.RenameColumn(
                name: "SubsriptionId",
                table: "users",
                newName: "subsription_id");

            migrationBuilder.RenameColumn(
                name: "ProfilePicture",
                table: "users",
                newName: "profile_picture");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "users",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "LastLogin",
                table: "users",
                newName: "last_login");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "users",
                newName: "first_name");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "users",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "businesses",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Logo",
                table: "businesses",
                newName: "logo");

            migrationBuilder.RenameColumn(
                name: "Country",
                table: "businesses",
                newName: "country");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "businesses",
                newName: "city");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "businesses",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "businesses",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ZipCode",
                table: "businesses",
                newName: "zip_code");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "businesses",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "SmsNotificationsEnabled",
                table: "businesses",
                newName: "sms_notifications_enabled");

            migrationBuilder.RenameColumn(
                name: "QrCodeToken",
                table: "businesses",
                newName: "qr_code_token");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "businesses",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "businesses",
                newName: "owner_id");

            migrationBuilder.RenameColumn(
                name: "OpeningHours",
                table: "businesses",
                newName: "opening_hours");

            migrationBuilder.RenameColumn(
                name: "MaxQueueSize",
                table: "businesses",
                newName: "max_queue_size");

            migrationBuilder.RenameColumn(
                name: "IsQueuePaused",
                table: "businesses",
                newName: "is_queue_paused");

            migrationBuilder.RenameColumn(
                name: "IsQueueActive",
                table: "businesses",
                newName: "is_queue_active");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "businesses",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CustomMessage",
                table: "businesses",
                newName: "custom_message");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "businesses",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ClientTimeoutMinutes",
                table: "businesses",
                newName: "client_timeout_minutes");

            migrationBuilder.RenameColumn(
                name: "BusinessType",
                table: "businesses",
                newName: "business_type");

            migrationBuilder.RenameColumn(
                name: "AverageServiceTime",
                table: "businesses",
                newName: "average_service_time");

            migrationBuilder.RenameColumn(
                name: "AutoAdvanceEnabled",
                table: "businesses",
                newName: "auto_advance_enabled");

            migrationBuilder.RenameIndex(
                name: "IX_Businesses_OwnerId",
                table: "businesses",
                newName: "IX_businesses_owner_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_roles",
                table: "roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_businesses",
                table: "businesses",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_roles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_users_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_users_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_roles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_users_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_users_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_businesses_users_owner_id",
                table: "businesses",
                column: "owner_id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QueueEntries_businesses_BusinessId",
                table: "QueueEntries",
                column: "BusinessId",
                principalTable: "businesses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
