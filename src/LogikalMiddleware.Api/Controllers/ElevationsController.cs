using LogikalMiddleware.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogikalMiddleware.Api.Controllers;

[ApiController]
[Route("api")]
public class ElevationsController : ControllerBase
{
    private readonly ILogikalService _logikalService;
    private readonly ILogger<ElevationsController> _logger;

    public ElevationsController(ILogikalService logikalService, ILogger<ElevationsController> logger)
    {
        _logikalService = logikalService;
        _logger = logger;
    }

    [HttpGet("projects/{projectGuid}/elevations")]
    public ActionResult GetElevations(Guid projectGuid)
    {
        if (!_logikalService.IsConnected)
            return StatusCode(503, new { error = "Logikal is not connected" });

        try
        {
            var elevations = _logikalService.GetElevations(projectGuid);
            return Ok(elevations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting elevations for {ProjectGuid}", projectGuid);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("elevations/{elevationGuid}/thumbnail")]
    public ActionResult GetThumbnail(Guid elevationGuid)
    {
        if (!_logikalService.IsConnected)
            return StatusCode(503, new { error = "Logikal is not connected" });

        try
        {
            var imageBytes = _logikalService.GetThumbnail(elevationGuid);
            return File(imageBytes, "image/png");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting thumbnail for {ElevationGuid}", elevationGuid);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("elevations/{elevationGuid}/partslist")]
    public ActionResult GetPartsList(Guid elevationGuid)
    {
        if (!_logikalService.IsConnected)
            return StatusCode(503, new { error = "Logikal is not connected" });

        try
        {
            var json = _logikalService.GetPartsList(elevationGuid);
            return Content(json, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parts list for {ElevationGuid}", elevationGuid);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
