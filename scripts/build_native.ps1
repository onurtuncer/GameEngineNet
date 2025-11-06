<#
.SYNOPSIS
    Build the native HazelBridge DLL via CMake and copy it next to the WPF app.

.DESCRIPTION
    This script configures and builds the C++ HazelBridge project in `native/`
    using CMake, and then copies the resulting HazelBridge.dll into the
    GameHost output folder (bin\Debug\net8.0-windows or bin\Release\net8.0-windows).

.PARAMETER Config
    Build configuration: Debug or Release (default: Release).

.EXAMPLE
    ./build_native.ps1
    ./build_native.ps1 -Config Debug
#>

param(
    [ValidateSet("Debug", "Release")]
    [string]$Config = "Release"
)

# --- Paths ---
$rootDir = Split-Path -Parent $PSScriptRoot
$nativeDir = Join-Path $rootDir "native"
$buildDir  = Join-Path $rootDir "build\native"
$outDir    = Join-Path $buildDir "out\$Config\bin"
$wpfOutDir = Join-Path $rootDir "GameHost\bin\$Config\net8.0-windows"

# --- Create build folder ---
if (-not (Test-Path $buildDir)) {
    New-Item -ItemType Directory -Force -Path $buildDir | Out-Null
}

Write-Host "=== Building HazelBridge ($Config) ===" -ForegroundColor Cyan

# --- Step 1: Configure ---
Push-Location $buildDir
cmake -S $nativeDir -B . -DCMAKE_BUILD_TYPE=$Config
if ($LASTEXITCODE -ne 0) {
    Write-Host "CMake configuration failed." -ForegroundColor Red
    Pop-Location
    exit 1
}

# --- Step 2: Build ---
cmake --build . --config $Config --parallel
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed." -ForegroundColor Red
    Pop-Location
    exit 1
}

Pop-Location

# --- Step 3: Locate and copy DLL ---
$dllName = "HazelBridge.dll"
$dllPath = Join-Path $outDir $dllName

if (-not (Test-Path $dllPath)) {
    Write-Host "HazelBridge.dll not found at $dllPath" -ForegroundColor Red
    exit 1
}

# Create WPF output dir if missing
if (-not (Test-Path $wpfOutDir)) {
    New-Item -ItemType Directory -Force -Path $wpfOutDir | Out-Null
}

$destPath = Join-Path $wpfOutDir $dllName
Copy-Item $dllPath $destPath -Force
Write-Host "Copied $dllName to $destPath" -ForegroundColor Green

Write-Host "=== Native build complete ===" -ForegroundColor Cyan
