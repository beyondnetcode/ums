using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ums.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainResourceParentAndDomainMethodType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentResourceId",
                schema: "ums_authorization",
                table: "SystemSuiteDomainResources",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentResourceId",
                schema: "ums_authorization",
                table: "SystemSuiteDomainResources");
        }
    }
}
