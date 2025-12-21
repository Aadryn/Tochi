# Quick Start - Run Both Services
# This script starts both Admin API and Gateway in split terminals

Write-Host "Starting LLM Proxy services..." -ForegroundColor Cyan
Write-Host ""

# Start Admin API in background
Write-Host "Starting Admin API on port 5000..." -ForegroundColor Yellow
$adminJob = Start-Job -ScriptBlock {
    Set-Location "D:\workspaces\sandbox\proxy"
    dotnet run --project src\Presentation\LLMProxy.Admin.API
}

# Wait a bit for Admin API to start
Start-Sleep -Seconds 3

# Start Gateway in background
Write-Host "Starting Gateway on port 5001..." -ForegroundColor Yellow
$gatewayJob = Start-Job -ScriptBlock {
    Set-Location "D:\workspaces\sandbox\proxy"
    dotnet run --project src\Presentation\LLMProxy.Gateway
}

# Wait for services to initialize
Start-Sleep -Seconds 5

Write-Host ""
Write-Host "✓ Services started!" -ForegroundColor Green
Write-Host ""
Write-Host "Admin API:  http://localhost:5000/health" -ForegroundColor Cyan
Write-Host "Gateway:    http://localhost:5001/health" -ForegroundColor Cyan
Write-Host "Jaeger UI:  http://localhost:16686" -ForegroundColor Cyan
Write-Host ""
Write-Host "Monitoring output..." -ForegroundColor Yellow
Write-Host "Press Ctrl+C to stop both services" -ForegroundColor Yellow
Write-Host ""

# Monitor both jobs
try {
    while ($true) {
        # Show Admin API output
        $adminOutput = Receive-Job -Job $adminJob
        if ($adminOutput) {
            Write-Host "[Admin API] " -ForegroundColor Magenta -NoNewline
            Write-Host $adminOutput
        }
        
        # Show Gateway output
        $gatewayOutput = Receive-Job -Job $gatewayJob
        if ($gatewayOutput) {
            Write-Host "[Gateway]   " -ForegroundColor Cyan -NoNewline
            Write-Host $gatewayOutput
        }
        
        # Check if jobs are still running
        if ($adminJob.State -eq "Failed" -or $gatewayJob.State -eq "Failed") {
            Write-Host ""
            Write-Host "✗ One or more services failed!" -ForegroundColor Red
            break
        }
        
        Start-Sleep -Milliseconds 500
    }
} finally {
    # Cleanup on exit
    Write-Host ""
    Write-Host "Stopping services..." -ForegroundColor Yellow
    Stop-Job -Job $adminJob, $gatewayJob
    Remove-Job -Job $adminJob, $gatewayJob
    Write-Host "✓ Services stopped" -ForegroundColor Green
}
