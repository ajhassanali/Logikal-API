using LogikalMiddleware.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogikalMiddleware.Api.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IFileDataService _fileDataService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IFileDataService fileDataService, ILogger<FilesController> logger)
    {
        _fileDataService = fileDataService;
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

    private string GetDirsPath()
    {
        // This mirrors the logic in FileDataService for folder existence checks
        var config = HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptions<LogikalMiddleware.Core.Models.LogikalSettings>>();
        var launcherDir = Path.GetDirectoryName(config.Value.LauncherPath) ?? @"D:\LOGIKAL\LOGIKAL";
        return Path.Combine(launcherDir, "objekte", "DIRS");
    }
}
