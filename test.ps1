#!/usr/bin/env pwsh
<#
.SYNOPSIS
Quick build-and-test workflow: Build, deploy, and optionally launch RimWorld.

.PARAMETER Launch
If specified, launches RimWorld after deploy.

.EXAMPLE
./test.ps1 -Launch
# Builds, deploys, and launches RimWorld
#>
param(
    [switch]$Launch
)

$ModsDir = Split-Path -Parent $PSCommandPath

Write-Host "[WORKFLOW] Build and Test" -ForegroundColor Cyan
Write-Host ""

# Build and deploy
& "$ModsDir\build.ps1" -Deploy

if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Build failed, not launching RimWorld" -ForegroundColor Red
    exit 1
}

Write-Host ""

if ($Launch) {
    Write-Host "[LAUNCH] Starting RimWorld..." -ForegroundColor Cyan
    $RimWorldExe = 'D:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64.exe'

    if (Test-Path $RimWorldExe) {
        & $RimWorldExe
    } else {
        Write-Host "[ERROR] RimWorld executable not found at: $RimWorldExe" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "[COMPLETE] Ready to test!" -ForegroundColor Green
