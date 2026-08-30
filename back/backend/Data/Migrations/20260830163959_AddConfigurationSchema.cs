using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "configuration");

            migrationBuilder.RenameTable(
                name: "workflows",
                newName: "workflows",
                newSchema: "configuration");

            migrationBuilder.RenameTable(
                name: "personas",
                newName: "personas",
                newSchema: "configuration");

            migrationBuilder.RenameTable(
                name: "knowledge_bases",
                newName: "knowledge_bases",
                newSchema: "configuration");

            migrationBuilder.UpdateData(
                table: "agent_users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 30, 16, 39, 58, 567, DateTimeKind.Utc).AddTicks(2741));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "workflows",
                schema: "configuration",
                newName: "workflows");

            migrationBuilder.RenameTable(
                name: "personas",
                schema: "configuration",
                newName: "personas");

            migrationBuilder.RenameTable(
                name: "knowledge_bases",
                schema: "configuration",
                newName: "knowledge_bases");

            migrationBuilder.UpdateData(
                table: "agent_users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 30, 16, 29, 11, 283, DateTimeKind.Utc).AddTicks(7177));
        }
    }
}
