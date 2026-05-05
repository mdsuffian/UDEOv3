#!/usr/bin/env pwsh
# build.ps1 — UDEO C# Build Script (Windows PowerShell)
param([string]$Configuration = "Release")

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path $PSCommandPath -Parent

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  UDEO v3.1.0 — C# Build Pipeline" -ForegroundColor Cyan
Write-Host "  Configuration: $Configuration" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

# Step 1: Build C++ native library using MSBuild
$NativeProj = Join-Path $ScriptDir "src/UDEO.Native/UDEO.Native.csproj"
if (Test-Path $NativeProj) {
    Write-Host "`n[1/3] Building C++ native support..." -ForegroundColor Yellow
    # Use vcvarsall if available
    $vsPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath 2>$null
    if ($vsPath) {
        $vcvars = Join-Path $vsPath "VC\Auxiliary\Build\vcvars64.bat"
        if (Test-Path $vcvars) {
            Write-Host "  Found Visual Studio at: $vsPath" -ForegroundColor DarkGray
        }
    }
}

# Step 2: Restore NuGet
Write-Host "`n[2/3] Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore (Join-Path $ScriptDir "UDEO.sln") --verbosity quiet

# Step 3: Build
Write-Host "`n[3/3] Building .NET solution..." -ForegroundColor Yellow
dotnet build (Join-Path $ScriptDir "UDEO.sln") --configuration $Configuration --no-restore

Write-Host "`n============================================" -ForegroundColor Green
Write-Host "  Build complete!" -ForegroundColor Green
Write-Host "  Output: src/UDEO.Cli/bin/$Configuration/net8.0/" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
