using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaitifyApi.Migrations
{
    /// <inheritdoc />
    public partial class CompleteDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "QueueEntries",
                type: "varchar(50)",
                nullable: true,
                defaultValue: "waiting",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "SmsLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    QueueEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Phone = table.Column<string>(type: "varchar(20)", nullable: false),
                    MessageType = table.Column<string>(type: "varchar(50)", nullable: true),
                    MessageContent = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValue: "pending"),
                    ProviderResponse = table.Column<string>(type: "jsonb", nullable: true),
                    CostCents = table.Column<int>(type: "integer", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsLogs", x => x.Id);
                    table.CheckConstraint("CK_SmsLogs_Status", "\"Status\" IN ('pending', 'sent', 'failed')");
                    table.ForeignKey(
                        name: "FK_SmsLogs_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SmsLogs_QueueEntries_QueueEntryId",
                        column: x => x.QueueEntryId,
                        principalTable: "QueueEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    PriceCents = table.Column<int>(type: "integer", nullable: false),
                    MaxBusinesses = table.Column<int>(type: "integer", nullable: false),
                    SmsQuotaMonthly = table.Column<int>(type: "integer", nullable: false),
                    Features = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                    table.CheckConstraint("CK_SubscriptionPlans_MaxBusinesses", "\"MaxBusinesses\" = -1 OR \"MaxBusinesses\" > 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_SubsriptionId",
                table: "Users",
                column: "SubsriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SubsriptionStatus",
                table: "Users",
                column: "SubsriptionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_BusinessId_Status",
                table: "QueueEntries",
                columns: new[] { "BusinessId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_QueueEntries_Status",
                table: "QueueEntries",
                column: "Status");

            migrationBuilder.AddCheckConstraint(
                name: "CK_QueueEntries_Status",
                table: "QueueEntries",
                sql: "\"Status\" IN ('waiting', 'called', 'served', 'missed', 'cancelled')");

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_IsActive",
                table: "Businesses",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_QrCodeToken",
                table: "Businesses",
                column: "QrCodeToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_BusinessId",
                table: "SmsLogs",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_QueueEntryId",
                table: "SmsLogs",
                column: "QueueEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_SentAt",
                table: "SmsLogs",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_Status",
                table: "SmsLogs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_Name",
                table: "SubscriptionPlans",
                column: "Name",
                unique: true);

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION update_updated_at_column()
                RETURNS TRIGGER AS $$
                BEGIN
                    NEW.""UpdatedAt"" = NOW();
                    RETURN NEW;
                END;
                $$ language 'plpgsql';

                CREATE TRIGGER trigger_users_updated_at
                    BEFORE UPDATE ON ""Users""
                    FOR EACH ROW EXECUTE PROCEDURE update_updated_at_column();

                CREATE TRIGGER trigger_businesses_updated_at
                    BEFORE UPDATE ON ""Businesses""
                    FOR EACH ROW EXECUTE PROCEDURE update_updated_at_column();

                CREATE TRIGGER trigger_queue_entries_updated_at
                    BEFORE UPDATE ON ""QueueEntries""
                    FOR EACH ROW EXECUTE PROCEDURE update_updated_at_column();

                CREATE TRIGGER trigger_subscription_plans_updated_at
                    BEFORE UPDATE ON ""SubscriptionPlans""
                    FOR EACH ROW EXECUTE PROCEDURE update_updated_at_column();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SmsLogs");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_SubsriptionId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_SubsriptionStatus",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_QueueEntries_BusinessId_Status",
                table: "QueueEntries");

            migrationBuilder.DropIndex(
                name: "IX_QueueEntries_Status",
                table: "QueueEntries");

            migrationBuilder.DropCheckConstraint(
                name: "CK_QueueEntries_Status",
                table: "QueueEntries");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_IsActive",
                table: "Businesses");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_QrCodeToken",
                table: "Businesses");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "QueueEntries",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true,
                oldDefaultValue: "waiting");

            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS trigger_users_updated_at ON ""Users"";
                DROP TRIGGER IF EXISTS trigger_businesses_updated_at ON ""Businesses"";
                DROP TRIGGER IF EXISTS trigger_queue_entries_updated_at ON ""QueueEntries"";
                DROP TRIGGER IF EXISTS trigger_subscription_plans_updated_at ON ""SubscriptionPlans"";
                DROP FUNCTION IF EXISTS update_updated_at_column();
            ");
        }
    }
}
