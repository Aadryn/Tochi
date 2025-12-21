# Test Script - Verify System is Working

param(
    [Parameter(Mandatory=$false)]
    [string]$ApiKey = ""
)

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  LLM Proxy - System Test" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Health Checks
Write-Host "[Test 1/5] Health Checks" -ForegroundColor Yellow
Write-Host "Testing Admin API..." -ForegroundColor Gray
try {
    $adminHealth = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing -TimeoutSec 5
    if ($adminHealth.StatusCode -eq 200) {
        Write-Host "✓ Admin API is healthy" -ForegroundColor Green
    } else {
        Write-Host "✗ Admin API returned status: $($adminHealth.StatusCode)" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Admin API is not responding" -ForegroundColor Red
    Write-Host "  Make sure to run: dotnet run --project src\Presentation\LLMProxy.Admin.API" -ForegroundColor Yellow
}

Write-Host "Testing Gateway..." -ForegroundColor Gray
try {
    $gatewayHealth = Invoke-WebRequest -Uri "http://localhost:5001/health" -UseBasicParsing -TimeoutSec 5
    if ($gatewayHealth.StatusCode -eq 200) {
        Write-Host "✓ Gateway is healthy" -ForegroundColor Green
    } else {
        Write-Host "✗ Gateway returned status: $($gatewayHealth.StatusCode)" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Gateway is not responding" -ForegroundColor Red
    Write-Host "  Make sure to run: dotnet run --project src\Presentation\LLMProxy.Gateway" -ForegroundColor Yellow
}

# Test 2: PostgreSQL
Write-Host ""
Write-Host "[Test 2/5] Database Connection" -ForegroundColor Yellow
try {
    $pgContainer = docker ps --filter "name=postgres" --format "{{.Names}}"
    if ($pgContainer) {
        Write-Host "✓ PostgreSQL container is running" -ForegroundColor Green
        
        # Check if tables exist
        $tables = docker exec $pgContainer psql -U llmproxy -d llmproxy -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public'" 2>$null
        if ($tables -and [int]$tables.Trim() -gt 0) {
            Write-Host "✓ Database schema exists ($($tables.Trim()) tables)" -ForegroundColor Green
        } else {
            Write-Host "✗ Database schema not found" -ForegroundColor Red
            Write-Host "  Run: dotnet ef database update" -ForegroundColor Yellow
        }
    } else {
        Write-Host "✗ PostgreSQL container not running" -ForegroundColor Red
        Write-Host "  Run: docker-compose up -d" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Cannot check PostgreSQL" -ForegroundColor Red
}

# Test 3: Redis
Write-Host ""
Write-Host "[Test 3/5] Cache Connection" -ForegroundColor Yellow
try {
    $redisContainer = docker ps --filter "name=redis" --format "{{.Names}}"
    if ($redisContainer) {
        Write-Host "✓ Redis container is running" -ForegroundColor Green
        
        # Test Redis ping
        $redisPing = docker exec $redisContainer redis-cli ping 2>$null
        if ($redisPing -eq "PONG") {
            Write-Host "✓ Redis is responding" -ForegroundColor Green
        } else {
            Write-Host "✗ Redis not responding" -ForegroundColor Red
        }
    } else {
        Write-Host "✗ Redis container not running" -ForegroundColor Red
        Write-Host "  Run: docker-compose up -d" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Cannot check Redis" -ForegroundColor Red
}

# Test 4: Jaeger
Write-Host ""
Write-Host "[Test 4/5] Telemetry" -ForegroundColor Yellow
try {
    $jaegerContainer = docker ps --filter "name=jaeger" --format "{{.Names}}"
    if ($jaegerContainer) {
        Write-Host "✓ Jaeger container is running" -ForegroundColor Green
        Write-Host "  UI available at: http://localhost:16686" -ForegroundColor Gray
    } else {
        Write-Host "✗ Jaeger container not running" -ForegroundColor Red
        Write-Host "  Run: docker-compose up -d" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Cannot check Jaeger" -ForegroundColor Red
}

# Test 5: API Request (if API key provided)
Write-Host ""
Write-Host "[Test 5/5] API Request Test" -ForegroundColor Yellow
if ($ApiKey) {
    Write-Host "Testing with provided API key..." -ForegroundColor Gray
    try {
        $headers = @{
            "Authorization" = "Bearer $ApiKey"
            "Content-Type" = "application/json"
        }
        $body = @{
            model = "gpt-4"
            messages = @(
                @{ role = "user"; content = "Say hello" }
            )
        } | ConvertTo-Json
        
        $response = Invoke-WebRequest -Uri "http://localhost:5001/v1/chat/completions" -Method POST -Headers $headers -Body $body -UseBasicParsing -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Host "✓ API request successful!" -ForegroundColor Green
            Write-Host "  Response: $($response.Content.Substring(0, [Math]::Min(100, $response.Content.Length)))..." -ForegroundColor Gray
        } else {
            Write-Host "✗ API request failed with status: $($response.StatusCode)" -ForegroundColor Red
        }
    } catch {
        Write-Host "✗ API request failed: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "⊘ Skipped (no API key provided)" -ForegroundColor DarkGray
    Write-Host "  Run with: .\test.ps1 -ApiKey 'your-api-key'" -ForegroundColor Gray
}

# Summary
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Test Complete" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor White
Write-Host "  • View logs in Jaeger: http://localhost:16686" -ForegroundColor Gray
Write-Host "  • Create tenant: See NEXT_STEPS.md" -ForegroundColor Gray
Write-Host "  • Monitor metrics: Check audit_logs table" -ForegroundColor Gray
Write-Host ""
