using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ums.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDomainResourceModuleLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FeatureFlags_FlagCode",
                schema: "ums_configuration",
                table: "FeatureFlags");

            migrationBuilder.AddColumn<Guid>(
                name: "SystemSuiteId",
                schema: "ums_configuration",
                table: "FeatureFlags",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "ums_configuration",
                table: "FeatureFlags",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FeatureFlagCriteria",
                schema: "ums_configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FeatureFlagId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CriteriaType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Operator = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureFlagCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureFlagCriteria_FeatureFlags_FeatureFlagId",
                        column: x => x.FeatureFlagId,
                        principalSchema: "ums_configuration",
                        principalTable: "FeatureFlags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlags_SystemSuiteId",
                schema: "ums_configuration",
                table: "FeatureFlags",
                column: "SystemSuiteId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlags_SystemSuiteId_FlagCode",
                schema: "ums_configuration",
                table: "FeatureFlags",
                columns: new[] { "SystemSuiteId", "FlagCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlagCriteria_FeatureFlagId",
                schema: "ums_configuration",
                table: "FeatureFlagCriteria",
                column: "FeatureFlagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeatureFlagCriteria",
                schema: "ums_configuration");

            migrationBuilder.DropIndex(
                name: "IX_FeatureFlags_SystemSuiteId",
                schema: "ums_configuration",
                table: "FeatureFlags");

            migrationBuilder.DropIndex(
                name: "IX_FeatureFlags_SystemSuiteId_FlagCode",
                schema: "ums_configuration",
                table: "FeatureFlags");

            migrationBuilder.DropColumn(
                name: "SystemSuiteId",
                schema: "ums_configuration",
                table: "FeatureFlags");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "ums_configuration",
                table: "FeatureFlags");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlags_FlagCode",
                schema: "ums_configuration",
                table: "FeatureFlags",
                column: "FlagCode",
                unique: true);
        }
    }
}
