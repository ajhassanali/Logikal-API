# Logikal Middleware API Project

## Project Overview

Build a middleware layer that connects **Logikal** (fenestration CAD software by Orgadata) to external business systems. This enables automated extraction of project data, generation of survey sheets, stock calculations, and supplier ordering.

## Business Requirements

When an order is confirmed in Logikal:
1. **Generate Survey Sheets** - Pull elevation data (dimensions, thumbnails, specs) and populate survey templates
2. **Calculate Stock Requirements** - Extract parts lists, compare against inventory, identify shortfalls
3. **Automate Glass Orders** - Format glass specifications and send to suppliers
4. **Sync to ERP** - Push order status and requirements to Airtable

## Technical Context

### Logikal API Characteristics
- **Local IPC Only** - The API uses Windows interprocess communication; it must run on the same machine as Logikal
- **NuGet Package** - `Ofcas.Lk.Api.Client.Ui` version 3.0.2.8
- **Target Framework** - .NET Framework 4.5.2 (can also work with .NET 6+ with compatibility)
- **Demo Project** - Full working demo in `/docs/demo-project/`

### Connection Flow
```
1. ServiceProxyUiFactory.CreateServiceProxy(launcherPath, commandLine)
   ↓
2. ServiceProxy.Start()
   ↓
3. ServiceProxy.Login(parameters) → ILoginScopeUi
   ↓
4. LoginScope.ProjectCenters → Select project center
   ↓
5. ProjectCenter.GetProject(guid) → IProjectUi
   ↓
6. Project.Phases → Phase.Elevations → IElevationUi
```

### Key API Methods for Our Use Cases

**Elevation Data:**
- `ElevationUi.Info` → Name, Width, Height, UValue, ProcessingStatus, etc.
- `ElevationUi.GetThumbnail(params)` → Image stream (PNG/JPG)
- `ElevationUi.GetPartsList()` → SQLite database stream with all parts/profiles
- `ElevationUi.GetInformation(params)` → XML with detailed elevation data
- `ElevationUi.GetCalculationPrice()` / `GetQuotationPrice()` → Pricing

**Drawing Exports:**
- `ElevationUi.GetDrawing(params)` → DXF or PDF
- Parameters: Format, View (Interior/Exterior), Scale, ShowDimensions

**Project Data:**
- `ProjectUi.Info` → JobNumber, OfferNumber, CustomerName, SiteAddress
- `ProjectUi.AddressContainer` → Customer and site addresses

## Project Structure

```
logikal-middleware/
├── docs/
│   ├── demo-project/           # Extracted Ofcas demo (reference only)
│   └── api-notes.md            # Key API patterns extracted from demo
├── src/
│   ├── LogikalMiddleware.Core/         # Core business logic
│   │   ├── Services/
│   │   │   ├── ILogikalService.cs
│   │   │   └── LogikalService.cs
│   │   ├── Models/
│   │   │   ├── ProjectDto.cs
│   │   │   ├── ElevationDto.cs
│   │   │   └── PartsListDto.cs
│   │   └── Mappers/
│   ├── LogikalMiddleware.Service/       # Windows Service host
│   │   ├── Program.cs
│   │   ├── Worker.cs
│   │   └── appsettings.json
│   └── LogikalMiddleware.Api/           # REST API layer
│       ├── Controllers/
│       │   ├── ProjectsController.cs
│       │   ├── ElevationsController.cs
│       │   └── ExportsController.cs
│       └── Program.cs
├── tests/
│   └── LogikalMiddleware.Tests/
└── logikal-middleware.sln
```

## Implementation Phases

### Phase 1: Basic Connection & Read
1. Create Windows Service that connects to Logikal on startup
2. Expose REST endpoint: `GET /api/projectcenters` - list available project centers
3. Expose: `GET /api/projects?search={term}` - search projects
4. Expose: `GET /api/projects/{guid}/elevations` - list elevations with thumbnails
5. Test: Connect, authenticate, list projects, get elevation data

### Phase 2: Data Extraction
1. `GET /api/elevations/{guid}/thumbnail` → PNG image
2. `GET /api/elevations/{guid}/partslist` → JSON (parsed from SQLite export)
3. `GET /api/elevations/{guid}/drawing?format=pdf` → PDF download
4. `GET /api/elevations/{guid}/info` → Full elevation details JSON

### Phase 3: Automation Workflows
1. `POST /api/projects/{guid}/generate-survey` → Trigger survey sheet PDF generation
2. `POST /api/projects/{guid}/calculate-stock` → Return stock requirements
3. `POST /api/projects/{guid}/glass-order` → Generate glass order document

### Phase 4: Event Monitoring
1. Subscribe to Logikal synchronization events (status changes)
2. When project status changes to "Confirmed", trigger workflow automatically
3. Push webhooks to n8n for further processing

## Configuration

**appsettings.json:**
```json
{
  "Logikal": {
    "LauncherPath": "C:\\Program Files\\Orgadata\\Logikal\\Launcher.exe",
    "ProjectCenterName": "DTW Production"
  },
  "Api": {
    "Port": 5000,
    "AllowedOrigins": ["http://localhost:3000"]
  },
  "Webhooks": {
    "N8nUrl": "http://n8n.directtradewindows.co.uk:5678/webhook/logikal"
  }
}
```

## Development Notes

### From the Demo Project

**Creating Service Proxy:**
```csharp
var serviceProxy = ServiceProxyUiFactory.CreateServiceProxy(
    environmentModel.LauncherPath, 
    Environment.CommandLine
).ServiceProxyUi;

serviceProxy.Start();
```

**Login:**
```csharp
var loginParams = new Dictionary<string, object>();
// Add credentials if required
var loginResult = serviceProxy.Login(loginParams);
var loginScope = loginResult.CoreObject;
```

**Getting Parts List:**
```csharp
if (elevationUi.CanGetPartsList().Value)
{
    using (var partsListStream = elevationUi.GetPartsList().Stream)
    {
        // Stream is SQLite database - copy to file and query
    }
}
```

**Getting Thumbnail:**
```csharp
var thumbnailParams = new Dictionary<string, object>
{
    { WellKnownParameterKey.Elevation.Thumbnail.Width, 800 },
    { WellKnownParameterKey.Elevation.Thumbnail.Height, 600 },
    { WellKnownParameterKey.Elevation.Thumbnail.Format, ThumbnailFormat.Png }
};

using (var stream = elevationUi.GetThumbnail(thumbnailParams).Stream)
{
    // PNG image stream
}
```

### Elevation Model Fields
```
- Guid, VersionGuid, CoreObjectId
- Name, UserDescription, ModelDescription, AutomaticDescription
- Width, Height (dimensions in mm)
- UValue (thermal performance)
- ProcessingStatus (Offer, Order, Production, etc.)
- ElementType
- SystemDescription (e.g., "SMA ALITHERM 400")
- SurfaceFrameColors (e.g., "RAL 7021")
- Thumbnail (image)
```

## Testing

**Test Project Structure:**
1. Create a test project in Logikal with known elevations
2. Note the Project GUID and Elevation GUIDs
3. Use these in integration tests

**Manual Testing:**
```bash
# After service is running
curl http://localhost:5000/api/projectcenters
curl http://localhost:5000/api/projects/search?term=Perkins
curl http://localhost:5000/api/elevations/{guid}/thumbnail --output thumb.png
```

## Dependencies

**NuGet Packages:**
- `Ofcas.Lk.Api.Client.Ui` (3.0.2.8) - from Logikal installation
- `Microsoft.Data.Sqlite` - for parsing parts list exports
- `Microsoft.Extensions.Hosting.WindowsServices` - Windows Service support
- `System.Drawing.Common` - image handling

## Next Steps

After middleware is working:
1. Connect to n8n workflows for automated processing
2. Build React dashboard for project overview
3. Build React Native mobile survey app
4. Integrate with Airtable ERP
