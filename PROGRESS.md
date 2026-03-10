# Logikal API Middleware - Project Status

## ARCHITECTURE
- .NET 6 API (LogikalMiddleware.Api) on port 5000
- .NET Framework 4.7.2 Bridge (LogikalMiddleware.Bridge) for WCF/Logikal IPC
- React dashboard (LogikalMiddleware.Dashboard) served from same port
- Hybrid data: XML files for basic data, Logikal API for thumbnails/parts

## WHAT'S WORKING
- File-based project search (reads POSITIONINFO.XML, ANSCHR.XML, PROJECT.PRJ)
- Job numbers extracted from ANSCHR.XML Order field
- Dimensions, system types, quantities from POSITIONINFO.XML
- Customer data from ANSCHR.XML
- 12-month filter to avoid old file conversions
- Thumbnails via API (needs Logikal running)
- Parts lists via API (needs Logikal running)
- Dashboard with search, elevation cards, parts viewer
- API endpoints: /api/files/projects, /api/files/elevations, /api/health, /api/projects/search, /api/elevations/{guid}/thumbnail, /api/elevations/{guid}/partslist

## LOGIKAL CONNECTION REQUIREMENTS
- Program mode: BIM (not ERP - ERP failed)
- Logikal must be open and logged in
- Bridge uses WCF named pipes via ServiceProxyUiFactory
- Login params: PROGRAM_MODE=BIM, APPLICATION_HANDLE, ENABLE_EVENT_SYNCHRONIZATION

## FILE LOCATIONS (Logikal Data)
- Projects: D:\LOGIKAL\LOGIKAL\objekte\DIRS\{ProjectCenter}\OBJ*\
- Elevations: POSITIONINFO.XML (Width, Height, System, PosNr, GUID)
- Customer: ANSCHR.XML (Order=job number, Customer, City, etc.)
- Project GUID: PROJECT.PRJ
- Thumbnails: PREVIEWS.OZP (password-protected, use API instead)
- Parts: Documents/{GUID}.SDC (password-protected, use API instead)

## KNOWN ISSUES / TODO
- Download thumbnails button not working
- Download parts list button not working
- Survey PDF generation not built yet
- 4-digit job number search needs improvement
- File locking if same project open in Logikal and API simultaneously

## BUGS FIXED
- System.ServiceModel missing - added assembly resolver for .NET Framework GAC
- "No program mode set" - added PROGRAM_MODE=BIM to login
- Old files triggering conversion dialogs - added 12-month filter
- Parts list XML parsing - rewrote parser for actual Logikal table/row/column format

## TO START THE API
1. Open Logikal on server, log in
2. PowerShell: cd C:\Dev\claude-code-logikal
3. Run: dotnet run --project src/LogikalMiddleware.Api
4. Open: http://localhost:5000

## KEY SOURCE FILES
- src/LogikalMiddleware.Api/Program.cs - API host
- src/LogikalMiddleware.Api/Controllers/FilesController.cs - file-based endpoints
- src/LogikalMiddleware.Core/Services/FileDataService.cs - XML parser
- src/LogikalMiddleware.Bridge/ - WCF connection to Logikal
- src/LogikalMiddleware.Dashboard/src/App.tsx - React dashboard
