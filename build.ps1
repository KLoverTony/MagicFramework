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

.PARAMETER VerifyTextures
If specified, verifies XML texture references resolve to mod or vanilla textures.

.PARAMETER Full
If specified, runs the build with additional validation checks.

.EXAMPLE
./build.ps1 -Deploy
# Builds all mods and copies to RimWorld Mods folder

./build.ps1 -Clean
# Cleans and rebuilds all mods

./build.ps1 -Full
# Builds all mods and verifies texture dependencies
#>
param(
    [switch]$Deploy,
    [switch]$Clean,
    [switch]$VerifyTextures,
    [switch]$Full
)

$ErrorActionPreference = 'Stop'

$ModsDir = Split-Path -Parent $PSCommandPath
$RimWorldModsPath = 'D:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods'
$RimWorldCoreTexturesPath = 'D:\Program Files (x86)\Steam\steamapps\common\RimWorld\Data\Core\Textures'
$KnownVanillaTexturePaths = @(
    'Things/Projectile/Spark',
    'Things/Building/Linked/Wall_Blueprint_Atlas'
)

if ($Full) {
    $VerifyTextures = $true
}

# Build order (dependencies first)
$Projects = @(
    'MagicFramework'
    'MFVanilla'
    'AeternusFaith'
)

Write-Host "[BUILD] RimWorld Mods Build Script" -ForegroundColor Cyan
Write-Host "Mods dir: $ModsDir" -ForegroundColor Gray
Write-Host ""

function Get-XmlLineNumber {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Lines,

        [Parameter(Mandatory = $true)]
        [string]$ElementName,

        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    $escapedValue = [regex]::Escape($Value)
    $pattern = "<$ElementName>\s*$escapedValue\s*</$ElementName>"

    for ($i = 0; $i -lt $Lines.Count; $i++) {
        if ($Lines[$i] -match $pattern) {
            return $i + 1
        }
    }

    return 1
}

function Test-TexturePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$TexturePath,

        [Parameter(Mandatory = $true)]
        [string]$GraphicClass,

        [Parameter(Mandatory = $true)]
        [string[]]$TextureRoots
    )

    $relativePath = $TexturePath.Trim().TrimStart('/', '\')
    if ([string]::IsNullOrWhiteSpace($relativePath)) {
        return $true
    }

    $relativePath = $relativePath -replace '/', [System.IO.Path]::DirectorySeparatorChar
    $candidates = @("$relativePath.png", "$relativePath.jpg", "$relativePath.jpeg")

    if ($GraphicClass -eq 'Graphic_Multi') {
        $directionCandidates = @()
        foreach ($suffix in @('_north', '_south', '_east', '_west')) {
            $directionCandidates += "$relativePath$suffix.png"
        }

        foreach ($root in $TextureRoots) {
            $foundRequired = 0
            foreach ($candidate in $directionCandidates | Where-Object { $_ -notlike '*_west.png' }) {
                if (Test-Path (Join-Path $root $candidate)) {
                    $foundRequired++
                }
            }

            if ($foundRequired -eq 3) {
                return $true
            }
        }
    }

    foreach ($root in $TextureRoots) {
        foreach ($candidate in $candidates) {
            if (Test-Path (Join-Path $root $candidate)) {
                return $true
            }
        }

        if ($GraphicClass -eq 'Graphic_Random') {
            $collectionDir = Join-Path $root $relativePath
            if ((Test-Path $collectionDir) -and (Get-ChildItem -Path $collectionDir -File -Include '*.png', '*.jpg', '*.jpeg' -ErrorAction SilentlyContinue | Select-Object -First 1)) {
                return $true
            }
        }
    }

    return $false
}

function Test-ModTextureDependencies {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Projects,

        [Parameter(Mandatory = $true)]
        [string]$ModsDir,

        [Parameter(Mandatory = $true)]
        [string]$CoreTexturesPath,

        [Parameter(Mandatory = $true)]
        [string[]]$KnownVanillaTexturePaths
    )

    Write-Host "[VERIFY] Checking texture dependencies..." -ForegroundColor Cyan

    $textureRoots = @()
    foreach ($proj in $Projects) {
        $textureRoot = Join-Path (Join-Path $ModsDir $proj) 'Textures'
        if (Test-Path $textureRoot) {
            $textureRoots += $textureRoot
        }
    }

    if (Test-Path $CoreTexturesPath) {
        $textureRoots += $CoreTexturesPath
    } else {
        Write-Host "  [INFO] RimWorld Core loose texture folder not found; using known vanilla texture allowlist." -ForegroundColor Gray
    }

    $missing = @()
    $knownVanillaLookup = @{}
    foreach ($path in $KnownVanillaTexturePaths) {
        $knownVanillaLookup[$path.ToLowerInvariant()] = $true
    }

    foreach ($proj in $Projects) {
        $defsPath = Join-Path (Join-Path $ModsDir $proj) 'Defs'
        if (-not (Test-Path $defsPath)) {
            continue
        }

        $xmlFiles = Get-ChildItem -Path $defsPath -Filter '*.xml' -Recurse -File
        foreach ($xmlFile in $xmlFiles) {
            try {
                [xml]$xml = Get-Content -Path $xmlFile.FullName -Raw
            } catch {
                Write-Host "  [WARN] Could not parse $($xmlFile.FullName): $($_.Exception.Message)" -ForegroundColor Yellow
                continue
            }

            $lines = Get-Content -Path $xmlFile.FullName
            $graphicNodes = $xml.SelectNodes('//graphicData[texPath]')
            foreach ($graphicNode in $graphicNodes) {
                $texturePath = $graphicNode.texPath.InnerText
                if ([string]::IsNullOrWhiteSpace($texturePath)) {
                    continue
                }

                if ($knownVanillaLookup.ContainsKey($texturePath.Trim().ToLowerInvariant())) {
                    continue
                }

                $graphicClass = if ($graphicNode.graphicClass) { $graphicNode.graphicClass.InnerText } else { 'Graphic_Single' }

                if (-not (Test-TexturePath -TexturePath $texturePath -GraphicClass $graphicClass -TextureRoots $textureRoots)) {
                    $missing += [pscustomobject]@{
                        File = $xmlFile.FullName
                        Line = Get-XmlLineNumber -Lines $lines -ElementName 'texPath' -Value $texturePath
                        Path = $texturePath
                        Type = $graphicClass
                    }
                }
            }

            foreach ($elementName in @('gizmoIconPath', 'sustainedOverlayTexturePath')) {
                $nodes = $xml.SelectNodes("//$elementName")
                foreach ($node in $nodes) {
                    $texturePath = $node.InnerText
                    if ([string]::IsNullOrWhiteSpace($texturePath)) {
                        continue
                    }

                    if ($knownVanillaLookup.ContainsKey($texturePath.Trim().ToLowerInvariant())) {
                        continue
                    }

                    if (-not (Test-TexturePath -TexturePath $texturePath -GraphicClass 'Graphic_Single' -TextureRoots $textureRoots)) {
                        $missing += [pscustomobject]@{
                            File = $xmlFile.FullName
                            Line = Get-XmlLineNumber -Lines $lines -ElementName $elementName -Value $texturePath
                            Path = $texturePath
                            Type = $elementName
                        }
                    }
                }
            }
        }
    }

    if ($missing.Count -eq 0) {
        Write-Host "  [OK] Texture dependencies resolved" -ForegroundColor Green
        return $true
    }

    Write-Host "  [ERROR] Missing texture dependencies:" -ForegroundColor Red
    foreach ($item in $missing) {
        $relativeFile = Resolve-Path -Path $item.File -Relative
        Write-Host "    ${relativeFile}:$($item.Line) $($item.Type) -> $($item.Path)" -ForegroundColor Red
    }

    return $false
}

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

if ($VerifyTextures) {
    Write-Host ""
    if (-not (Test-ModTextureDependencies -Projects $Projects -ModsDir $ModsDir -CoreTexturesPath $RimWorldCoreTexturesPath -KnownVanillaTexturePaths $KnownVanillaTexturePaths)) {
        exit 1
    }
}

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
        $srcModPath = "$ModsDir\$proj"
        $dstModPath = "$RimWorldModsPath\$proj"
        $payloadDirs = @('About', 'Assemblies', 'Defs', 'Textures')

        if (-not (Test-Path $srcModPath)) {
            Write-Host "  [SKIP] $proj (mod folder not found)" -ForegroundColor Yellow
            continue
        }

        New-Item -ItemType Directory -Path $dstModPath -Force -ErrorAction SilentlyContinue | Out-Null

        foreach ($payloadDir in $payloadDirs) {
            $srcPath = Join-Path $srcModPath $payloadDir
            $dstPath = Join-Path $dstModPath $payloadDir

            if (-not (Test-Path $srcPath)) {
                Write-Host "  [SKIP] $proj/$payloadDir (not found)" -ForegroundColor Yellow
                continue
            }

            if (Test-Path $dstPath) {
                Remove-Item $dstPath -Recurse -Force
            }

            Copy-Item $srcPath $dstModPath -Recurse -Force
        }

        Write-Host "  [OK] Deployed $proj" -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "[SUCCESS] Ready to test in RimWorld!" -ForegroundColor Green
}

Write-Host ""
Write-Host "Build complete. Use -Deploy to copy to RimWorld Mods folder, or -Full to include validation checks." -ForegroundColor Gray
