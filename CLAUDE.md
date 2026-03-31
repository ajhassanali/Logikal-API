# Logikal Middleware - Claude Code Engineering Guide

## Project Overview
.NET 6 middleware API that bridges Logikal (window/door CAD software) to external systems via REST API. Includes a React dashboard, a .NET Framework 4.7.2 bridge process, and a Windows Service host.

## Architecture
- **API** (`src/LogikalMiddleware.Api`): ASP.NET Core 6, net6.0-windows. REST endpoints, serves React dashboard from wwwroot.
- **Core** (`src/LogikalMiddleware.Core`): Shared models, services, interfaces. FileDataService parses XML files from Logikal's file system.
- **Bridge** (`src/LogikalMiddleware.Bridge`): .NET Framework 4.7.2 console app. HTTP bridge to Logikal API via reflection. Required because Logikal DLLs target .NET Framework.
- **Service** (`src/LogikalMiddleware.Service`): Windows Service wrapper around the API.
- **Dashboard** (`src/LogikalMiddleware.Dashboard`): React 19 + TypeScript + Vite. Builds into API's wwwroot folder.

## Key Data Flow
1. XML files at `D:\LOGIKAL\LOGIKAL\objekte\DIRS\*` provide project/elevation data (POSITIONINFO.XML, ANSCHR.XML, PROJECT.PRJ)
2. Bridge process on port 5100 provides live Logikal API access (thumbnails, parts lists)
3. API on port 5000 combines both data sources and serves the dashboard

## Branching & Workflow (gstack)
- **main**: Production. Protected. Requires PR + CI pass.
- **develop**: Integration branch. Default. Protected.
- **feature/issue-N-description**: One branch per issue. PR into develop.
- Each Claude Code instance works on ONE issue on its own branch.
- Human reviews PRs, merges to develop, then promotes to main.
- NEVER push directly to main or develop.

## Development Rules
- All code changes happen on feature branches, pushed to GitHub first.
- Local C:\Dev folder only receives code via `git pull` or `deploy.ps1`.
- Run CI checks mentally before pushing: does it build? does it lint?
- Keep PRs small and focused — one issue per branch.

## Build Commands
```bash
# .NET
dotnet restore LogikalMiddleware.sln
dotnet build LogikalMiddleware.sln --configuration Release

# Dashboard
cd src/LogikalMiddleware.Dashboard
npm ci
npm run build    # outputs to ../LogikalMiddleware.Api/wwwroot
npm run lint
npm run dev      # dev server on :5173, proxies /api to :5000
```

## Code Conventions
- C#: nullable enabled, implicit usings, .NET 6 minimal hosting pattern
- TypeScript: strict mode, ESLint enforced
- API routes: `/api/{resource}` (plural nouns, RESTful)
- DTOs in `Core/Models/`, services in `Core/Services/`
- File-based endpoints under `/api/files/*`, live Logikal endpoints under `/api/*`

## Testing
- Playwright for dashboard E2E tests: `src/LogikalMiddleware.Dashboard/tests/`
- API shell tests: `tests/api-tests.sh`
- Test locally by running API on port 5000 and Bridge on port 5100

## Config
- `appsettings.json`: Logikal launcher path, project center name, API port, CORS origins
- Dashboard Vite config proxies `/api` to `http://localhost:5000`

## Important Notes
- net6.0-windows target means CI must run on windows-latest runners
- Bridge uses reflection to load Logikal DLLs — cannot build without Logikal installed, but compiles fine
- FileDataService reads XML with FileShare.Read — watch for file locking issues
- Dashboard builds into wwwroot — the API serves it as static files with SPA fallback
