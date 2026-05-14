#!/usr/bin/env pwsh
<#
.SYNOPSIS
Synchronize static documentation web assets.

.DESCRIPTION
Copies the source SpellForge files from the workspace-level SpellDefBuilder folder into
the tracked docs site under Mods/docs/spell-def-builder. The docs copy needs shorter
relative guide links, so index.html is transformed during sync. Use -Check in CI or
before publishing to verify the docs copy has not drifted.

.PARAMETER Check
Compare files without copying. Exits with 1 if drift is detected.

.EXAMPLE
./sync-docs.ps1

.EXAMPLE
./sync-docs.ps1 -Check
#>
param(
    [switch]$Check
)

$ErrorActionPreference = 'Stop'

$ModsDir = Split-Path -Parent $PSCommandPath
$WorkspaceDir = Split-Path -Parent $ModsDir
$BuilderSource = Join-Path $WorkspaceDir 'SpellDefBuilder'
$BuilderDocs = Join-Path $ModsDir 'docs\spell-def-builder'
$Files = @('index.html', 'style.css', 'script.js')

if (-not (Test-Path $BuilderSource)) {
    Write-Host "[ERROR] SpellDefBuilder source folder not found: $BuilderSource" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $BuilderDocs)) {
    Write-Host "[ERROR] Docs SpellForge folder not found: $BuilderDocs" -ForegroundColor Red
    exit 1
}

function Get-DocsContent {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourcePath,

        [Parameter(Mandatory = $true)]
        [string]$FileName
    )

    $content = Get-Content -LiteralPath $SourcePath -Raw

    if ($FileName -eq 'index.html') {
        $content = $content -replace '\.\./Mods/docs/spell-design-guide/index\.html', '../spell-design-guide/index.html'
    }

    return $content
}

$drift = @()

foreach ($file in $Files) {
    $sourcePath = Join-Path $BuilderSource $file
    $docsPath = Join-Path $BuilderDocs $file

    if (-not (Test-Path $sourcePath)) {
        Write-Host "[ERROR] Missing source file: $sourcePath" -ForegroundColor Red
        exit 1
    }

    $expectedContent = Get-DocsContent -SourcePath $sourcePath -FileName $file

    if ($Check) {
        $actualContent = if (Test-Path $docsPath) { Get-Content -LiteralPath $docsPath -Raw } else { $null }
        if ($actualContent -ne $expectedContent) {
            $drift += $file
        }

        continue
    }

    Set-Content -LiteralPath $docsPath -Value $expectedContent -NoNewline
    Write-Host "[SYNC] $file" -ForegroundColor Green
}

if ($Check) {
    if ($drift.Count -gt 0) {
        Write-Host "[ERROR] Docs SpellForge copy is out of sync:" -ForegroundColor Red
        foreach ($file in $drift) {
            Write-Host "  $file" -ForegroundColor Red
        }

        Write-Host "Run ./sync-docs.ps1 to update Mods/docs/spell-def-builder." -ForegroundColor Gray
        exit 1
    }

    Write-Host "[OK] Docs SpellForge copy is in sync." -ForegroundColor Green
    exit 0
}

Write-Host "[COMPLETE] SpellForge docs copy updated." -ForegroundColor Cyan
