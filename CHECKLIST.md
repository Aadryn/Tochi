# ✅ Deployment Checklist

Use this checklist to deploy your LLM Proxy step by step.

---

## Pre-Deployment (Already Complete ✅)

- [x] Code written and compiling
- [x] All 65+ errors fixed
- [x] Database migration created
- [x] Docker Compose configured
- [x] Documentation written
- [x] Setup scripts created

**Status: 100% Complete!**

---

## Deployment Steps (Do Now)

### Phase 1: Start Infrastructure

- [ ] **Start Docker Desktop**
  - Open Docker Desktop application
  - Wait for "Docker Desktop is running" message
  - Verify: `docker --version` works
  
- [ ] **Run setup script**
  ```powershell
  .\setup.ps1
  ```
  - This takes ~2-3 minutes
  - Watch for ✓ green checkmarks
  
- [ ] **Verify infrastructure is running**
  ```powershell
  docker-compose ps
  ```
  - Should show: postgres, redis, jaeger all "Up"

---

### Phase 2: Start Applications

- [ ] **Start Admin API** (Terminal 1)
  ```powershell
  dotnet run --project src\Presentation\LLMProxy.Admin.API
  ```
  - Wait for: "Now listening on: http://localhost:5000"
  
- [ ] **Start Gateway** (Terminal 2)
  ```powershell
  dotnet run --project src\Presentation\LLMProxy.Gateway
  ```
  - Wait for: "Now listening on: http://localhost:5001"

---

### Phase 3: Verify System

- [ ] **Run test script**
  ```powershell
  .\test.ps1
  ```
  - All 5 tests should pass ✓

- [ ] **Test health endpoints**
  ```powershell
  curl http://localhost:5000/health  # Should return: Healthy
  curl http://localhost:5001/health  # Should return: Healthy
  ```

- [ ] **Check Jaeger UI**
  - Open: http://localhost:16686
  - Should see Jaeger interface

---

### Phase 4: Configure First Tenant

- [ ] **Create tenant**
  ```powershell
  curl -X POST http://localhost:5000/api/tenants `
    -H "Content-Type: application/json" `
    -d '{"name":"Demo Company","slug":"demo","maxUsers":10,"maxProviders":5,"enableAuditLogging":true,"auditRetentionDays":90,"enableResponseCache":true}'
  ```
  - Save the returned `id` (tenant GUID)

- [ ] **Create user**
  ```powershell
  # Replace <tenant-id> with actual GUID
  curl -X POST http://localhost:5000/api/users `
    -H "Content-Type: application/json" `
    -d '{"tenantId":"<tenant-id>","email":"admin@demo.com","firstName":"Admin","lastName":"User","role":"TenantAdmin"}'
  ```
  - Save the returned `id` (user GUID)

- [ ] **Generate API key**
  ```powershell
  # Replace <user-id> with actual GUID
  curl -X POST http://localhost:5000/api/apikeys `
    -H "Content-Type: application/json" `
    -d '{"userId":"<user-id>","name":"Primary Key","expiresAt":"2026-12-31T23:59:59Z"}'
  ```
  - **IMPORTANT:** Save the returned `key` - shown only once!

- [ ] **Add LLM provider**
  ```powershell
  # Option 1: OpenAI
  curl -X POST http://localhost:5000/api/llmproviders `
    -H "Content-Type: application/json" `
    -d '{"tenantId":"<tenant-id>","name":"OpenAI GPT-4","type":"OpenAI","baseUrl":"https://api.openai.com/v1","model":"gpt-4","apiKeySecretName":"openai-api-key","timeoutSeconds":30,"maxRetries":3,"supportsStreaming":true,"routingMethod":"RoundRobin","priority":1}'
  
  # Option 2: Ollama (local)
  curl -X POST http://localhost:5000/api/llmproviders `
    -H "Content-Type: application/json" `
    -d '{"tenantId":"<tenant-id>","name":"Local Ollama","type":"Ollama","baseUrl":"http://localhost:11434/v1","model":"llama2","apiKeySecretName":"","timeoutSeconds":60,"maxRetries":2,"supportsStreaming":true,"routingMethod":"Failover","priority":2}'
  ```

---

### Phase 5: Test the Proxy

- [ ] **Make your first proxied request**
  ```powershell
  # Replace <your-api-key> with the key from Phase 4
  curl -X POST http://localhost:5001/v1/chat/completions `
    -H "Authorization: Bearer <your-api-key>" `
    -H "Content-Type: application/json" `
    -d '{
      "model": "gpt-4",
      "messages": [
        {"role": "system", "content": "You are a helpful assistant."},
        {"role": "user", "content": "Say hello!"}
      ]
    }'
  ```
  - Should receive LLM response! 🎉

---

### Phase 6: Verify Observability

- [ ] **Check traces in Jaeger**
  - Go to: http://localhost:16686
  - Select service: `llm-gateway`
  - Click "Find Traces"
  - See your request trace with timing

- [ ] **Query audit logs**
  ```powershell
  # Using database directly
  docker exec -it proxy-postgres-1 psql -U llmproxy -d llmproxy -c "SELECT * FROM audit_logs ORDER BY created_at DESC LIMIT 5;"
  ```

- [ ] **Check token usage**
  ```powershell
  docker exec -it proxy-postgres-1 psql -U llmproxy -d llmproxy -c "SELECT * FROM token_usage_metrics;"
  ```

---

## Post-Deployment

### Optional: Add Quotas

- [ ] **Create quota limit**
  ```powershell
  curl -X POST http://localhost:5000/api/quotas `
    -H "Content-Type: application/json" `
    -d '{"tenantId":"<tenant-id>","userId":"<user-id>","quotaType":"Requests","period":"Daily","maxValue":100}'
  ```

### Optional: Test Caching

- [ ] **Make same request twice**
  - First request: Normal latency
  - Second request: Much faster (cached)
  - Check Redis: `docker exec -it proxy-redis-1 redis-cli KEYS "*"`

---

## Troubleshooting

If anything fails, check:

- [ ] Docker is actually running: `docker ps`
- [ ] Services are healthy: `.\test.ps1`
- [ ] Ports aren't in use: `netstat -an | findstr "5000 5001 5432 6379"`
- [ ] Check logs: `docker-compose logs`
- [ ] Application logs in terminals

**Full troubleshooting guide:** [NEXT_STEPS.md](NEXT_STEPS.md) section "Troubleshooting"

---

## Success Criteria

You've successfully deployed when:

- ✅ All infrastructure services running
- ✅ Both APIs responding to health checks
- ✅ Tenant, user, and API key created
- ✅ At least one provider configured
- ✅ Successfully proxied a request through Gateway
- ✅ Traces visible in Jaeger
- ✅ Audit logs recorded in database

---

## What's Next?

Once deployed:

1. **Add more providers** - Diversify your LLM options
2. **Set up quotas** - Control usage
3. **Monitor metrics** - Check token_usage_metrics table
4. **Review audit logs** - See all requests
5. **Test routing** - Add multiple providers, test failover
6. **Explore caching** - Make duplicate requests
7. **Production hardening** - See NEXT_STEPS.md "Production Deployment Checklist"

---

## Quick Reference

**Services:**
- Admin API: http://localhost:5000
- Gateway: http://localhost:5001
- Jaeger: http://localhost:16686

**Scripts:**
```powershell
.\setup.ps1           # Initial setup
.\start-services.ps1  # Start both APIs
.\test.ps1            # Run tests
docker-compose down   # Stop infrastructure
```

**Documentation:**
- **[START_HERE.md](START_HERE.md)** - Quickest overview
- **[NEXT_STEPS.md](NEXT_STEPS.md)** - Detailed guide
- **[DOCUMENTATION_INDEX.md](DOCUMENTATION_INDEX.md)** - All docs

---

## Progress Tracker

Current Phase: **⏳ Waiting for Docker Desktop**

```
Phase 1: Infrastructure  [ Pending... ]
Phase 2: Applications    [ Not Started ]
Phase 3: Verification    [ Not Started ]
Phase 4: Configuration   [ Not Started ]
Phase 5: Testing         [ Not Started ]
Phase 6: Observability   [ Not Started ]
```

**Next action:** Start Docker Desktop → Run `.\setup.ps1`

---

*Keep this file open and check off items as you complete them!*
