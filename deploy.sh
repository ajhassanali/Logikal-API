#!/bin/bash
# Logikal Middleware - Local Deploy Script (Git Bash)
# Usage: ./deploy.sh [branch]
#   branch: Branch to pull from (default: main)

set -e

BRANCH="${1:-main}"
PROJECT_ROOT="$(cd "$(dirname "$0")" && pwd)"

echo "========================================"
echo "  Logikal Middleware - Local Deploy"
echo "========================================"
echo ""

# Step 1: Pull latest
echo "[1/4] Pulling latest from origin/$BRANCH..."
cd "$PROJECT_ROOT"
git fetch origin
git checkout "$BRANCH"
git pull origin "$BRANCH"

COMMIT=$(git rev-parse --short HEAD)
MSG=$(git log -1 --pretty=format:"%s")
echo "  Latest commit: $COMMIT - $MSG"

# Step 2: Restore NuGet
echo ""
echo "[2/4] Restoring NuGet packages..."
dotnet restore "$PROJECT_ROOT/LogikalMiddleware.sln"

# Step 3: Build .NET
echo ""
echo "[3/4] Building .NET solution (Release)..."
dotnet build "$PROJECT_ROOT/LogikalMiddleware.sln" --configuration Release --no-restore

# Step 4: Build Dashboard
echo ""
echo "[4/4] Building React Dashboard..."
cd "$PROJECT_ROOT/src/LogikalMiddleware.Dashboard"
npm ci
npm run build

echo ""
echo "========================================"
echo "  Deploy complete!"
echo "  Branch: $BRANCH"
echo "  Commit: $COMMIT"
echo "========================================"
