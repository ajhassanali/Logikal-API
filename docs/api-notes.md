# Logikal API Quick Reference

## Core Interfaces

### IServiceProxyUi
Entry point - creates connection to Logikal.
```csharp
var proxy = ServiceProxyUiFactory.CreateServiceProxy(launcherPath, commandLine).ServiceProxyUi;
proxy.Start();
var loginScope = proxy.Login(new Dictionary<string, object>()).CoreObject;
```

### ILoginScopeUi
Authenticated session - access project centers, search, etc.
```csharp
IEnumerable<IProjectCenterInfo> centers = loginScope.ProjectCenters;
ISearchAgent searchAgent = loginScope.SearchAgent;
IProject project = loginScope.GetProjectByGuid(guid);
IProject project = loginScope.GetProjectFromElevation(elevationGuid);
```

### IProjectCenterUi
Database/folder containing projects.
```csharp
var projectCenter = loginScope.GetProjectCenter(centerInfo);
var recentProjects = projectCenter.GetRecentProjects();
var project = projectCenter.GetProject(projectInfo);
```

### IProjectUi
A quote/order with phases and elevations.
```csharp
// Project Info
projectUi.Info.JobNumber
projectUi.Info.OfferNumber
projectUi.Info.Description

// Hierarchy
var phases = projectUi.ChildrenInfos;  // IPhaseInfo[]
var phase = projectUi.GetChild(phaseInfo);  // IPhaseUi

// Addresses
var addressContainer = projectUi.AddressContainer;
```

### IPhaseUi
A phase within a project (e.g., "Ground Floor").
```csharp
var elevations = phaseUi.ChildrenInfos;  // IElevationInfo[]
var elevation = phaseUi.GetChild(elevationInfo);  // IElevationUi
```

### IElevationUi
A single window/door unit - the main data source.
```csharp
// Basic Info
elevationUi.Info.Name           // "001"
elevationUi.Info.Width          // 1030 (mm)
elevationUi.Info.Height         // 1590 (mm)
elevationUi.Info.UValue         // "1.4"
elevationUi.Info.ProcessingStatus

// Extended Info (from ElevationModel mapping)
info.UserDescription            // Room name
info.SystemDescription          // "SMA ALITHERM 400"
info.SurfaceFrameColors         // "RAL 7021"

// Exports
elevationUi.GetThumbnail(params)   // → Stream (PNG/JPG)
elevationUi.GetPartsList()         // → Stream (SQLite DB)
elevationUi.GetDrawing(params)     // → Stream (DXF/PDF)
elevationUi.GetInformation(params) // → Stream (XML)
elevationUi.GetGdz(params)         // → Stream (3D model ZIP)

// Pricing
elevationUi.GetCalculationPrice()  // Internal cost
elevationUi.GetQuotationPrice()    // Customer price
```

---

## Export Parameters

### Thumbnail
```csharp
var params = new Dictionary<string, object>
{
    { WellKnownParameterKey.Elevation.Thumbnail.Width, 800 },
    { WellKnownParameterKey.Elevation.Thumbnail.Height, 600 },
    { WellKnownParameterKey.Elevation.Thumbnail.Format, ThumbnailFormat.Png },
    { WellKnownParameterKey.Elevation.Thumbnail.View, View.Interior }
};
```

### Drawing (DXF/PDF)
```csharp
var params = new Dictionary<string, object>
{
    { WellKnownParameterKey.Elevation.Drawing.Format, ElevationDrawingFormat.PDF },
    { WellKnownParameterKey.Elevation.Drawing.View, View.Interior },
    { WellKnownParameterKey.Elevation.Drawing.Type, ElevationDrawingType.Elevation },
    { WellKnownParameterKey.Elevation.Drawing.ShowDimensions, true },
    { WellKnownParameterKey.Elevation.Drawing.ShowDescription, true },
    { WellKnownParameterKey.Elevation.Drawing.Scale, 1.0 }
};
```

### Information (XML)
```csharp
var params = new Dictionary<string, object>
{
    { WellKnownParameterKey.Elevation.Information.Format, "XML" },
    { WellKnownParameterKey.Elevation.Information.UseMetric, true },
    { WellKnownParameterKey.Elevation.Information.LevelOfDetail, LevelOfDetail.Overview }
};
```

---

## Operation Pattern

All API calls follow a Can/Do pattern:
```csharp
// Always check CanX before calling X
var operationInfo = elevationUi.CanGetPartsList();
if (operationInfo.Value)  // or use CheckForAnyRestriction()
{
    var result = elevationUi.GetPartsList();
    using (var stream = result.Stream)
    {
        // Process stream
    }
}
```

---

## Enums

### View
```csharp
View.Interior   // Inside view
View.Exterior   // Outside view
```

### ElevationDrawingFormat
```csharp
ElevationDrawingFormat.DXF
ElevationDrawingFormat.PDF
```

### ElevationDrawingType
```csharp
ElevationDrawingType.Elevation    // Front view
ElevationDrawingType.Section      // Cross-section
ElevationDrawingType.Explosion    // Exploded view
ElevationDrawingType.SectionLine  // Section with line
```

### ThumbnailFormat
```csharp
ThumbnailFormat.Png
ThumbnailFormat.Jpg
ThumbnailFormat.Bmp
```

### LevelOfDetail
```csharp
LevelOfDetail.Overview
LevelOfDetail.Detailed
```

---

## Parts List SQLite Schema

The `GetPartsList()` returns a SQLite database. Query it for:
- Profile codes and cut lengths
- Glass specifications
- Hardware items
- Quantities

Example query (you'll need to explore the schema):
```sql
SELECT * FROM sqlite_master WHERE type='table';
-- Then query specific tables for parts data
```

---

## Synchronization (Events)

Monitor for status changes:
```csharp
loginScope.SynchronizationContainer.SynchronizedEventReceived += (sender, e) =>
{
    var syncEvent = e.SynchronizedEvent;
    // Check event type, object affected
    loginScope.SynchronizationContainer.SetHandled(syncEvent);
};
```

---

## Error Handling

Check OperationCode on results:
```csharp
var result = elevationUi.GetPartsList();
if (result.OperationCode == OperationCode.Success)
{
    // Use result.Stream
}
else if (result.OperationCode == OperationCode.Rejected)
{
    // Operation was cancelled/rejected
}
```

---

## File Locations (Typical)

- **Logikal Launcher:** `C:\Program Files\Orgadata\Logikal\Launcher.exe`
- **API Assemblies:** `C:\Program Files\Orgadata\Logikal\Api\` or in GAC
- **Project Data:** Network path `\\DTWINTERNAL\logikal\LOGIKAL\`
