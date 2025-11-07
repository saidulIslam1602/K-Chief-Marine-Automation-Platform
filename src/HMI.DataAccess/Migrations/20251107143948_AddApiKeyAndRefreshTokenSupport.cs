using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddApiKeyAndRefreshTokenSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EscalationLevel",
                table: "Alarms",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GroupId",
                table: "Alarms",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RuleId",
                table: "Alarms",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SourceValue",
                table: "Alarms",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ThresholdValue",
                table: "Alarms",
                type: "REAL",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    KeyHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    OwnerId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    OwnerType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Scopes = table.Column<string>(type: "TEXT", nullable: false),
                    AllowedIPs = table.Column<string>(type: "TEXT", nullable: false),
                    RateLimitPerMinute = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UsageCount = table.Column<long>(type: "INTEGER", nullable: false),
                    Metadata = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TokenHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    JwtId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RevokedReason = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedByIp = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    UsedByIp = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    RevokedByIp = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    ReplacedByToken = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    JobTitle = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    EmailVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Engines",
                keyColumn: "Id",
                keyValue: "engine-001",
                column: "LastUpdated",
                value: new DateTime(2025, 11, 7, 14, 39, 48, 363, DateTimeKind.Utc).AddTicks(877));

            migrationBuilder.UpdateData(
                table: "Engines",
                keyColumn: "Id",
                keyValue: "engine-002",
                column: "LastUpdated",
                value: new DateTime(2025, 11, 7, 14, 39, 48, 363, DateTimeKind.Utc).AddTicks(882));

            migrationBuilder.UpdateData(
                table: "Engines",
                keyColumn: "Id",
                keyValue: "engine-003",
                column: "LastUpdated",
                value: new DateTime(2025, 11, 7, 14, 39, 48, 363, DateTimeKind.Utc).AddTicks(884));

            migrationBuilder.UpdateData(
                table: "Sensors",
                keyColumn: "Id",
                keyValue: "sensor-001",
                column: "LastUpdated",
                value: new DateTime(2025, 11, 7, 14, 39, 48, 363, DateTimeKind.Utc).AddTicks(938));

            migrationBuilder.UpdateData(
                table: "Sensors",
                keyColumn: "Id",
                keyValue: "sensor-002",
                column: "LastUpdated",
                value: new DateTime(2025, 11, 7, 14, 39, 48, 363, DateTimeKind.Utc).AddTicks(940));

            migrationBuilder.UpdateData(
                table: "Sensors",
                keyColumn: "Id",
                keyValue: "sensor-003",
                column: "LastUpdated",
                value: new DateTime(2025, 11, 7, 14, 39, 48, 363, DateTimeKind.Utc).AddTicks(942));

            migrationBuilder.UpdateData(
                table: "Vessels",
                keyColumn: "Id",
                keyValue: "vessel-001",
                columns: new[] { "CreatedAt", "LastUpdated" },
                values: new object[] { new DateTime(2025, 11, 7, 14, 39, 48, 363, DateTimeKind.Utc).AddTicks(775), new DateTime(2025, 11, 7, 14, 39, 48, 363, DateTimeKind.Utc).AddTicks(771) });

            migrationBuilder.UpdateData(
                table: "Vessels",
                keyColumn: "Id",
                keyValue: "vessel-002",
                columns: new[] { "CreatedAt", "LastUpdated" },
                values: new object[] { new DateTime(2025, 11, 7, 14, 39, 48, 363, DateTimeKind.Utc).AddTicks(778), new DateTime(2025, 11, 7, 14, 39, 48, 363, DateTimeKind.Utc).AddTicks(776) });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_ExpiresAt",
                table: "ApiKeys",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_IsActive",
                table: "ApiKeys",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_KeyHash",
                table: "ApiKeys",
                column: "KeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_OwnerId",
                table: "ApiKeys",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_JwtId",
                table: "RefreshTokens",
                column: "JwtId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_RevokedAt",
                table: "RefreshTokens",
                column: "RevokedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UsedAt",
                table: "RefreshTokens",
                column: "UsedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropColumn(
                name: "EscalationLevel",
                table: "Alarms");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Alarms");

            migrationBuilder.DropColumn(
                name: "RuleId",
                table: "Alarms");

            migrationBuilder.DropColumn(
                name: "SourceValue",
                table: "Alarms");

            migrationBuilder.DropColumn(
                name: "ThresholdValue",
                table: "Alarms");

            migrationBuilder.UpdateData(
                table: "Engines",
                keyColumn: "Id",
                keyValue: "engine-001",
                column: "LastUpdated",
                value: new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(4972));

            migrationBuilder.UpdateData(
                table: "Engines",
                keyColumn: "Id",
                keyValue: "engine-002",
                column: "LastUpdated",
                value: new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(4977));

            migrationBuilder.UpdateData(
                table: "Engines",
                keyColumn: "Id",
                keyValue: "engine-003",
                column: "LastUpdated",
                value: new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(4979));

            migrationBuilder.UpdateData(
                table: "Sensors",
                keyColumn: "Id",
                keyValue: "sensor-001",
                column: "LastUpdated",
                value: new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(4999));

            migrationBuilder.UpdateData(
                table: "Sensors",
                keyColumn: "Id",
                keyValue: "sensor-002",
                column: "LastUpdated",
                value: new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(5002));

            migrationBuilder.UpdateData(
                table: "Sensors",
                keyColumn: "Id",
                keyValue: "sensor-003",
                column: "LastUpdated",
                value: new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(5004));

            migrationBuilder.UpdateData(
                table: "Vessels",
                keyColumn: "Id",
                keyValue: "vessel-001",
                columns: new[] { "CreatedAt", "LastUpdated" },
                values: new object[] { new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(4886), new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(4882) });

            migrationBuilder.UpdateData(
                table: "Vessels",
                keyColumn: "Id",
                keyValue: "vessel-002",
                columns: new[] { "CreatedAt", "LastUpdated" },
                values: new object[] { new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(4889), new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(4887) });
        }
    }
}
