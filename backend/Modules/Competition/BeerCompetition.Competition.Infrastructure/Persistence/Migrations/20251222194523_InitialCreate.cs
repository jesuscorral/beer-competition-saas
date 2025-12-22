using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeerCompetition.Competition.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "competitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    registration_deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    judging_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    judging_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    max_entries_per_entrant = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_competitions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_competitions_registration_deadline",
                table: "competitions",
                column: "registration_deadline");

            migrationBuilder.CreateIndex(
                name: "ix_competitions_tenant_id",
                table: "competitions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_competitions_tenant_status",
                table: "competitions",
                columns: new[] { "tenant_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "competitions");
        }
    }
}
