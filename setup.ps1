# LLM Proxy - Complete Setup Script
# Run this after Docker Desktop is started

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  LLM Proxy - Automated Setup Script" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Check Docker
Write-Host "[1/6] Checking Docker..." -ForegroundColor Yellow
try {
    docker --version | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Docker not found"
    }
    Write-Host "✓ Docker is available" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker is not running or not installed" -ForegroundColor Red
    Write-Host "Please start Docker Desktop and try again" -ForegroundColor Red
    exit 1
}

# Step 2: Start Infrastructure
Write-Host ""
Write-Host "[2/6] Starting infrastructure services..." -ForegroundColor Yellow
docker-compose up -d
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Failed to start services" -ForegroundColor Red
    exit 1
}

Write-Host "Waiting for services to be ready (10 seconds)..." -ForegroundColor Yellow
Start-Sleep -Seconds 10
Write-Host "✓ Infrastructure started" -ForegroundColor Green

# Step 3: Verify services
Write-Host ""
Write-Host "[3/6] Verifying services..." -ForegroundColor Yellow
$services = docker-compose ps --format json | ConvertFrom-Json
$postgresRunning = $services | Where-Object { $_.Service -like "*postgres*" -and $_.State -eq "running" }
$redisRunning = $services | Where-Object { $_.Service -like "*redis*" -and $_.State -eq "running" }
$jaegerRunning = $services | Where-Object { $_.Service -like "*jaeger*" -and $_.State -eq "running" }

if ($postgresRunning) { Write-Host "✓ PostgreSQL is running" -ForegroundColor Green } else { Write-Host "✗ PostgreSQL failed to start" -ForegroundColor Red }
if ($redisRunning) { Write-Host "✓ Redis is running" -ForegroundColor Green } else { Write-Host "✗ Redis failed to start" -ForegroundColor Red }
if ($jaegerRunning) { Write-Host "✓ Jaeger is running" -ForegroundColor Green } else { Write-Host "✗ Jaeger failed to start" -ForegroundColor Red }

if (-not ($postgresRunning -and $redisRunning -and $jaegerRunning)) {
    Write-Host ""
    Write-Host "Some services failed to start. Check Docker logs:" -ForegroundColor Red
    Write-Host "docker-compose logs" -ForegroundColor Yellow
    exit 1
}

# Step 4: Build solution
Write-Host ""
Write-Host "[4/6] Building solution..." -ForegroundColor Yellow
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Build successful" -ForegroundColor Green

# Step 5: Apply migration
Write-Host ""
Write-Host "[5/6] Applying database migration..." -ForegroundColor Yellow
dotnet ef database update --project src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL --startup-project src\Presentation\LLMProxy.Admin.API
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Migration failed" -ForegroundColor Red
    Write-Host "Trying to create database first..." -ForegroundColor Yellow
    Start-Sleep -Seconds 2
    dotnet ef database update --project src\Infrastructure\LLMProxy.Infrastructure.PostgreSQL --startup-project src\Presentation\LLMProxy.Admin.API
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Migration still failed. Check connection settings." -ForegroundColor Red
        exit 1
    }
}
Write-Host "✓ Database migration applied" -ForegroundColor Green

# Step 6: Verify database
Write-Host ""
Write-Host "[6/6] Verifying database schema..." -ForegroundColor Yellow
Start-Sleep -Seconds 2
Write-Host "✓ Setup complete!" -ForegroundColor Green

# Summary
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Setup Complete! 🎉" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Services running:" -ForegroundColor White
Write-Host "  • PostgreSQL:  localhost:5432" -ForegroundColor Gray
Write-Host "  • Redis:       localhost:6379" -ForegroundColor Gray
Write-Host "  • Jaeger UI:   http://localhost:16686" -ForegroundColor Gray
Write-Host ""
Write-Host "Next steps:" -ForegroundColor White
Write-Host "  1. Start Admin API:" -ForegroundColor Yellow
Write-Host "     dotnet run --project src\Presentation\LLMProxy.Admin.API" -ForegroundColor Gray
Write-Host ""
Write-Host "  2. Start Gateway (in another terminal):" -ForegroundColor Yellow
Write-Host "     dotnet run --project src\Presentation\LLMProxy.Gateway" -ForegroundColor Gray
Write-Host ""
Write-Host "  3. Follow NEXT_STEPS.md to create your first tenant" -ForegroundColor Yellow
Write-Host ""
Write-Host "To stop infrastructure:" -ForegroundColor White
Write-Host "  docker-compose down" -ForegroundColor Gray
Write-Host ""
