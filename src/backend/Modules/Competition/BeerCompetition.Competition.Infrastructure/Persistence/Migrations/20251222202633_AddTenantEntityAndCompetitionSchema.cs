using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeerCompetition.Competition.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantEntityAndCompetitionSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Competition");

            migrationBuilder.RenameTable(
                name: "competitions",
                newName: "competitions",
                newSchema: "Competition");

            migrationBuilder.CreateTable(
                name: "tenants",
                schema: "Competition",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tenants_email",
                schema: "Competition",
                table: "tenants",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenants_status",
                schema: "Competition",
                table: "tenants",
                column: "status");

            migrationBuilder.AddForeignKey(
                name: "FK_competitions_tenants_tenant_id",
                schema: "Competition",
                table: "competitions",
                column: "tenant_id",
                principalSchema: "Competition",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_competitions_tenants_tenant_id",
                schema: "Competition",
                table: "competitions");

            migrationBuilder.DropTable(
                name: "tenants",
                schema: "Competition");

            migrationBuilder.RenameTable(
                name: "competitions",
                schema: "Competition",
                newName: "competitions");
        }
    }
}
