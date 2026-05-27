using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ums.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSuiteDomainResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemSuiteDomainResources",
                schema: "ums_authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SystemSuiteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuditTimeSpan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSuiteDomainResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemSuiteDomainResources_SystemSuites_SystemSuiteId",
                        column: x => x.SystemSuiteId,
                        principalSchema: "ums_authorization",
                        principalTable: "SystemSuites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemSuiteDomainResources_SystemSuiteId_Code",
                schema: "ums_authorization",
                table: "SystemSuiteDomainResources",
                columns: new[] { "SystemSuiteId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSuiteDomainResources",
                schema: "ums_authorization");
        }
    }
}
