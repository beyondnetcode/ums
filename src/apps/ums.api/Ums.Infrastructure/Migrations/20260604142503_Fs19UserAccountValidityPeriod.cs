using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ums.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fs19UserAccountValidityPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAtUtc",
                schema: "ums_identity",
                table: "UserAccounts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiresAtUtc",
                schema: "ums_identity",
                table: "UserAccounts");
        }
    }
}
