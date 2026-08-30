using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "billing");

            migrationBuilder.RenameTable(
                name: "usage_records",
                newName: "usage_records",
                newSchema: "billing");

            migrationBuilder.RenameTable(
                name: "subscriptions",
                newName: "subscriptions",
                newSchema: "billing");

            migrationBuilder.RenameTable(
                name: "plans",
                newName: "plans",
                newSchema: "billing");

            migrationBuilder.RenameTable(
                name: "partner_plans",
                newName: "partner_plans",
                newSchema: "identity");

            migrationBuilder.UpdateData(
                table: "agent_users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 30, 16, 29, 11, 283, DateTimeKind.Utc).AddTicks(7177));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "usage_records",
                schema: "billing",
                newName: "usage_records");

            migrationBuilder.RenameTable(
                name: "subscriptions",
                schema: "billing",
                newName: "subscriptions");

            migrationBuilder.RenameTable(
                name: "plans",
                schema: "billing",
                newName: "plans");

            migrationBuilder.RenameTable(
                name: "partner_plans",
                schema: "identity",
                newName: "partner_plans");

            migrationBuilder.UpdateData(
                table: "agent_users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 30, 16, 11, 25, 211, DateTimeKind.Utc).AddTicks(2249));
        }
    }
}
