using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kurvenanzeige.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalogReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TagName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DbNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Offset = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<float>(type: "REAL", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Quality = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalogReadings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataBlockReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TagName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DbNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    StructureJson = table.Column<string>(type: "TEXT", nullable: false),
                    Quality = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataBlockReadings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataPointConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TagName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DataType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DbNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Offset = table.Column<int>(type: "INTEGER", nullable: false),
                    Bit = table.Column<int>(type: "INTEGER", nullable: true),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    MinValue = table.Column<float>(type: "REAL", nullable: true),
                    MaxValue = table.Column<float>(type: "REAL", nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    PollingInterval = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataPointConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DigitalReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TagName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DbNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Offset = table.Column<int>(type: "INTEGER", nullable: false),
                    Bit = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<bool>(type: "INTEGER", nullable: false),
                    Quality = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalReadings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalogReadings_TagName_Timestamp",
                table: "AnalogReadings",
                columns: new[] { "TagName", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalogReadings_Timestamp",
                table: "AnalogReadings",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_DataBlockReadings_TagName_Timestamp",
                table: "DataBlockReadings",
                columns: new[] { "TagName", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_DataBlockReadings_Timestamp",
                table: "DataBlockReadings",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_DataPointConfigurations_TagName",
                table: "DataPointConfigurations",
                column: "TagName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DigitalReadings_TagName_Timestamp",
                table: "DigitalReadings",
                columns: new[] { "TagName", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_DigitalReadings_Timestamp",
                table: "DigitalReadings",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalogReadings");

            migrationBuilder.DropTable(
                name: "DataBlockReadings");

            migrationBuilder.DropTable(
                name: "DataPointConfigurations");

            migrationBuilder.DropTable(
                name: "DigitalReadings");
        }
    }
}
