#!/usr/bin/env pwsh
<#
.SYNOPSIS
Build and deploy RimWorld mods to RimWorld Mods folder.

.DESCRIPTION
Builds all mod projects in dependency order, then optionally copies
built assemblies to RimWorld's active Mods folder for testing.

.PARAMETER Deploy
If specified, copies the built mods to RimWorld's active Mods folder.

.PARAMETER Clean
If specified, cleans before building (removes Assemblies).

.EXAMPLE
./build.ps1 -Deploy
# Builds all mods and copies to RimWorld Mods folder

./build.ps1 -Clean
# Cleans and rebuilds all mods
#>
param(
    [switch]$Deploy,
    [switch]$Clean
)

$ErrorActionPreference = 'Stop'

$ModsDir = Split-Path -Parent $PSCommandPath
$RimWorldModsPath = 'D:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods'

# Build order (dependencies first)
$Projects = @(
    'MagicFramework'
    'MFVanilla'
    'AeternusFaith'
)

Write-Host "[BUILD] RimWorld Mods Build Script" -ForegroundColor Cyan
Write-Host "Mods dir: $ModsDir" -ForegroundColor Gray
Write-Host ""

# Clean if requested
if ($Clean) {
    Write-Host "[CLEAN] Cleaning..." -ForegroundColor Yellow
    foreach ($proj in $Projects) {
        $assemblyPath = "$ModsDir\$proj\Assemblies"
        if (Test-Path $assemblyPath) {
            Remove-Item $assemblyPath -Recurse -Force
            Write-Host "  Cleaned $proj"
        }
    }
    Write-Host ""
}

# Build projects
$failed = @()
foreach ($proj in $Projects) {
    $projPath = "$ModsDir\$proj\Source\$proj.csproj"

    if (-not (Test-Path $projPath)) {
        Write-Host "[SKIP] $proj (no project file found)" -ForegroundColor Yellow
        continue
    }

    # Clean obj/bin to avoid file locking issues with Roslyn compiler
    $objPath = Split-Path -Parent $projPath | Join-Path -ChildPath 'obj'
    if (Test-Path $objPath) {
        Remove-Item $objPath -Recurse -Force -ErrorAction SilentlyContinue
    }

    Write-Host "[BUILD] Building $proj..." -ForegroundColor Cyan
    dotnet build $projPath -c Release --nologo -v quiet

    if ($LASTEXITCODE -eq 0) {
        Write-Host "  [OK] $proj built successfully" -ForegroundColor Green
    } else {
        Write-Host "  [ERROR] $proj build failed" -ForegroundColor Red
        $failed += $proj
    }
}

Write-Host ""
if ($failed.Count -gt 0) {
    Write-Host "[ERROR] Build failed: $($failed -join ', ')" -ForegroundColor Red
    exit 1
}

Write-Host "[SUCCESS] All mods built successfully" -ForegroundColor Green

# Deploy if requested
if ($Deploy) {
    Write-Host ""
    Write-Host "[DEPLOY] Deploying to RimWorld..." -ForegroundColor Cyan

    if (-not (Test-Path $RimWorldModsPath)) {
        Write-Host "[ERROR] RimWorld Mods folder not found at: $RimWorldModsPath" -ForegroundColor Red
        Write-Host "  Update the RimWorldModsPath in this script if your installation is elsewhere." -ForegroundColor Gray
        exit 1
    }

    foreach ($proj in $Projects) {
        $srcPath = "$ModsDir\$proj\Assemblies"
        $dstPath = "$RimWorldModsPath\$proj\Assemblies"

        if (-not (Test-Path $srcPath)) {
            Write-Host "  [SKIP] $proj (no assemblies found)" -ForegroundColor Yellow
            continue
        }

        # Create destination if needed
        $dstDir = Split-Path -Parent "$dstPath"
        New-Item -ItemType Directory -Path $dstDir -Force -ErrorAction SilentlyContinue | Out-Null

        # Copy assemblies
        Copy-Item "$srcPath\*" $dstPath -Force
        Write-Host "  [OK] Deployed $proj" -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "[SUCCESS] Ready to test in RimWorld!" -ForegroundColor Green
}

Write-Host ""
Write-Host "Build complete. Use -Deploy flag to copy to RimWorld Mods folder." -ForegroundColor Gray
