#Requires -Version 5.1
<#
.SYNOPSIS
    Pulls the latest validated code from GitHub and rebuilds locally.
.DESCRIPTION
    This script pulls the latest code from the specified branch (default: main),
    rebuilds the .NET solution, and restores the React dashboard.
.PARAMETER Branch
    The branch to pull from. Default is 'main'.
.PARAMETER SkipDashboard
    Skip rebuilding the React dashboard.
.PARAMETER SkipBackend
    Skip rebuilding the .NET backend.
.EXAMPLE
    .\deploy.ps1
    .\deploy.ps1 -Branch develop
    .\deploy.ps1 -SkipDashboard
#>
param(
    [string]$Branch = "main",
    [switch]$SkipDashboard,
    [switch]$SkipBackend
)

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Logikal Middleware - Local Deploy" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Pull latest from GitHub
Write-Host "[1/4] Pulling latest from origin/$Branch..." -ForegroundColor Yellow
Push-Location $ProjectRoot
try {
    git fetch origin
    if ($LASTEXITCODE -ne 0) { throw "git fetch failed" }

    git checkout $Branch
    if ($LASTEXITCODE -ne 0) { throw "git checkout failed" }

    git pull origin $Branch
    if ($LASTEXITCODE -ne 0) { throw "git pull failed" }

    $commitHash = git rev-parse --short HEAD
    $commitMsg = git log -1 --pretty=format:"%s"
    Write-Host "  Latest commit: $commitHash - $commitMsg" -ForegroundColor Green
}
finally {
    Pop-Location
}

# Step 2: Restore and build .NET
if (-not $SkipBackend) {
    Write-Host ""
    Write-Host "[2/4] Restoring NuGet packages..." -ForegroundColor Yellow
    dotnet restore "$ProjectRoot\LogikalMiddleware.sln"
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed" }

    Write-Host ""
    Write-Host "[3/4] Building .NET solution (Release)..." -ForegroundColor Yellow
    dotnet build "$ProjectRoot\LogikalMiddleware.sln" --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed" }
}
else {
    Write-Host ""
    Write-Host "[2/4] Skipping NuGet restore (--SkipBackend)" -ForegroundColor DarkGray
    Write-Host "[3/4] Skipping .NET build (--SkipBackend)" -ForegroundColor DarkGray
}

# Step 3: Rebuild Dashboard
if (-not $SkipDashboard) {
    Write-Host ""
    Write-Host "[4/4] Building React Dashboard..." -ForegroundColor Yellow
    Push-Location "$ProjectRoot\src\LogikalMiddleware.Dashboard"
    try {
        npm ci
        if ($LASTEXITCODE -ne 0) { throw "npm ci failed" }

        npm run build
        if ($LASTEXITCODE -ne 0) { throw "npm run build failed" }
    }
    finally {
        Pop-Location
    }
}
else {
    Write-Host ""
    Write-Host "[4/4] Skipping Dashboard build (--SkipDashboard)" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Deploy complete!" -ForegroundColor Green
Write-Host "  Branch: $Branch" -ForegroundColor Green
Write-Host "  Commit: $commitHash" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
