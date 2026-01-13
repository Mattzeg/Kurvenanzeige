using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kurvenanzeige.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStringReadings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StringReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TagName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DbNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Offset = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    MaxLength = table.Column<int>(type: "INTEGER", nullable: false),
                    Quality = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StringReadings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StringReadings_TagName_Timestamp",
                table: "StringReadings",
                columns: new[] { "TagName", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_StringReadings_Timestamp",
                table: "StringReadings",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StringReadings");
        }
    }
}
