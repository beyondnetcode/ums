using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ums.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Ep09ProfileRequestFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                schema: "ums_identity",
                table: "UserAccounts",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionReason",
                schema: "approvals",
                table: "ApprovalRequests",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GrantedRoleId",
                schema: "approvals",
                table: "ApprovalRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Justification",
                schema: "approvals",
                table: "ApprovalRequests",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestedBranchId",
                schema: "approvals",
                table: "ApprovalRequests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestedRoleId",
                schema: "approvals",
                table: "ApprovalRequests",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "RequestedSystemId",
                schema: "approvals",
                table: "ApprovalRequests",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ParameterDefinitions",
                schema: "ums_configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    DataTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultValue = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    ScopeId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsMandatory = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParameterGlobalValues",
                schema: "ums_configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParameterDefinitionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EffectiveValue = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterGlobalValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParameterTenantValues",
                schema: "ums_configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParameterDefinitionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OverrideValue = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParameterTenantValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantParameters",
                schema: "ums_identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    ValueTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSensitive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DefaultValue = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    AllowedValues = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantParameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantSignupRequests",
                schema: "ums_identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CompanyReference = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ContactName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedTenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantSignupRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_TargetUserId_RequestedSystemId_RequestedBranchId_StatusId",
                schema: "approvals",
                table: "ApprovalRequests",
                columns: new[] { "TargetUserId", "RequestedSystemId", "RequestedBranchId", "StatusId" });

            migrationBuilder.CreateIndex(
                name: "IX_ParameterDefinitions_Code",
                schema: "ums_configuration",
                table: "ParameterDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParameterDefinitions_IsActive",
                schema: "ums_configuration",
                table: "ParameterDefinitions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterDefinitions_ScopeId",
                schema: "ums_configuration",
                table: "ParameterDefinitions",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterGlobalValues_ParameterDefinitionId",
                schema: "ums_configuration",
                table: "ParameterGlobalValues",
                column: "ParameterDefinitionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParameterGlobalValues_StatusId",
                schema: "ums_configuration",
                table: "ParameterGlobalValues",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterTenantValues_StatusId",
                schema: "ums_configuration",
                table: "ParameterTenantValues",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterTenantValues_TenantId",
                schema: "ums_configuration",
                table: "ParameterTenantValues",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ParameterTenantValues_TenantId_ParameterDefinitionId",
                schema: "ums_configuration",
                table: "ParameterTenantValues",
                columns: new[] { "TenantId", "ParameterDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantParameters_TenantId_Code_IsActive",
                schema: "ums_identity",
                table: "TenantParameters",
                columns: new[] { "TenantId", "Code", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSignupRequests_CompanyReference",
                schema: "ums_identity",
                table: "TenantSignupRequests",
                column: "CompanyReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantSignupRequests_ContactEmail",
                schema: "ums_identity",
                table: "TenantSignupRequests",
                column: "ContactEmail");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSignupRequests_StatusId",
                schema: "ums_identity",
                table: "TenantSignupRequests",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParameterDefinitions",
                schema: "ums_configuration");

            migrationBuilder.DropTable(
                name: "ParameterGlobalValues",
                schema: "ums_configuration");

            migrationBuilder.DropTable(
                name: "ParameterTenantValues",
                schema: "ums_configuration");

            migrationBuilder.DropTable(
                name: "TenantParameters",
                schema: "ums_identity");

            migrationBuilder.DropTable(
                name: "TenantSignupRequests",
                schema: "ums_identity");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalRequests_TargetUserId_RequestedSystemId_RequestedBranchId_StatusId",
                schema: "approvals",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                schema: "ums_identity",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "DecisionReason",
                schema: "approvals",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "GrantedRoleId",
                schema: "approvals",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "Justification",
                schema: "approvals",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "RequestedBranchId",
                schema: "approvals",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "RequestedRoleId",
                schema: "approvals",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "RequestedSystemId",
                schema: "approvals",
                table: "ApprovalRequests");
        }
    }
}
