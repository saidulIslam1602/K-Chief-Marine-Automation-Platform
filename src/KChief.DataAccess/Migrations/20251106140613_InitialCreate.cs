using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KChief.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alarms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Severity = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    VesselId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    EngineId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    SensorId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TriggeredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AcknowledgedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ClearedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ClearedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alarms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MessageBusEvents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageBusEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    MinValue = table.Column<double>(type: "REAL", nullable: false),
                    MaxValue = table.Column<double>(type: "REAL", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vessels",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", maxLength: 20, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Length = table.Column<double>(type: "REAL", nullable: false),
                    Width = table.Column<double>(type: "REAL", nullable: false),
                    MaxSpeed = table.Column<double>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vessels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Engines",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", maxLength: 20, nullable: false),
                    VesselId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RPM = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxRPM = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRunning = table.Column<bool>(type: "INTEGER", nullable: false),
                    Temperature = table.Column<double>(type: "REAL", nullable: false),
                    OilPressure = table.Column<double>(type: "REAL", nullable: false),
                    FuelConsumption = table.Column<double>(type: "REAL", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Engines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Engines_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Sensors",
                columns: new[] { "Id", "IsActive", "LastUpdated", "MaxValue", "MinValue", "Name", "Type", "Unit", "Value" },
                values: new object[,]
                {
                    { "sensor-001", true, new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(4999), 120.0, 0.0, "Engine Temperature Sensor 1", "Temperature", "Celsius", 85.5 },
                    { "sensor-002", true, new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(5002), 10.0, 0.0, "Engine Pressure Sensor 1", "Pressure", "Bar", 2.5 },
                    { "sensor-003", true, new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(5004), 50.0, 0.0, "Fuel Flow Sensor 1", "Flow", "L/min", 12.5 }
                });

            migrationBuilder.InsertData(
                table: "Vessels",
                columns: new[] { "Id", "CreatedAt", "LastUpdated", "Length", "Location", "MaxSpeed", "Name", "Status", "Type", "Width" },
                values: new object[,]
                {
                    { "vessel-001", new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(4886), new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(4882), 300.0, "Port of Rotterdam", 24.5, "MV Atlantic Explorer", 1, "Container Ship", 45.0 },
                    { "vessel-002", new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(4889), new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(4887), 280.0, "Port of Singapore", 22.0, "MV Pacific Navigator", 1, "Bulk Carrier", 42.0 }
                });

            migrationBuilder.InsertData(
                table: "Engines",
                columns: new[] { "Id", "FuelConsumption", "IsRunning", "LastUpdated", "MaxRPM", "Name", "OilPressure", "RPM", "Status", "Temperature", "Type", "VesselId" },
                values: new object[,]
                {
                    { "engine-001", 12.5, true, new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(4972), 1800, "Main Engine Port", 0.0, 1200, 2, 85.5, "Diesel", "vessel-001" },
                    { "engine-002", 12.800000000000001, true, new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(4977), 1800, "Main Engine Starboard", 0.0, 1180, 2, 87.200000000000003, "Diesel", "vessel-001" },
                    { "engine-003", 0.0, false, new DateTime(2025, 11, 6, 14, 6, 12, 903, DateTimeKind.Utc).AddTicks(4979), 1600, "Main Engine", 0.0, 0, 0, 25.0, "Diesel", "vessel-002" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alarms_Severity",
                table: "Alarms",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_Alarms_Status",
                table: "Alarms",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Alarms_TriggeredAt",
                table: "Alarms",
                column: "TriggeredAt");

            migrationBuilder.CreateIndex(
                name: "IX_Alarms_VesselId",
                table: "Alarms",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_Engines_Name",
                table: "Engines",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Engines_VesselId",
                table: "Engines",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageBusEvents_EventType",
                table: "MessageBusEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_MessageBusEvents_Source",
                table: "MessageBusEvents",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_MessageBusEvents_Timestamp",
                table: "MessageBusEvents",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_Name",
                table: "Sensors",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_Type",
                table: "Sensors",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_Name",
                table: "Vessels",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alarms");

            migrationBuilder.DropTable(
                name: "Engines");

            migrationBuilder.DropTable(
                name: "MessageBusEvents");

            migrationBuilder.DropTable(
                name: "Sensors");

            migrationBuilder.DropTable(
                name: "Vessels");
        }
    }
}
