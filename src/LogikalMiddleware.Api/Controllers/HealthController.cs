using LogikalMiddleware.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogikalMiddleware.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogikalService _logikalService;

    public HealthController(ILogikalService logikalService)
    {
        _logikalService = logikalService;
    }

    [HttpGet]
    public ActionResult GetHealth()
    {
        return Ok(new
        {
            status = _logikalService.IsConnected ? "connected" : "disconnected",
            timestamp = DateTime.UtcNow
        });
    }

    [HttpPost("/api/connect")]
    public async Task<ActionResult> Connect(CancellationToken cancellationToken)
    {
        if (_logikalService.IsConnected)
            return Ok(new { status = "already connected" });

        await _logikalService.ConnectAsync(cancellationToken);
        return Ok(new { status = _logikalService.IsConnected ? "connected" : "failed" });
    }
}
