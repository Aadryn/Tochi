# Database Migration Scripts

## Initial Migration

### Create the initial migration
```powershell
# Navigate to Infrastructure.PostgreSQL project
cd src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL

# Create initial migration
dotnet ef migrations add InitialCreate --startup-project ../../Presentation/LLMProxy.Admin.API/LLMProxy.Admin.API.csproj

# Apply migration to database
dotnet ef database update --startup-project ../../Presentation/LLMProxy.Admin.API/LLMProxy.Admin.API.csproj
```

### Alternative: Using Package Manager Console (Visual Studio)
```powershell
# Set Infrastructure.PostgreSQL as default project in Package Manager Console
Add-Migration InitialCreate -Project LLMProxy.Infrastructure.PostgreSQL -StartupProject LLMProxy.Admin.API
Update-Database -Project LLMProxy.Infrastructure.PostgreSQL -StartupProject LLMProxy.Admin.API
```

## Database Schema

The migration will create the following tables:

### Core Tables
- **tenants**: Multi-tenant isolation
  - Columns: id, name, slug, is_active, settings (JSONB), created_at, updated_at
  - Indexes: slug (unique), is_active

- **users**: User authentication and authorization
  - Columns: id, tenant_id, email, name, is_active, role, created_at, updated_at
  - Indexes: tenant_id + email (unique), tenant_id, email

- **api_keys**: API key management with SHA256 hashing
  - Columns: id, user_id, tenant_id, name, key_hash, key_prefix, is_active, is_revoked, expires_at, last_used_at, revoked_at, created_at, updated_at
  - Indexes: key_hash (unique), key_prefix, user_id, tenant_id, is_active

### LLM Management Tables
- **llm_providers**: LLM provider configuration per tenant
  - Columns: id, tenant_id, name, provider_type, base_url, model, is_active, priority, configuration (JSONB), routing_strategy (JSONB), created_at, updated_at
  - Indexes: tenant_id, is_active, priority

### Quota & Metering Tables
- **quota_limits**: Quota configuration per user
  - Columns: id, user_id, tenant_id, quota_type, limit_value, period, created_at, updated_at
  - Indexes: user_id + quota_type + period (unique), user_id, tenant_id

- **token_usage_metrics**: Aggregated token usage tracking
  - Columns: id, tenant_id, user_id, provider_id, period_start, period, total_requests, prompt_tokens, completion_tokens, total_tokens, created_at, updated_at
  - Indexes: tenant_id + user_id + provider_id + period_start + period (unique - aggregation key)

### Audit Tables
- **audit_logs**: Comprehensive audit trail with JSONB metadata
  - Columns: id, tenant_id, user_id, action, resource_type, resource_id, ip_address, user_agent, metadata (JSONB), created_at
  - Indexes: tenant_id, user_id, resource_type, action, created_at (for retention policies)

## Running Migrations in Docker

If using Docker, migrations can be applied on container startup:

```yaml
# In docker-compose.yml, add migration service
services:
  db-migrate:
    image: mcr.microsoft.com/dotnet/sdk:9.0
    depends_on:
      postgres:
        condition: service_healthy
    volumes:
      - ./src:/app/src
    working_dir: /app/src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL
    command: >
      sh -c "
      dotnet tool install --global dotnet-ef &&
      dotnet ef database update --startup-project ../../Presentation/LLMProxy.Admin.API
      "
```

## Common Migration Commands

### Add a new migration
```powershell
dotnet ef migrations add MigrationName --startup-project ../../Presentation/LLMProxy.Admin.API/LLMProxy.Admin.API.csproj
```

### Remove last migration (if not applied)
```powershell
dotnet ef migrations remove --startup-project ../../Presentation/LLMProxy.Admin.API/LLMProxy.Admin.API.csproj
```

### Generate SQL script (for manual execution)
```powershell
dotnet ef migrations script --startup-project ../../Presentation/LLMProxy.Admin.API/LLMProxy.Admin.API.csproj --output schema.sql
```

### Check migration status
```powershell
dotnet ef migrations list --startup-project ../../Presentation/LLMProxy.Admin.API/LLMProxy.Admin.API.csproj
```

## Seed Data (Optional)

Create a SQL script for initial data:

```sql
-- Create default tenant
INSERT INTO tenants (id, name, slug, is_active, settings, created_at, updated_at)
VALUES (
  'a0000000-0000-0000-0000-000000000001',
  'Default Tenant',
  'default',
  true,
  '{"maxUsers": 100, "maxApiKeys": 500, "maxRequestsPerMinute": 1000, "maxRequestsPerDay": 100000, "maxTokensPerDay": 1000000, "allowedProviders": ["OpenAI", "Ollama", "Anthropic"]}'::jsonb,
  NOW(),
  NOW()
);

-- Create admin user
INSERT INTO users (id, tenant_id, email, name, is_active, role, created_at, updated_at)
VALUES (
  'b0000000-0000-0000-0000-000000000001',
  'a0000000-0000-0000-0000-000000000001',
  'admin@llmproxy.local',
  'Admin User',
  true,
  1, -- Admin role
  NOW(),
  NOW()
);
```

Execute seed data:
```powershell
psql -h localhost -U postgres -d llmproxy -f seed.sql
```

## Troubleshooting

### Connection Issues
- Verify PostgreSQL is running: `docker ps` or `netstat -an | findstr 5432`
- Check connection string in appsettings.json
- Ensure user has proper permissions

### Migration Errors
- If schema is out of sync: Drop database and rerun migrations
- Check for pending migrations: `dotnet ef migrations list`
- Review migration code in Migrations folder

### Performance Tips
- Ensure indexes are created (check EF Core configurations)
- Use EXPLAIN ANALYZE for slow queries
- Monitor query performance in pgAdmin or DataGrip
