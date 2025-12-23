---
description: Best practices for Bash, Batch, and PowerShell scripting - robust, maintainable, and testable
name: Scripting_Best_Practices
applyTo: "**/*.{sh,bash,bat,cmd,ps1,psm1}"
---

# Scripting Best Practices - Guide Expert

## ⛔ À NE PAS FAIRE

- **N'écris jamais** de script sans shebang (Bash) ou header de documentation
- **N'ignore jamais** les erreurs (pas de script sans gestion d'erreurs stricte)
- **N'utilise jamais** de variables non initialisées ou non validées
- **Ne hardcode jamais** de chemins absolus ou de credentials
- **N'exécute jamais** un script sans option `--dry-run` disponible
- **Ne supprime jamais** de fichiers sans confirmation ou backup
- **N'utilise jamais** `rm -rf` sans validation de chemin préalable

## ✅ À FAIRE

- **Commence toujours** par le shebang (`#!/bin/bash`) ou header descriptif
- **Active toujours** le mode strict (`set -euo pipefail` Bash, `$ErrorActionPreference = 'Stop'` PS)
- **Valide toujours** tous les arguments avant utilisation
- **Fournis toujours** une option `--dry-run` ou `-WhatIf` pour tester
- **Écris toujours** des scripts idempotents (réexécutables sans effets de bord)
- **Utilise toujours** des exit codes appropriés (0 = succès, >0 = erreur)
- **Documente toujours** avec header : description, usage, exemples, auteur

## 🎯 Actions Obligatoires (Mandatory)

**À TOUJOURS respecter lors de l'écriture de scripts :**

1. ✅ **Shebang obligatoire** : Toujours spécifier l'interpréteur (Bash/Shell)
2. ✅ **Gestion d'erreurs stricte** : `set -e` (Bash) ou `$ErrorActionPreference = 'Stop'` (PowerShell)
3. ✅ **Validation des paramètres** : Vérifier TOUS les arguments avant utilisation
4. ✅ **Messages d'erreur explicites** : Toujours indiquer ce qui a échoué et pourquoi
5. ✅ **Idempotence** : Les scripts doivent être réexécutables sans effets de bord
6. ✅ **Dry-run** : Fournir une option `--dry-run` ou `-WhatIf` pour tester
7. ✅ **Logging** : Logger toutes les actions importantes
8. ✅ **Documentation** : Header avec description, usage, exemples
9. ✅ **Exit codes** : Utiliser des codes de sortie appropriés (0 = succès)
10. ✅ **Tests** : Créer des tests pour valider le comportement

## Structure Standard d'un Script

### Bash Script Template

```bash
#!/bin/bash
################################################################################
# Script Name: example-script.sh
# Description: Brief description of what this script does
# Author: Your Name
# Created: YYYY-MM-DD
# Last Modified: YYYY-MM-DD
# Version: 1.0.0
#
# Usage:
#   ./example-script.sh [OPTIONS] <required_arg>
#
# Options:
#   -h, --help          Display this help message
#   -v, --verbose       Enable verbose output
#   -d, --dry-run       Show what would be done without doing it
#   -f, --force         Force execution without prompts
#
# Examples:
#   ./example-script.sh --dry-run myfile.txt
#   ./example-script.sh -v --force myfile.txt
#
# Exit Codes:
#   0 - Success
#   1 - General error
#   2 - Invalid arguments
#   3 - Required file/resource not found
################################################################################

set -euo pipefail  # Exit on error, undefined vars, pipe failures
IFS=$'\n\t'        # Better word splitting

# =============================================================================
# VARIABLES GLOBALES
# =============================================================================
readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly SCRIPT_NAME="$(basename "${BASH_SOURCE[0]}")"
readonly VERSION="1.0.0"
readonly LOG_FILE="/var/log/${SCRIPT_NAME%.sh}.log"

# Couleurs pour output
readonly RED='\033[0;31m'
readonly GREEN='\033[0;32m'
readonly YELLOW='\033[1;33m'
readonly NC='\033[0m' # No Color

# Options par défaut
VERBOSE=false
DRY_RUN=false
FORCE=false

# =============================================================================
# FONCTIONS UTILITAIRES
# =============================================================================

# Afficher un message d'aide
show_help() {
    sed -n '/^# Usage:/,/^################################################################################$/p' "$0" | sed 's/^# \?//'
}

# Logger un message
log() {
    local level="$1"
    shift
    local message="$*"
    local timestamp
    timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    
    echo "[${timestamp}] [${level}] ${message}" | tee -a "$LOG_FILE"
}

# Afficher un message d'info
info() {
    echo -e "${GREEN}[INFO]${NC} $*"
    log "INFO" "$*"
}

# Afficher un avertissement
warn() {
    echo -e "${YELLOW}[WARN]${NC} $*" >&2
    log "WARN" "$*"
}

# Afficher une erreur et quitter
error() {
    echo -e "${RED}[ERROR]${NC} $*" >&2
    log "ERROR" "$*"
    exit 1
}

# Message de succès
success() {
    echo -e "${GREEN}[SUCCESS]${NC} $*"
    log "SUCCESS" "$*"
}

# Message verbeux (seulement si verbose activé)
debug() {
    if [[ "$VERBOSE" == true ]]; then
        echo -e "[DEBUG] $*"
        log "DEBUG" "$*"
    fi
}

# Vérifier si une commande existe
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Vérifier les dépendances requises
check_dependencies() {
    local missing_deps=()
    
    for cmd in "$@"; do
        if ! command_exists "$cmd"; then
            missing_deps+=("$cmd")
        fi
    done
    
    if [[ ${#missing_deps[@]} -gt 0 ]]; then
        error "Missing required dependencies: ${missing_deps[*]}"
    fi
}

# Demander confirmation à l'utilisateur
confirm() {
    if [[ "$FORCE" == true ]]; then
        return 0
    fi
    
    local prompt="${1:-Are you sure?}"
    local response
    
    read -r -p "${prompt} [y/N] " response
    case "$response" in
        [yY][eE][sS]|[yY]) 
            return 0
            ;;
        *)
            return 1
            ;;
    esac
}

# Cleanup à la sortie
cleanup() {
    local exit_code=$?
    debug "Cleaning up..."
    
    # Ajouter ici les opérations de nettoyage
    # rm -f /tmp/tempfile-$$
    
    if [[ $exit_code -eq 0 ]]; then
        success "Script completed successfully"
    else
        error "Script failed with exit code: $exit_code"
    fi
}

trap cleanup EXIT

# =============================================================================
# FONCTIONS PRINCIPALES
# =============================================================================

# Valider les arguments
validate_args() {
    if [[ $# -lt 1 ]]; then
        error "Missing required argument. Use --help for usage information."
    fi
    
    local file="$1"
    
    if [[ ! -f "$file" ]]; then
        error "File not found: $file"
    fi
    
    if [[ ! -r "$file" ]]; then
        error "File not readable: $file"
    fi
}

# Fonction principale du script
main_function() {
    local file="$1"
    
    info "Processing file: $file"
    
    if [[ "$DRY_RUN" == true ]]; then
        info "[DRY-RUN] Would process: $file"
        return 0
    fi
    
    # Logique principale ici
    debug "Executing main logic..."
    
    # Exemple d'opération idempotente
    if [[ -f "$file.backup" ]]; then
        warn "Backup already exists: $file.backup"
    else
        cp "$file" "$file.backup"
        info "Created backup: $file.backup"
    fi
    
    success "Processing completed for: $file"
}

# =============================================================================
# PARSING DES ARGUMENTS
# =============================================================================

parse_args() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            -h|--help)
                show_help
                exit 0
                ;;
            -v|--verbose)
                VERBOSE=true
                shift
                ;;
            -d|--dry-run)
                DRY_RUN=true
                info "DRY-RUN mode enabled"
                shift
                ;;
            -f|--force)
                FORCE=true
                shift
                ;;
            --version)
                echo "$SCRIPT_NAME version $VERSION"
                exit 0
                ;;
            -*)
                error "Unknown option: $1"
                ;;
            *)
                # Arguments positionnels
                POSITIONAL_ARGS+=("$1")
                shift
                ;;
        esac
    done
}

# =============================================================================
# POINT D'ENTRÉE
# =============================================================================

main() {
    local -a POSITIONAL_ARGS=()
    
    # Parser les arguments
    parse_args "$@"
    
    # Restaurer les arguments positionnels
    set -- "${POSITIONAL_ARGS[@]}"
    
    # Vérifier les dépendances
    check_dependencies cp date
    
    # Valider les arguments
    validate_args "$@"
    
    # Exécuter la fonction principale
    main_function "$@"
}

# Exécuter le script si appelé directement
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi
```

### PowerShell Script Template

```powershell
<#
.SYNOPSIS
    Brief description of what this script does

.DESCRIPTION
    Detailed description of the script's functionality, purpose, and behavior.
    This script follows PowerShell best practices and includes proper error handling.

.PARAMETER InputPath
    Path to the input file or directory

.PARAMETER OutputPath
    Path where output will be written

.PARAMETER Force
    Forces execution without prompts

.PARAMETER WhatIf
    Shows what would happen if the script runs (dry-run mode)

.PARAMETER Verbose
    Enables verbose output

.EXAMPLE
    .\Example-Script.ps1 -InputPath "C:\input.txt" -OutputPath "C:\output.txt"
    
    Processes input.txt and writes to output.txt

.EXAMPLE
    .\Example-Script.ps1 -InputPath "C:\input.txt" -WhatIf -Verbose
    
    Shows what would happen without actually doing it, with verbose output

.NOTES
    Author: Your Name
    Created: 2025-11-27
    Version: 1.0.0
    
    Exit Codes:
        0 - Success
        1 - General error
        2 - Invalid parameters
        3 - Required resource not found

.LINK
    https://your-documentation-url.com
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Mandatory = $true, Position = 0, HelpMessage = "Path to input file")]
    [ValidateNotNullOrEmpty()]
    [ValidateScript({ Test-Path $_ -PathType Leaf })]
    [string]$InputPath,
    
    [Parameter(Mandatory = $false, Position = 1)]
    [ValidateNotNullOrEmpty()]
    [string]$OutputPath,
    
    [Parameter(Mandatory = $false)]
    [switch]$Force
)

################################################################################
# CONFIGURATION GLOBALE
################################################################################

# Configuration stricte des erreurs
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

# Constantes
Set-Variable -Name SCRIPT_VERSION -Value "1.0.0" -Option Constant
Set-Variable -Name SCRIPT_NAME -Value $MyInvocation.MyCommand.Name -Option Constant
Set-Variable -Name SCRIPT_PATH -Value $PSScriptRoot -Option Constant

# Configuration du logging
$LogPath = Join-Path $env:TEMP "$($SCRIPT_NAME).log"
$StartTime = Get-Date

################################################################################
# FONCTIONS UTILITAIRES
################################################################################

function Write-Log {
    <#
    .SYNOPSIS
        Writes a message to the log file and console
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message,
        
        [Parameter(Mandatory = $false)]
        [ValidateSet('Info', 'Warning', 'Error', 'Success', 'Debug')]
        [string]$Level = 'Info'
    )
    
    $Timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    $LogMessage = "[$Timestamp] [$Level] $Message"
    
    # Écrire dans le fichier log
    Add-Content -Path $LogPath -Value $LogMessage -ErrorAction SilentlyContinue
    
    # Afficher dans la console avec couleur
    switch ($Level) {
        'Info'    { Write-Host $LogMessage -ForegroundColor Cyan }
        'Warning' { Write-Warning $Message }
        'Error'   { Write-Error $Message }
        'Success' { Write-Host $LogMessage -ForegroundColor Green }
        'Debug'   { Write-Debug $Message }
    }
}

function Test-Administrator {
    <#
    .SYNOPSIS
        Checks if script is running with administrator privileges
    #>
    $currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    return $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Test-Dependencies {
    <#
    .SYNOPSIS
        Verifies that required commands/modules are available
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Commands,
        
        [Parameter(Mandatory = $false)]
        [string[]]$Modules
    )
    
    $missingCommands = @()
    $missingModules = @()
    
    foreach ($command in $Commands) {
        if (-not (Get-Command $command -ErrorAction SilentlyContinue)) {
            $missingCommands += $command
        }
    }
    
    foreach ($module in $Modules) {
        if (-not (Get-Module -Name $module -ListAvailable -ErrorAction SilentlyContinue)) {
            $missingModules += $module
        }
    }
    
    if ($missingCommands.Count -gt 0) {
        throw "Missing required commands: $($missingCommands -join ', ')"
    }
    
    if ($missingModules.Count -gt 0) {
        throw "Missing required modules: $($missingModules -join ', '). Install with: Install-Module -Name <ModuleName>"
    }
    
    Write-Log "All dependencies verified successfully" -Level Success
}

function Invoke-WithRetry {
    <#
    .SYNOPSIS
        Executes a script block with retry logic
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$ScriptBlock,
        
        [Parameter(Mandatory = $false)]
        [int]$MaxRetries = 3,
        
        [Parameter(Mandatory = $false)]
        [int]$DelaySeconds = 5
    )
    
    $attempt = 1
    
    while ($attempt -le $MaxRetries) {
        try {
            Write-Log "Attempt $attempt of $MaxRetries" -Level Debug
            return & $ScriptBlock
        }
        catch {
            if ($attempt -eq $MaxRetries) {
                Write-Log "All retry attempts failed" -Level Error
                throw
            }
            
            Write-Log "Attempt $attempt failed: $_. Retrying in $DelaySeconds seconds..." -Level Warning
            Start-Sleep -Seconds $DelaySeconds
            $attempt++
        }
    }
}

function Backup-File {
    <#
    .SYNOPSIS
        Creates a backup of a file (idempotent)
    #>
    [CmdletBinding(SupportsShouldProcess = $true)]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateScript({ Test-Path $_ -PathType Leaf })]
        [string]$FilePath
    )
    
    $backupPath = "$FilePath.backup"
    
    if (Test-Path $backupPath) {
        Write-Log "Backup already exists: $backupPath" -Level Warning
        return $backupPath
    }
    
    if ($PSCmdlet.ShouldProcess($FilePath, "Create backup")) {
        Copy-Item -Path $FilePath -Destination $backupPath -Force
        Write-Log "Created backup: $backupPath" -Level Success
        return $backupPath
    }
}

function Get-FormattedSize {
    <#
    .SYNOPSIS
        Converts bytes to human-readable format
    #>
    param([long]$Bytes)
    
    $sizes = 'B', 'KB', 'MB', 'GB', 'TB'
    $order = 0
    
    while ($Bytes -ge 1024 -and $order -lt $sizes.Length - 1) {
        $order++
        $Bytes = $Bytes / 1024
    }
    
    return "{0:N2} {1}" -f $Bytes, $sizes[$order]
}

################################################################################
# FONCTIONS PRINCIPALES
################################################################################

function Invoke-MainLogic {
    <#
    .SYNOPSIS
        Main logic of the script
    #>
    [CmdletBinding(SupportsShouldProcess = $true)]
    param(
        [Parameter(Mandatory = $true)]
        [string]$InputPath,
        
        [Parameter(Mandatory = $false)]
        [string]$OutputPath
    )
    
    try {
        Write-Log "Starting main logic" -Level Info
        Write-Log "Input: $InputPath" -Level Debug
        
        # Vérifier si le fichier existe (déjà fait par ValidateScript, mais bon exemple)
        if (-not (Test-Path $InputPath)) {
            throw "Input file not found: $InputPath"
        }
        
        # Obtenir des infos sur le fichier
        $fileInfo = Get-Item $InputPath
        $fileSize = Get-FormattedSize $fileInfo.Length
        Write-Log "Processing file: $($fileInfo.Name) ($fileSize)" -Level Info
        
        # Créer un backup (idempotent)
        if ($PSCmdlet.ShouldProcess($InputPath, "Create backup")) {
            $backupPath = Backup-File -FilePath $InputPath
            Write-Log "Backup created at: $backupPath" -Level Success
        }
        
        # Logique principale ici
        if ($PSCmdlet.ShouldProcess($InputPath, "Process file")) {
            Write-Log "Processing file content..." -Level Info
            
            # Exemple : lire et traiter le contenu
            $content = Get-Content -Path $InputPath -Raw
            
            # Déterminer le chemin de sortie
            if (-not $OutputPath) {
                $OutputPath = "$InputPath.processed"
            }
            
            # Écrire le résultat
            Set-Content -Path $OutputPath -Value $content -Force
            Write-Log "Output written to: $OutputPath" -Level Success
        }
        
        return @{
            Success = $true
            InputPath = $InputPath
            OutputPath = $OutputPath
        }
    }
    catch {
        Write-Log "Error in main logic: $_" -Level Error
        throw
    }
}

################################################################################
# VALIDATION ET INITIALISATION
################################################################################

function Initialize-Script {
    <#
    .SYNOPSIS
        Initializes script environment and validates prerequisites
    #>
    
    Write-Log "=== $SCRIPT_NAME v$SCRIPT_VERSION ===" -Level Info
    Write-Log "Script started at: $StartTime" -Level Debug
    Write-Log "Running as: $env:USERNAME on $env:COMPUTERNAME" -Level Debug
    
    # Vérifier les dépendances
    # Test-Dependencies -Commands @('git', 'dotnet') -Modules @('Pester')
    
    # Vérifier les privilèges si nécessaire
    # if (-not (Test-Administrator)) {
    #     throw "This script requires administrator privileges"
    # }
    
    Write-Log "Initialization complete" -Level Success
}

################################################################################
# CLEANUP
################################################################################

function Invoke-Cleanup {
    <#
    .SYNOPSIS
        Cleanup operations before exit
    #>
    param([bool]$Success = $true)
    
    $duration = (Get-Date) - $StartTime
    
    Write-Log "Cleanup started" -Level Debug
    
    # Ajouter les opérations de nettoyage ici
    
    if ($Success) {
        Write-Log "Script completed successfully in $($duration.TotalSeconds) seconds" -Level Success
    }
    else {
        Write-Log "Script failed after $($duration.TotalSeconds) seconds" -Level Error
    }
}

################################################################################
# POINT D'ENTRÉE PRINCIPAL
################################################################################

try {
    # Initialiser
    Initialize-Script
    
    # Exécuter la logique principale
    $result = Invoke-MainLogic -InputPath $InputPath -OutputPath $OutputPath
    
    # Cleanup avec succès
    Invoke-Cleanup -Success $true
    
    # Exit avec succès
    exit 0
}
catch {
    Write-Log "Fatal error: $_" -Level Error
    Write-Log "Stack trace: $($_.ScriptStackTrace)" -Level Debug
    
    # Cleanup avec échec
    Invoke-Cleanup -Success $false
    
    # Exit avec erreur
    exit 1
}
finally {
    # Code toujours exécuté
    Write-Log "Log file: $LogPath" -Level Debug
}
```

### Batch Script Template

```batch
@ECHO OFF
REM ============================================================================
REM Script Name: example-script.bat
REM Description: Brief description of what this script does
REM Author: Your Name
REM Created: 2025-11-27
REM Version: 1.0.0
REM
REM Usage:
REM   example-script.bat [OPTIONS] <required_arg>
REM
REM Options:
REM   /h, /help       Display this help message
REM   /v, /verbose    Enable verbose output
REM   /d, /dryrun     Show what would be done without doing it
REM
REM Exit Codes:
REM   0 - Success
REM   1 - General error
REM   2 - Invalid arguments
REM ============================================================================

SETLOCAL EnableDelayedExpansion

REM ============================================================================
REM CONFIGURATION
REM ============================================================================
SET "SCRIPT_NAME=%~n0"
SET "SCRIPT_DIR=%~dp0"
SET "SCRIPT_VERSION=1.0.0"
SET "LOG_FILE=%TEMP%\%SCRIPT_NAME%.log"

SET "VERBOSE=0"
SET "DRY_RUN=0"

REM ============================================================================
REM FONCTIONS
REM ============================================================================

:Log
    SET "LEVEL=%~1"
    SET "MESSAGE=%~2"
    SET "TIMESTAMP=%DATE% %TIME%"
    ECHO [%TIMESTAMP%] [%LEVEL%] %MESSAGE% >> "%LOG_FILE%"
    ECHO [%LEVEL%] %MESSAGE%
    EXIT /B 0

:Info
    CALL :Log "INFO" "%~1"
    EXIT /B 0

:Warn
    CALL :Log "WARN" "%~1"
    EXIT /B 0

:Error
    CALL :Log "ERROR" "%~1"
    EXIT /B 1

:Success
    CALL :Log "SUCCESS" "%~1"
    EXIT /B 0

:Debug
    IF "%VERBOSE%"=="1" (
        CALL :Log "DEBUG" "%~1"
    )
    EXIT /B 0

:ShowHelp
    ECHO.
    ECHO %SCRIPT_NAME% v%SCRIPT_VERSION%
    ECHO.
    ECHO Usage:
    ECHO   %SCRIPT_NAME% [OPTIONS] ^<input_file^>
    ECHO.
    ECHO Options:
    ECHO   /h, /help       Display this help message
    ECHO   /v, /verbose    Enable verbose output
    ECHO   /d, /dryrun     Dry-run mode
    ECHO.
    ECHO Example:
    ECHO   %SCRIPT_NAME% /v myfile.txt
    ECHO.
    EXIT /B 0

:CheckDependencies
    WHERE git >nul 2>&1
    IF ERRORLEVEL 1 (
        CALL :Error "Required command 'git' not found"
        EXIT /B 1
    )
    EXIT /B 0

:ValidateArgs
    IF "%~1"=="" (
        CALL :Error "Missing required argument: input_file"
        EXIT /B 2
    )
    
    IF NOT EXIST "%~1" (
        CALL :Error "File not found: %~1"
        EXIT /B 3
    )
    EXIT /B 0

:MainFunction
    SET "INPUT_FILE=%~1"
    
    CALL :Info "Processing file: %INPUT_FILE%"
    
    IF "%DRY_RUN%"=="1" (
        CALL :Info "[DRY-RUN] Would process: %INPUT_FILE%"
        EXIT /B 0
    )
    
    REM Logique principale ici
    CALL :Debug "Executing main logic..."
    
    REM Exemple d'opération idempotente - créer backup
    IF EXIST "%INPUT_FILE%.backup" (
        CALL :Warn "Backup already exists: %INPUT_FILE%.backup"
    ) ELSE (
        COPY "%INPUT_FILE%" "%INPUT_FILE%.backup" >nul
        CALL :Info "Created backup: %INPUT_FILE%.backup"
    )
    
    CALL :Success "Processing completed"
    EXIT /B 0

REM ============================================================================
REM PARSING DES ARGUMENTS
REM ============================================================================

:ParseArgs
    IF "%~1"=="" GOTO :EndParse
    
    IF /I "%~1"=="/h" GOTO :Help
    IF /I "%~1"=="/help" GOTO :Help
    IF /I "%~1"=="/v" (
        SET "VERBOSE=1"
        SHIFT
        GOTO :ParseArgs
    )
    IF /I "%~1"=="/verbose" (
        SET "VERBOSE=1"
        SHIFT
        GOTO :ParseArgs
    )
    IF /I "%~1"=="/d" (
        SET "DRY_RUN=1"
        CALL :Info "DRY-RUN mode enabled"
        SHIFT
        GOTO :ParseArgs
    )
    IF /I "%~1"=="/dryrun" (
        SET "DRY_RUN=1"
        CALL :Info "DRY-RUN mode enabled"
        SHIFT
        GOTO :ParseArgs
    )
    
    REM Argument positionnel
    SET "POSITIONAL_ARG=%~1"
    SHIFT
    GOTO :ParseArgs
    
:Help
    CALL :ShowHelp
    EXIT /B 0

:EndParse
    EXIT /B 0

REM ============================================================================
REM POINT D'ENTRÉE
REM ============================================================================

:Main
    CALL :Info "=== %SCRIPT_NAME% v%SCRIPT_VERSION% ==="
    
    REM Parser les arguments
    CALL :ParseArgs %*
    
    REM Vérifier les dépendances
    REM CALL :CheckDependencies
    REM IF ERRORLEVEL 1 EXIT /B 1
    
    REM Valider les arguments
    CALL :ValidateArgs "%POSITIONAL_ARG%"
    IF ERRORLEVEL 1 EXIT /B %ERRORLEVEL%
    
    REM Exécuter la fonction principale
    CALL :MainFunction "%POSITIONAL_ARG%"
    IF ERRORLEVEL 1 EXIT /B %ERRORLEVEL%
    
    CALL :Success "Script completed successfully"
    EXIT /B 0

REM Exécuter le point d'entrée
CALL :Main %*
SET EXIT_CODE=%ERRORLEVEL%

REM Cleanup
CALL :Debug "Log file: %LOG_FILE%"

EXIT /B %EXIT_CODE%
```

## Bonnes Pratiques Communes

### 1. Gestion d'Erreurs Stricte

```bash
# Bash - Arrêter immédiatement en cas d'erreur
set -euo pipefail

# Bash - Capturer les erreurs avec trap
trap 'echo "Error on line $LINENO"' ERR
```

```powershell
# PowerShell - Configuration stricte
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# PowerShell - Try/Catch systématique
try {
    # Code risqué
}
catch {
    Write-Error "Operation failed: $_"
    exit 1
}
```

```batch
REM Batch - Vérifier ERRORLEVEL après chaque commande
COMMAND
IF ERRORLEVEL 1 (
    ECHO Command failed
    EXIT /B 1
)
```

### 2. Idempotence

```bash
# ✅ BON - Opération idempotente
if [[ ! -f "$BACKUP_FILE" ]]; then
    cp "$SOURCE_FILE" "$BACKUP_FILE"
fi

# ❌ MAUVAIS - Non idempotent
cp "$SOURCE_FILE" "$BACKUP_FILE"  # Écrase à chaque fois
```

```powershell
# ✅ BON - Vérifier avant de créer
if (-not (Test-Path $BackupPath)) {
    Copy-Item $SourcePath $BackupPath
}

# ✅ BON - Utiliser -Force de manière contrôlée
Copy-Item $SourcePath $BackupPath -Force:$false -ErrorAction SilentlyContinue
```

### 3. Validation des Paramètres

```bash
# Bash - Validation stricte
validate_file() {
    local file="$1"
    
    [[ -z "$file" ]] && error "File parameter is empty"
    [[ ! -e "$file" ]] && error "File does not exist: $file"
    [[ ! -f "$file" ]] && error "Not a regular file: $file"
    [[ ! -r "$file" ]] && error "File not readable: $file"
}
```

```powershell
# PowerShell - Utiliser ValidateScript
[Parameter(Mandatory = $true)]
[ValidateScript({
    if (-not (Test-Path $_)) {
        throw "File does not exist: $_"
    }
    if (-not (Test-Path $_ -PathType Leaf)) {
        throw "Path is not a file: $_"
    }
    $true
})]
[string]$FilePath
```

### 4. Dry-Run / WhatIf

```bash
# Bash - Implémenter dry-run
if [[ "$DRY_RUN" == true ]]; then
    echo "[DRY-RUN] Would execute: rm -rf $DIR"
else
    rm -rf "$DIR"
fi
```

```powershell
# PowerShell - Utiliser SupportsShouldProcess
[CmdletBinding(SupportsShouldProcess = $true)]
param()

if ($PSCmdlet.ShouldProcess($Target, $Operation)) {
    # Exécuter l'opération
}
```

### 5. Logging Structuré

```bash
# Bash - Logging avec rotation
log() {
    local level="$1"
    shift
    local message="$*"
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    
    echo "[${timestamp}] [${level}] ${message}" | tee -a "$LOG_FILE"
    
    # Rotation si fichier trop gros
    if [[ -f "$LOG_FILE" ]] && [[ $(stat -f%z "$LOG_FILE" 2>/dev/null || stat -c%s "$LOG_FILE") -gt 10485760 ]]; then
        mv "$LOG_FILE" "$LOG_FILE.old"
    fi
}
```

```powershell
# PowerShell - Logging avec CMDlet
function Write-Log {
    param(
        [string]$Message,
        [ValidateSet('Info','Warning','Error')]
        [string]$Level = 'Info'
    )
    
    $LogEntry = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] [$Level] $Message"
    Add-Content -Path $LogFile -Value $LogEntry
    
    switch ($Level) {
        'Warning' { Write-Warning $Message }
        'Error'   { Write-Error $Message }
        default   { Write-Host $LogEntry }
    }
}
```

## Tests de Scripts

### Test Bash avec Bats

```bash
# test_example.bats
#!/usr/bin/env bats

setup() {
    # Setup avant chaque test
    export TEST_DIR="$(mktemp -d)"
    export SCRIPT_PATH="./example-script.sh"
}

teardown() {
    # Cleanup après chaque test
    rm -rf "$TEST_DIR"
}

@test "Script existe et est exécutable" {
    [ -f "$SCRIPT_PATH" ]
    [ -x "$SCRIPT_PATH" ]
}

@test "Script échoue sans arguments" {
    run "$SCRIPT_PATH"
    [ "$status" -ne 0 ]
}

@test "Script réussit avec argument valide" {
    touch "$TEST_DIR/test.txt"
    run "$SCRIPT_PATH" "$TEST_DIR/test.txt"
    [ "$status" -eq 0 ]
}

@test "Script est idempotent" {
    touch "$TEST_DIR/test.txt"
    
    # Première exécution
    run "$SCRIPT_PATH" "$TEST_DIR/test.txt"
    [ "$status" -eq 0 ]
    
    # Deuxième exécution (doit aussi réussir)
    run "$SCRIPT_PATH" "$TEST_DIR/test.txt"
    [ "$status" -eq 0 ]
}

@test "Dry-run ne modifie rien" {
    touch "$TEST_DIR/test.txt"
    local checksum_before=$(md5sum "$TEST_DIR/test.txt")
    
    run "$SCRIPT_PATH" --dry-run "$TEST_DIR/test.txt"
    
    local checksum_after=$(md5sum "$TEST_DIR/test.txt")
    [ "$checksum_before" = "$checksum_after" ]
}
```

### Test PowerShell avec Pester

```powershell
# Example-Script.Tests.ps1
Describe 'Example-Script Tests' {
    BeforeAll {
        $ScriptPath = Join-Path $PSScriptRoot 'Example-Script.ps1'
        $TestDir = Join-Path $TestDrive 'TestFiles'
        New-Item -Path $TestDir -ItemType Directory -Force
    }
    
    Context 'Parameter Validation' {
        It 'Should fail with missing InputPath' {
            { & $ScriptPath } | Should -Throw
        }
        
        It 'Should fail with non-existent file' {
            { & $ScriptPath -InputPath 'C:\NonExistent.txt' } | Should -Throw
        }
        
        It 'Should accept valid file path' {
            $testFile = Join-Path $TestDir 'test.txt'
            'test content' | Set-Content $testFile
            
            { & $ScriptPath -InputPath $testFile -WhatIf } | Should -Not -Throw
        }
    }
    
    Context 'Idempotence' {
        It 'Should be idempotent' {
            $testFile = Join-Path $TestDir 'idempotent.txt'
            'test content' | Set-Content $testFile
            
            # Première exécution
            $result1 = & $ScriptPath -InputPath $testFile
            
            # Deuxième exécution (ne doit pas échouer)
            $result2 = & $ScriptPath -InputPath $testFile
            
            $result2.Success | Should -Be $true
        }
    }
    
    Context 'WhatIf Support' {
        It 'Should support WhatIf without making changes' {
            $testFile = Join-Path $TestDir 'whatif.txt'
            $originalContent = 'original content'
            $originalContent | Set-Content $testFile
            
            & $ScriptPath -InputPath $testFile -WhatIf
            
            $newContent = Get-Content $testFile -Raw
            $newContent.Trim() | Should -Be $originalContent
        }
    }
    
    Context 'Error Handling' {
        It 'Should handle errors gracefully' {
            Mock Write-Log { throw "Test error" } -ModuleName Example-Script
            
            { & $ScriptPath -InputPath 'test.txt' -ErrorAction Stop } | Should -Throw
        }
    }
}
```

## Checklist Scripts

### Avant de Committer

- [ ] **✅ Shebang présent** (Bash/Shell)
- [ ] **✅ Header de documentation complet**
- [ ] **✅ Gestion d'erreurs stricte** (`set -e` ou `$ErrorActionPreference`)
- [ ] **✅ Validation de TOUS les paramètres**
- [ ] **✅ Messages d'erreur explicites**
- [ ] **✅ Logging activé**
- [ ] **✅ Option dry-run/WhatIf implémentée**
- [ ] **✅ Script idempotent (réexécutable sans effets de bord)**
- [ ] **✅ Codes de sortie appropriés**
- [ ] **✅ Cleanup dans trap/finally**
- [ ] **✅ Tests créés et passants**
- [ ] **✅ Test d'idempotence passé**
- [ ] **✅ Dry-run testé**
- [ ] **✅ Compatible avec shellcheck (Bash) ou PSScriptAnalyzer (PowerShell)**

### Validation Automatique

```bash
#!/bin/bash
# validate-script.sh - Valider un script

SCRIPT="$1"

echo "=== Validating Script: $SCRIPT ==="

# Test 1: Shellcheck (pour Bash)
if [[ "$SCRIPT" =~ \.(sh|bash)$ ]]; then
    echo "[1/5] Running shellcheck..."
    shellcheck "$SCRIPT" || exit 1
fi

# Test 2: PSScriptAnalyzer (pour PowerShell)
if [[ "$SCRIPT" =~ \.ps1$ ]]; then
    echo "[1/5] Running PSScriptAnalyzer..."
    pwsh -Command "Invoke-ScriptAnalyzer -Path '$SCRIPT' -Severity Error" || exit 1
fi

# Test 3: Syntax check
echo "[2/5] Checking syntax..."
if [[ "$SCRIPT" =~ \.(sh|bash)$ ]]; then
    bash -n "$SCRIPT" || exit 1
elif [[ "$SCRIPT" =~ \.ps1$ ]]; then
    pwsh -Command "Get-Content '$SCRIPT' | Out-Null" || exit 1
fi

# Test 4: Vérifier la présence de dry-run
echo "[3/5] Checking dry-run support..."
if [[ "$SCRIPT" =~ \.(sh|bash)$ ]]; then
    grep -q "DRY.RUN\|dry.run" "$SCRIPT" || echo "⚠️  Warning: No dry-run support detected"
elif [[ "$SCRIPT" =~ \.ps1$ ]]; then
    grep -q "ShouldProcess\|WhatIf" "$SCRIPT" || echo "⚠️  Warning: No WhatIf support detected"
fi

# Test 5: Vérifier la gestion d'erreurs
echo "[4/5] Checking error handling..."
if [[ "$SCRIPT" =~ \.(sh|bash)$ ]]; then
    grep -q "set -e" "$SCRIPT" || echo "⚠️  Warning: 'set -e' not found"
elif [[ "$SCRIPT" =~ \.ps1$ ]]; then
    grep -q "ErrorActionPreference.*Stop" "$SCRIPT" || echo "⚠️  Warning: ErrorActionPreference not set to Stop"
fi

# Test 6: Exécuter les tests
echo "[5/5] Running tests..."
if [[ -f "${SCRIPT%.sh}.bats" ]]; then
    bats "${SCRIPT%.sh}.bats" || exit 1
elif [[ -f "${SCRIPT%.ps1}.Tests.ps1" ]]; then
    pwsh -Command "Invoke-Pester '${SCRIPT%.ps1}.Tests.ps1'" || exit 1
fi

echo "✅ All validations passed!"
```

## Anti-Patterns à Éviter

```bash
# ❌ Pas de gestion d'erreurs
command_that_might_fail
other_command  # S'exécute même si la première échoue

# ✅ Avec gestion d'erreurs
set -e
command_that_might_fail
other_command  # Ne s'exécute que si la première réussit

# ❌ Variables non quotées
rm -rf $DIR/*  # Dangereux si $DIR est vide!

# ✅ Variables quotées
rm -rf "${DIR:?}/"*  # Échoue si $DIR n'est pas défini

# ❌ Pas de validation
process_file "$1"

# ✅ Avec validation
[[ -z "$1" ]] && { echo "Error: Missing argument"; exit 1; }
[[ ! -f "$1" ]] && { echo "Error: File not found: $1"; exit 1; }
process_file "$1"

# ❌ Commandes hardcodées
rm -rf /var/app/data

# ✅ Avec confirmation
if confirm "Delete /var/app/data?"; then
    rm -rf /var/app/data
fi
```

**🎯 RÈGLE D'OR : Tout script doit être testable, idempotent, et fournir un mode dry-run**
