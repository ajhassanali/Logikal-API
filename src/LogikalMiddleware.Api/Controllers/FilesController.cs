using LogikalMiddleware.Core.Services;
using LogikalMiddleware.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LogikalMiddleware.Api.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IFileDataService _fileDataService;
    private readonly ILogikalService _logikalService;
    private readonly LogikalSettings _settings;
    private readonly ILogger<FilesController> _logger;
    private static readonly HttpClient _bridgeClient = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };

    public FilesController(IFileDataService fileDataService, ILogikalService logikalService,
        IOptions<LogikalSettings> settings, ILogger<FilesController> logger)
    {
        _fileDataService = fileDataService;
        _logikalService = logikalService;
        _settings = settings.Value;
        _logger = logger;
    }

    [HttpGet("projects")]
    public ActionResult GetProjects([FromQuery] string? search)
    {
        try
        {
            var projects = _fileDataService.GetAllProjects();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                projects = projects.Where(p =>
                    p.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (p.CustomerName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.ProjectCenter?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.JobNumber?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.OfferNumber?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    p.FolderPath.Contains(term, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file-based projects");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("elevations")]
    public ActionResult GetElevations([FromQuery] string folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
            return BadRequest(new { error = "folder parameter is required" });

        // Path traversal protection
        if (folder.Contains(".."))
            return BadRequest(new { error = "Invalid folder path" });

        try
        {
            var elevations = _fileDataService.GetElevations(folder);
            if (elevations.Count == 0 && !System.IO.Directory.Exists(
                Path.Combine(GetDirsPath(), folder)))
            {
                return NotFound(new { error = "Folder not found" });
            }

            return Ok(elevations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting elevations for folder {Folder}", folder);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get thumbnail for an elevation by file-system folder path and position index.
    /// Maps between file-system and Logikal API GUIDs via job number matching.
    /// </summary>
    [HttpGet("thumbnail")]
    public async Task<ActionResult> GetThumbnail([FromQuery] string folder, [FromQuery] int position)
    {
        if (!_logikalService.IsConnected)
            return StatusCode(503, new { error = "Logikal not connected" });

        if (string.IsNullOrWhiteSpace(folder) || folder.Contains(".."))
            return BadRequest(new { error = "Invalid folder" });

        try
        {
            var bridgePort = _settings.BridgePort > 0 ? _settings.BridgePort : 5100;
            var bridgeBase = $"http://localhost:{bridgePort}";

            // 1. Get the job number from the file-system project
            var projects = _fileDataService.GetAllProjects();
            var fileProject = projects.FirstOrDefault(p =>
                p.FolderPath.Equals(folder, StringComparison.OrdinalIgnoreCase));
            if (fileProject == null)
                return NotFound(new { error = "Project not found in file system" });

            // 2. Search Bridge cache for matching API project by job number
            // Use the last 4 digits of the job number for reliable matching
            var searchTerm = fileProject.JobNumber;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var digits = new string(searchTerm.Where(char.IsDigit).ToArray());
                if (digits.Length >= 4) searchTerm = digits.Substring(digits.Length - 4);
            }
            if (string.IsNullOrWhiteSpace(searchTerm)) searchTerm = fileProject.Name;

            var searchUrl = $"{bridgeBase}/projects/search?term={Uri.EscapeDataString(searchTerm)}";
            var searchResp = await _bridgeClient.GetStringAsync(searchUrl);
            var apiProjects = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(searchResp);

            string apiProjectGuid = null;
            foreach (var p in apiProjects.EnumerateArray())
            {
                var apiJobNumber = p.TryGetProperty("jobNumber", out var jn) ? jn.GetString() : null;
                if (apiJobNumber != null && fileProject.JobNumber != null &&
                    new string(apiJobNumber.Where(char.IsDigit).ToArray()) ==
                    new string(fileProject.JobNumber.Where(char.IsDigit).ToArray()))
                {
                    apiProjectGuid = p.GetProperty("guid").GetString();
                    break;
                }
            }

            if (apiProjectGuid == null)
                return NotFound(new { error = "Project not found in Logikal API" });

            // 3. Get elevations from Bridge for this project
            var elevUrl = $"{bridgeBase}/projects/{apiProjectGuid}/elevations";
            var elevResp = await _bridgeClient.GetStringAsync(elevUrl);
            var apiElevations = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(elevResp);

            // 4. Get the elevation at the requested position index (0-based)
            int idx = 0;
            string apiElevGuid = null;
            foreach (var e in apiElevations.EnumerateArray())
            {
                if (idx == position)
                {
                    apiElevGuid = e.GetProperty("guid").GetString();
                    break;
                }
                idx++;
            }

            if (apiElevGuid == null)
                return NotFound(new { error = $"Elevation at position {position} not found" });

            // 5. Get the thumbnail using the API GUID
            _logger.LogInformation("Mapped file position {Position} to API GUID {Guid}", position, apiElevGuid);
            var thumbUrl = $"{bridgeBase}/elevations/{apiElevGuid}/thumbnail";
            var thumbResp = await _bridgeClient.GetAsync(thumbUrl);

            if (!thumbResp.IsSuccessStatusCode)
                return StatusCode((int)thumbResp.StatusCode, new { error = "Thumbnail fetch failed" });

            var imageBytes = await thumbResp.Content.ReadAsByteArrayAsync();
            return File(imageBytes, "image/png");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mapped thumbnail for {Folder} position {Position}", folder, position);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get parts list for an elevation by file-system folder path and position index.
    /// </summary>
    [HttpGet("partslist")]
    public async Task<ActionResult> GetPartsList([FromQuery] string folder, [FromQuery] int position)
    {
        if (!_logikalService.IsConnected)
            return StatusCode(503, new { error = "Logikal not connected" });

        if (string.IsNullOrWhiteSpace(folder) || folder.Contains(".."))
            return BadRequest(new { error = "Invalid folder" });

        try
        {
            var bridgePort = _settings.BridgePort > 0 ? _settings.BridgePort : 5100;
            var bridgeBase = $"http://localhost:{bridgePort}";

            var projects = _fileDataService.GetAllProjects();
            var fileProject = projects.FirstOrDefault(p =>
                p.FolderPath.Equals(folder, StringComparison.OrdinalIgnoreCase));
            if (fileProject == null)
                return NotFound(new { error = "Project not found" });

            // Use last 4 digits of job number for reliable Bridge cache matching
            var searchTerm = fileProject.JobNumber;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var digits = new string(searchTerm.Where(char.IsDigit).ToArray());
                if (digits.Length >= 4) searchTerm = digits.Substring(digits.Length - 4);
            }
            if (string.IsNullOrWhiteSpace(searchTerm)) searchTerm = fileProject.Name;

            var searchUrl = $"{bridgeBase}/projects/search?term={Uri.EscapeDataString(searchTerm)}";
            var searchResp = await _bridgeClient.GetStringAsync(searchUrl);
            var apiProjects = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(searchResp);

            string apiProjectGuid = null;
            foreach (var p in apiProjects.EnumerateArray())
            {
                var apiJobNumber = p.TryGetProperty("jobNumber", out var jn) ? jn.GetString() : null;
                if (apiJobNumber != null && fileProject.JobNumber != null &&
                    new string(apiJobNumber.Where(char.IsDigit).ToArray()) ==
                    new string(fileProject.JobNumber.Where(char.IsDigit).ToArray()))
                {
                    apiProjectGuid = p.GetProperty("guid").GetString();
                    break;
                }
            }

            if (apiProjectGuid == null)
                return NotFound(new { error = "Project not found in Logikal API" });

            var elevUrl = $"{bridgeBase}/projects/{apiProjectGuid}/elevations";
            var elevResp = await _bridgeClient.GetStringAsync(elevUrl);
            var apiElevations = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(elevResp);

            int idx = 0;
            string apiElevGuid = null;
            foreach (var e in apiElevations.EnumerateArray())
            {
                if (idx == position)
                {
                    apiElevGuid = e.GetProperty("guid").GetString();
                    break;
                }
                idx++;
            }

            if (apiElevGuid == null)
                return NotFound(new { error = $"Elevation at position {position} not found" });

            var partsUrl = $"{bridgeBase}/elevations/{apiElevGuid}/partslist";
            var partsResp = await _bridgeClient.GetStringAsync(partsUrl);
            return Content(partsResp, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mapped parts list for {Folder} position {Position}", folder, position);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private string GetDirsPath()
    {
        // This mirrors the logic in FileDataService for folder existence checks
        var config = HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptions<LogikalMiddleware.Core.Models.LogikalSettings>>();
        var launcherDir = Path.GetDirectoryName(config.Value.LauncherPath) ?? @"D:\LOGIKAL\LOGIKAL";
        return Path.Combine(launcherDir, "objekte", "DIRS");
    }
}
