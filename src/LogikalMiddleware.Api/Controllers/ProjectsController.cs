using LogikalMiddleware.Core.Models;
using LogikalMiddleware.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogikalMiddleware.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ILogikalService _logikalService;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(ILogikalService logikalService, ILogger<ProjectsController> logger)
    {
        _logikalService = logikalService;
        _logger = logger;
    }

    [HttpGet("search")]
    public ActionResult<List<ProjectDto>> SearchProjects([FromQuery] string? term)
    {
        if (!_logikalService.IsConnected)
            return StatusCode(503, new { error = "Logikal is not connected" });

        try
        {
            var projects = _logikalService.SearchProjects(term ?? string.Empty);
            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching projects");
            return StatusCode(500, new { error = "Failed to search projects", detail = ex.Message });
        }
    }
}
