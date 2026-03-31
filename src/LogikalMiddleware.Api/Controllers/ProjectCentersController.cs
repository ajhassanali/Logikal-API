using LogikalMiddleware.Core.Models;
using LogikalMiddleware.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogikalMiddleware.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectCentersController : ControllerBase
{
    private readonly ILogikalService _logikalService;
    private readonly ILogger<ProjectCentersController> _logger;

    public ProjectCentersController(ILogikalService logikalService, ILogger<ProjectCentersController> logger)
    {
        _logikalService = logikalService;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<List<ProjectCenterDto>> GetProjectCenters()
    {
        if (!_logikalService.IsConnected)
            return StatusCode(503, new { error = "Logikal is not connected" });

        try
        {
            var centers = _logikalService.GetProjectCenters();
            return Ok(centers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project centers");
            return StatusCode(500, new { error = "Failed to retrieve project centers", detail = ex.Message });
        }
    }
}
