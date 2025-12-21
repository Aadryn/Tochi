# Next Steps - Ready to Deploy

**Current Status:** ✅ Code complete, migration created, ready for database initialization

---

## Prerequisites

### Required Software
- [x] .NET 9 SDK - Installed ✅
- [ ] Docker Desktop - **Need to start**
- [ ] PostgreSQL client (optional, for manual DB access)

---

## Step-by-Step Deployment

### 1. Start Docker Desktop

**Windows:**
```powershell
# Start Docker Desktop from Start Menu or run:
Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"

# Wait for Docker to be ready (about 30-60 seconds)
# Verify Docker is running:
docker --version
```

**Alternative (without Docker):**
If you prefer not to use Docker, you can install PostgreSQL and Redis directly:
- PostgreSQL 16+: https://www.postgresql.org/download/windows/
- Redis for Windows: https://github.com/microsoftarchive/redis/releases

---

### 2. Start Infrastructure Services

Once Docker is running:

```powershell
cd D:\workspaces\sandbox\proxy
docker-compose up -d
```

This starts:
- **PostgreSQL** on port 5432 (database)
- **Redis** on port 6379 (cache & quotas)
- **Jaeger** on port 16686 (telemetry UI)

Verify services are running:
```powershell
docker-compose ps
```

Expected output:
```
NAME                IMAGE                      STATUS
proxy-postgres-1    postgres:16-alpine         Up
proxy-redis-1       redis:7-alpine            Up
proxy-jaeger-1      jaegertracing/all-in-one  Up
```

---

### 3. Apply Database Migration

Create the database schema:

```powershell
dotnet ef database update `
  --project src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL `
  --startup-project src\Presentation\LLMProxy.Admin.API
```

Expected output:
```
Build started...
Build succeeded.
Applying migration '20251221031424_InitialCreate'.
Done.
```

Verify tables were created:
```powershell
# Using docker exec to access PostgreSQL
docker exec -it proxy-postgres-1 psql -U llmproxy -d llmproxy -c "\dt"
```

You should see 13 tables:
- api_keys
- audit_logs
- llm_provider_configurations
- llm_provider_routing_strategies
- llm_providers
- quota_limits
- quotas
- tenant_settings
- tenants
- token_usage_metrics
- users
- (plus EF Core migration history table)

---

### 4. Start the Applications

**Terminal 1 - Admin API:**
```powershell
cd D:\workspaces\sandbox\proxy
dotnet run --project src\Presentation\LLMProxy.Admin.API
```

Expected output:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

**Terminal 2 - Gateway:**
```powershell
cd D:\workspaces\sandbox\proxy
dotnet run --project src\Presentation\LLMProxy.Gateway
```

Expected output:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5001
```

---

### 5. Test the System

#### A. Health Checks

```powershell
# Admin API
curl http://localhost:5000/health

# Gateway
curl http://localhost:5001/health
```

Both should return: `Healthy`

#### B. Create Your First Tenant

```powershell
curl -X POST http://localhost:5000/api/tenants `
  -H "Content-Type: application/json" `
  -d '{
    "name": "Demo Company",
    "slug": "demo",
    "maxUsers": 10,
    "maxProviders": 5,
    "enableAuditLogging": true,
    "auditRetentionDays": 90,
    "enableResponseCache": true
  }'
```

Save the returned `id` (GUID) - you'll need it for the next steps.

#### C. Create a User

```powershell
# Replace <tenant-id> with the actual GUID from step B
curl -X POST http://localhost:5000/api/users `
  -H "Content-Type: application/json" `
  -d '{
    "tenantId": "<tenant-id>",
    "email": "admin@demo.com",
    "firstName": "Admin",
    "lastName": "User",
    "role": "TenantAdmin"
  }'
```

Save the returned `id` (user GUID).

#### D. Generate an API Key

```powershell
# Replace <user-id> with the actual GUID from step C
curl -X POST http://localhost:5000/api/apikeys `
  -H "Content-Type: application/json" `
  -d '{
    "userId": "<user-id>",
    "name": "Primary API Key",
    "expiresAt": "2026-12-31T23:59:59Z"
  }'
```

**IMPORTANT:** Save the returned `key` value - this is shown only once! It looks like: `llmproxy_xxxxxxxxxxxxx`

#### E. Add an LLM Provider

**Option 1: OpenAI**
```powershell
curl -X POST http://localhost:5000/api/llmproviders `
  -H "Content-Type: application/json" `
  -d '{
    "tenantId": "<tenant-id>",
    "name": "OpenAI GPT-4",
    "type": "OpenAI",
    "baseUrl": "https://api.openai.com/v1",
    "model": "gpt-4",
    "apiKeySecretName": "openai-api-key",
    "timeoutSeconds": 30,
    "maxRetries": 3,
    "supportsStreaming": true,
    "routingMethod": "RoundRobin",
    "priority": 1
  }'
```

**Option 2: Ollama (Local)**
```powershell
curl -X POST http://localhost:5000/api/llmproviders `
  -H "Content-Type: application/json" `
  -d '{
    "tenantId": "<tenant-id>",
    "name": "Local Ollama",
    "type": "Ollama",
    "baseUrl": "http://localhost:11434/v1",
    "model": "llama2",
    "apiKeySecretName": "",
    "timeoutSeconds": 60,
    "maxRetries": 2,
    "supportsStreaming": true,
    "routingMethod": "Failover",
    "priority": 2
  }'
```

#### F. Test the Proxy!

```powershell
# Replace <your-api-key> with the key from step D
curl -X POST http://localhost:5001/v1/chat/completions `
  -H "Authorization: Bearer <your-api-key>" `
  -H "Content-Type: application/json" `
  -d '{
    "model": "gpt-4",
    "messages": [
      {"role": "system", "content": "You are a helpful assistant."},
      {"role": "user", "content": "Hello! What can you do?"}
    ]
  }'
```

**Success!** You should get a response from the LLM provider routed through your proxy! 🎉

---

### 6. View Telemetry

Open Jaeger UI in your browser:
```
http://localhost:16686
```

- Select service: `llm-gateway` or `llm-admin-api`
- Click "Find Traces"
- See detailed traces of your requests with timing information

---

## Troubleshooting

### Docker Desktop won't start
**Solution:** Install PostgreSQL and Redis manually, update connection strings in `appsettings.json`

### Migration fails with "database does not exist"
**Solution:** The migration should auto-create the database, but if not:
```powershell
docker exec -it proxy-postgres-1 psql -U llmproxy -c "CREATE DATABASE llmproxy;"
```

### Port 5000 or 5001 already in use
**Solution:** Change ports in `appsettings.json` or stop other services

### "Cannot connect to PostgreSQL"
**Solution:** Check Docker is running: `docker ps | grep postgres`

### API Key authentication fails
**Solution:** 
1. Ensure you're using the full key (starts with `llmproxy_`)
2. Check the key hasn't expired
3. Verify user is active

---

## Configuration Files

### Connection Strings (appsettings.json)

**Admin.API and Gateway:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=llmproxy;Username=llmproxy;Password=llmproxy123",
    "Redis": "localhost:6379"
  }
}
```

### Environment Variables

For production, use environment variables instead of appsettings.json:

```bash
export ConnectionStrings__DefaultConnection="Host=prod-db;Port=5432;..."
export ConnectionStrings__Redis="prod-redis:6379"
export SecretProvider="AzureKeyVault"  # or "EnvironmentVariables"
```

---

## Production Deployment Checklist

Before deploying to production:

- [ ] Change default PostgreSQL password
- [ ] Set up proper secret management (Azure Key Vault, AWS Secrets Manager)
- [ ] Configure HTTPS/TLS
- [ ] Set up proper logging (Application Insights, CloudWatch)
- [ ] Configure rate limiting
- [ ] Set up health check monitoring
- [ ] Configure backup strategy for PostgreSQL
- [ ] Set Redis persistence mode
- [ ] Review and adjust quota limits
- [ ] Set up CI/CD pipeline
- [ ] Configure proper CORS policies
- [ ] Add request size limits
- [ ] Set up alert rules
- [ ] Configure horizontal scaling (multiple Gateway instances)
- [ ] Review audit log retention policy

---

## Quick Reference

### Service URLs
- Admin API: http://localhost:5000
- Gateway: http://localhost:5001
- Jaeger UI: http://localhost:16686
- PostgreSQL: localhost:5432
- Redis: localhost:6379

### Default Credentials (Development Only!)
- PostgreSQL: llmproxy / llmproxy123
- Redis: (no password)

### Key Files
- Migration: `src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL/Migrations/20251221031424_InitialCreate.cs`
- Docker Compose: `docker-compose.yml`
- Admin API Config: `src/Presentation/LLMProxy.Admin.API/appsettings.json`
- Gateway Config: `src/Presentation/LLMProxy.Gateway/appsettings.json`

---

## What's Next?

Once the system is running:

1. **Add Quotas:** Create quota limits for users/tenants
2. **Test Routing:** Add multiple providers and test routing strategies
3. **Monitor Usage:** Check token usage metrics in the database
4. **Review Logs:** Check audit logs for all requests
5. **Test Caching:** Make duplicate requests to see caching in action
6. **Explore Telemetry:** Use Jaeger to analyze request performance

---

## Support

- **Documentation:** See all `.md` files in the project root
- **Architecture:** [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)
- **Database Schema:** [docs/DATABASE.md](docs/DATABASE.md)
- **Issues:** Check build logs and application logs

---

**Ready to go!** 🚀 Start Docker Desktop and follow the steps above.
