# Logikal API Middleware - Project Status

## ARCHITECTURE
- .NET 6 API (LogikalMiddleware.Api) on port 5000
- .NET Framework 4.7.2 Bridge (LogikalMiddleware.Bridge) for WCF/Logikal IPC on port 5100
- React 19 dashboard (LogikalMiddleware.Dashboard) served from API wwwroot
- Hybrid data: XML files for metadata, Logikal API (via Bridge) for thumbnails/parts
- GitHub-first workflow: develop on branches → PR → merge → pull to C:\Dev for testing
- CI/CD: GitHub Actions for build validation, release artifacts on tags

## CRITICAL DISCOVERY: LOGIKAL API HIERARCHY

The Logikal API has a 3-level hierarchy that differs from the file system:

```
LoginScope → ProjectCenterInfos (2 types)
  → GetProjectCenter → ProjectCenterUi
    → ProjectCenterContainer.ChildrenInfos → sub-centers (Completed1, Live Leads, etc.)
      → GetChild(subInfo) → sub-center ProjectCenterUi → ChildrenInfos → projects
```

- `DirectoryName` at the top level is EMPTY — named folders are sub-centers via `ProjectCenterContainer`
- Use `DirectoryName` (not `Name`) to match sub-center names
- Use interface-based reflection (`SafeGetProperty`/`SafeGetMethod`) to avoid `AmbiguousMatchException`

## CRITICAL DISCOVERY: GUID MAPPING

Logikal uses DIFFERENT GUIDs for the same elevation in file system vs API:
- File system (POSITIONINFO.XML): `{06C0DDEF-A0B2-46CE-BE20-E77B250CE7CF}`
- Logikal API: `690abac9-c397-4038-afe2-f9f9083b936d`

Solution: GUID mapping layer that matches by job number → position index:
1. File system project → job number → Bridge cache search → API project GUID
2. API project GUID → Bridge elevations endpoint → API elevation GUIDs
3. Position index maps file system elevation to API elevation
4. Thumbnail/parts fetched using API GUID

## WHAT'S WORKING
- File-based project search (POSITIONINFO.XML, ANSCHR.XML, PROJECT.PRJ)
- Job numbers, customer names, dimensions, system types, quantities, colors
- Search by job number, customer name, project name, offer number
- Focused on Completed1 folder (6 projects, 105 elevations) via ProjectCenterFilter config
- **Thumbnails via mapped endpoint** (file folder + position → API GUID → Logikal rendering)
- **Parts lists via mapped endpoint** (same mapping flow)
- Dashboard with search, elevation cards, dimension placeholders, download buttons
- CSV export of elevations
- File locking fixed (FileShare.ReadWrite)
- Bridge navigates sub-centers via ProjectCenterContainer

## API ENDPOINTS
- `GET /api/files/projects?search=` - Search file-based projects
- `GET /api/files/elevations?folder=` - Get elevations for a project folder
- `GET /api/files/thumbnail?folder=&position=` - Get thumbnail via GUID mapping
- `GET /api/files/partslist?folder=&position=` - Get parts list via GUID mapping
- `GET /api/health` - Connection status
- `GET /api/elevations/{guid}/thumbnail` - Direct API GUID thumbnail (requires API GUID)
- `GET /api/elevations/{guid}/partslist` - Direct API GUID parts list

## LOGIKAL CONNECTION REQUIREMENTS
- Program mode: BIM (not ERP)
- Logikal must be open and logged in
- Bridge started with center filter: `LogikalMiddleware.Bridge.exe "D:\LOGIKAL\LOGIKAL\winstart.exe" 5100 BIM "Completed1"`
- Login params: PROGRAM_MODE=BIM, APPLICATION_HANDLE, ENABLE_EVENT_SYNCHRONIZATION=false

## FILE LOCATIONS
- Projects: `D:\LOGIKAL\LOGIKAL\objekte\DIRS\{ProjectCenter}\OBJ*\`
- Elevations: `POSITIONINFO.XML` (Width, Height, System, PosNr, GUID)
- Customer: `ANSCHR.XML` (Order=job number, KfmContact, Name1)
- Project GUID: `PROJECT.PRJ` (note: differs from API GUID)
- Drawing data: `Documents/{GUID}.SDC` (encrypted ZIP archives — use API for thumbnails)

## COMPLETED1 PROJECTS (Test Data)
| Job | Name | Elevations | Contact |
|-----|------|------------|---------|
| 2020-2131 | Heritage windows | 27 | Administrator |
| 2025-3315 | Alitherm 400 + Vision | 25 | - |
| 2025-3371 | Bifolds | 6 | Harry Jivanjee |
| 2025-3387 | Arabella | 23 | Harry Jivanjee |
| 2025-3312 | Alitherm 400 + Vision | 20 | Harry Jivanjee |
| 2025-3380 | Shopline + EcoFutural | 4 | Harry Jivanjee |

## KNOWN ISSUES / TODO
- Survey PDF generation (Phase 3)
- Drawing export PDF/DXF (Phase 2)
- Stock calculation (Phase 3)
- Glass order generation (Phase 3)
- Event monitoring (Phase 4)
- Webhook integration with n8n (Phase 4)
- Thumbnail loading is slow (~30s first load per project as Bridge opens project in Logikal)
- Multiple concurrent thumbnail requests cause Bridge to search repeatedly

## BUGS FIXED
- Download buttons not triggering (DOM append fix)
- File locking (FileShare.ReadWrite)
- Job number search (added to filter)
- 6400+ project scanning (ProjectCenterFilter)
- Bridge sub-center navigation (ProjectCenterContainer)
- GUID mapping (file system ≠ API GUIDs)
- AmbiguousMatchException on reflection (SafeGetProperty/SafeGetMethod)

## TO START
1. Open Logikal on server, log in, navigate to Completed1
2. Start Bridge: `cd C:\Dev\claude-code-logikal && src\LogikalMiddleware.Bridge\bin\Debug\net472\LogikalMiddleware.Bridge.exe "D:\LOGIKAL\LOGIKAL\winstart.exe" 5100 BIM "Completed1"`
3. Start API: `dotnet run --project src/LogikalMiddleware.Api`
4. Open: http://localhost:5000 or http://192.168.1.152:5000

## KEY SOURCE FILES
- `src/LogikalMiddleware.Api/Controllers/FilesController.cs` - File-based + mapped endpoints
- `src/LogikalMiddleware.Api/Controllers/ElevationsController.cs` - Direct API endpoints
- `src/LogikalMiddleware.Core/Services/FileDataService.cs` - XML parser
- `src/LogikalMiddleware.Core/Models/LogikalSettings.cs` - Config with ProjectCenterFilter
- `src/LogikalMiddleware.Bridge/Program.cs` - WCF bridge with sub-center navigation
- `src/LogikalMiddleware.Dashboard/src/App.tsx` - React dashboard
- `CLAUDE.md` - Claude Code engineering guide (gstack workflow)
