# Script de démarrage - Frontend + Backend + PostgreSQL
# Facilite le test de la connexion API

Write-Host "`n🚀 DÉMARRAGE ENVIRONNEMENT LLMPROXY`n" -ForegroundColor Cyan

# 1. Vérifier si PostgreSQL tourne
Write-Host "1️⃣ Vérification PostgreSQL..." -ForegroundColor Yellow
$postgresRunning = docker ps --filter "name=environments-postgres" --filter "status=running" --quiet
if ($postgresRunning) {
    Write-Host "   ✅ PostgreSQL déjà en cours d'exécution" -ForegroundColor Green
} else {
    Write-Host "   ⚠️ Démarrage de PostgreSQL..." -ForegroundColor Yellow
    docker-compose up -d postgres
    Write-Host "   ⏳ Attente 5s pour initialisation..." -ForegroundColor Gray
    Start-Sleep -Seconds 5
    Write-Host "   ✅ PostgreSQL démarré" -ForegroundColor Green
}

# 2. Vérifier si l'API backend tourne
Write-Host "`n2️⃣ Vérification API Backend..." -ForegroundColor Yellow
$apiRunning = netstat -ano | findstr ":5001" | Select-String "LISTENING"
if ($apiRunning) {
    Write-Host "   ✅ API Backend déjà en cours d'exécution (port 5001)" -ForegroundColor Green
} else {
    Write-Host "   ⚠️ API Backend non détectée sur port 5001" -ForegroundColor Yellow
    Write-Host "   📝 Pour démarrer manuellement :" -ForegroundColor Gray
    Write-Host "      cd backend\src\Presentation\LLMProxy.Admin.API" -ForegroundColor Gray
    Write-Host "      dotnet run" -ForegroundColor Gray
    Write-Host ""
    $start = Read-Host "   Voulez-vous démarrer l'API maintenant ? (o/N)"
    if ($start -eq 'o' -or $start -eq 'O') {
        Write-Host "   🚀 Démarrage API Backend..." -ForegroundColor Cyan
        Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\..\backend\src\Presentation\LLMProxy.Admin.API'; dotnet run"
        Write-Host "   ⏳ Attente 10s pour démarrage API..." -ForegroundColor Gray
        Start-Sleep -Seconds 10
    }
}

# 3. Configuration du frontend
Write-Host "`n3️⃣ Configuration Frontend..." -ForegroundColor Yellow
$envPath = Join-Path $PSScriptRoot ".env"
if (Test-Path $envPath) {
    $content = Get-Content $envPath -Raw
    if ($content -match "VITE_USE_MOCK_DATA=false") {
        Write-Host "   ✅ Mode API RÉELLE activé" -ForegroundColor Green
    } else {
        Write-Host "   ℹ️  Mode MOCK DATA activé" -ForegroundColor Blue
        Write-Host "   💡 Pour utiliser l'API réelle, modifier .env :" -ForegroundColor Gray
        Write-Host "      VITE_USE_MOCK_DATA=false" -ForegroundColor Gray
    }
} else {
    Write-Host "   ⚠️ Fichier .env non trouvé" -ForegroundColor Yellow
    Write-Host "   📝 Création du fichier .env..." -ForegroundColor Gray
    @"
VITE_API_BASE_URL=/api
VITE_API_VERSION=v2025-12-22
VITE_USE_MOCK_DATA=false
"@ | Set-Content $envPath -Encoding UTF8
    Write-Host "   ✅ Fichier .env créé (mode API réelle)" -ForegroundColor Green
}

# 4. Démarrage du frontend
Write-Host "`n4️⃣ Démarrage Frontend..." -ForegroundColor Yellow
$frontendRunning = netstat -ano | findstr ":3001" | Select-String "LISTENING"
if ($frontendRunning) {
    Write-Host "   ✅ Frontend déjà en cours d'exécution (port 3001)" -ForegroundColor Green
    Write-Host "   🌐 Ouvrir : http://localhost:3001" -ForegroundColor Cyan
} else {
    Write-Host "   🚀 Démarrage du serveur Vite..." -ForegroundColor Cyan
    npm run dev
}

Write-Host "`n✅ ENVIRONNEMENT PRÊT`n" -ForegroundColor Green
Write-Host "📋 URLs utiles :" -ForegroundColor White
Write-Host "   🌐 Frontend :        http://localhost:3001" -ForegroundColor Cyan
Write-Host "   🔌 API Backend :     http://localhost:5001" -ForegroundColor Cyan
Write-Host "   📚 Swagger :         http://localhost:5001/swagger" -ForegroundColor Cyan
Write-Host "   🐘 PostgreSQL :      localhost:15432" -ForegroundColor Cyan
Write-Host ""
