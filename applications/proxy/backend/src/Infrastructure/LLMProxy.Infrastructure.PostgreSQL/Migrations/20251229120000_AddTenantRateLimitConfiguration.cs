using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LLMProxy.Infrastructure.PostgreSQL.Migrations
{
    /// <summary>
    /// Migration pour ajouter les tables de configuration de rate limiting par tenant.
    /// </summary>
    /// <remarks>
    /// Conforme à l'ADR-041 Rate Limiting et Throttling.
    /// Tables créées :
    /// - configuration.tenant_ratelimit_configurations : configuration globale par tenant
    /// - configuration.endpoint_limits : limites spécifiques par endpoint
    /// </remarks>
    public partial class AddTenantRateLimitConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Créer le schéma configuration s'il n'existe pas
            migrationBuilder.Sql(@"CREATE SCHEMA IF NOT EXISTS configuration;");

            // Table principale : configurations de rate limiting par tenant
            migrationBuilder.CreateTable(
                name: "tenant_ratelimit_configurations",
                schema: "configuration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    global_requests_per_minute = table.Column<int>(type: "integer", nullable: false, defaultValue: 1000),
                    global_requests_per_day = table.Column<int>(type: "integer", nullable: false, defaultValue: 100000),
                    global_tokens_per_minute = table.Column<int>(type: "integer", nullable: false, defaultValue: 100000),
                    global_tokens_per_day = table.Column<int>(type: "integer", nullable: false, defaultValue: 10000000),
                    api_key_requests_per_minute = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    api_key_tokens_per_minute = table.Column<int>(type: "integer", nullable: false, defaultValue: 10000),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_ratelimit_configurations", x => x.id);
                    table.ForeignKey(
                        name: "fk_tenant_ratelimit_configurations_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Index unique sur TenantId (un seul config par tenant)
            migrationBuilder.CreateIndex(
                name: "ix_tenant_ratelimit_configurations_tenant_id",
                schema: "configuration",
                table: "tenant_ratelimit_configurations",
                column: "tenant_id",
                unique: true);

            // Table des limites par endpoint
            migrationBuilder.CreateTable(
                name: "endpoint_limits",
                schema: "configuration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_ratelimit_configuration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    endpoint_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    requests_per_minute = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    tokens_per_minute = table.Column<int>(type: "integer", nullable: false, defaultValue: 50000),
                    burst_capacity = table.Column<int>(type: "integer", nullable: false, defaultValue: 200),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_endpoint_limits", x => x.id);
                    table.ForeignKey(
                        name: "fk_endpoint_limits_tenant_ratelimit_configurations",
                        column: x => x.tenant_ratelimit_configuration_id,
                        principalSchema: "configuration",
                        principalTable: "tenant_ratelimit_configurations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Index unique sur (ConfigId, EndpointPath)
            migrationBuilder.CreateIndex(
                name: "ix_endpoint_limits_configuration_endpoint",
                schema: "configuration",
                table: "endpoint_limits",
                columns: new[] { "tenant_ratelimit_configuration_id", "endpoint_path" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "endpoint_limits",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "tenant_ratelimit_configurations",
                schema: "configuration");
        }
    }
}
