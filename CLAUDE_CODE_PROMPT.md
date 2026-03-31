# Instructions for Claude Code

## First Steps

1. Read `README.md` for project overview
2. Read `docs/api-notes.md` for API quick reference
3. Examine `docs/demo-project/` for working examples

## Build Order

### Step 1: Understand the Demo
```
Look at these files in docs/demo-project/:
- App.xaml.cs (startup)
- ViewModels/LoginViewModel.cs (connection logic)
- ViewModels/ElevationViewModel.cs (data extraction)
- Models/ElevationModel.cs (data structure)
- Utils/ServiceProxyAdapter.cs (connection wrapper)
```

### Step 2: Create Solution Structure
Create a .NET 6 solution with three projects:
- LogikalMiddleware.Core (class library)
- LogikalMiddleware.Service (Windows Service)
- LogikalMiddleware.Api (ASP.NET Core minimal API)

### Step 3: Implement Core Service
In LogikalMiddleware.Core:
1. Create `ILogikalService` interface
2. Create `LogikalService` implementation
3. Handle connection, authentication, data extraction
4. Reference the Ofcas.Lk.Api.Client.Ui NuGet

### Step 4: Add REST API
In LogikalMiddleware.Api:
1. Add controllers/endpoints for:
   - GET /api/projectcenters
   - GET /api/projects/search
   - GET /api/projects/{guid}/elevations
   - GET /api/elevations/{guid}/thumbnail
   - GET /api/elevations/{guid}/partslist

### Step 5: Test
1. Build and run on the Logikal server machine
2. Test each endpoint manually
3. Verify thumbnail and parts list exports

## Key Patterns from Demo

### Connection
```csharp
var serviceProxy = ServiceProxyUiFactory.CreateServiceProxy(
    launcherPath, 
    Environment.CommandLine
).ServiceProxyUi;
serviceProxy.Start();
var loginScope = serviceProxy.Login(new Dictionary<string, object>()).CoreObject;
```

### Getting Data
```csharp
// Always check Can before Do
if (elevationUi.CanGetThumbnail(params).Value)
{
    using var stream = elevationUi.GetThumbnail(params).Stream;
    // Process stream
}
```

### Error Handling
```csharp
var result = someOperation();
if (result.OperationCode == OperationCode.Success)
{
    // Use result
}
```

## Important Notes

- API only works on the machine running Logikal (local IPC)
- Target .NET Framework 4.5.2 for direct compatibility, or .NET 6+ with compatibility shims
- Parts list exports as SQLite database - use Microsoft.Data.Sqlite to parse
- Thumbnail exports as PNG/JPG stream
