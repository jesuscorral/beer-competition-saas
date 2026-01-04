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
            migrationBuilder.EnsureSchema(
                name: "Competition");

            migrationBuilder.CreateTable(
                name: "subscription_plans",
                schema: "Competition",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    max_entries = table.Column<int>(type: "integer", nullable: false),
                    price_amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "EUR"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_plans", x => x.id);
                });

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

            migrationBuilder.CreateTable(
                name: "competitions",
                schema: "Competition",
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
                    subscription_plan_id = table.Column<Guid>(type: "uuid", nullable: true),
                    max_entries = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    is_public = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_competitions", x => x.id);
                    table.ForeignKey(
                        name: "FK_competitions_subscription_plans_subscription_plan_id",
                        column: x => x.subscription_plan_id,
                        principalSchema: "Competition",
                        principalTable: "subscription_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_competitions_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "Competition",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_competitions_registration_deadline",
                schema: "Competition",
                table: "competitions",
                column: "registration_deadline");

            migrationBuilder.CreateIndex(
                name: "ix_competitions_subscription_plan_id_unique",
                schema: "Competition",
                table: "competitions",
                column: "subscription_plan_id",
                unique: true,
                filter: "subscription_plan_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_competitions_tenant_id",
                schema: "Competition",
                table: "competitions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_competitions_tenant_status",
                schema: "Competition",
                table: "competitions",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_subscription_plans_tenant",
                schema: "Competition",
                table: "subscription_plans",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "idx_subscription_plans_tenant_name",
                schema: "Competition",
                table: "subscription_plans",
                columns: new[] { "tenant_id", "name" },
                unique: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "competitions",
                schema: "Competition");

            migrationBuilder.DropTable(
                name: "subscription_plans",
                schema: "Competition");

            migrationBuilder.DropTable(
                name: "tenants",
                schema: "Competition");
        }
    }
}
