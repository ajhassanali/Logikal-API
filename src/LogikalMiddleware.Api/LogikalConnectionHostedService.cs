using LogikalMiddleware.Core.Services;

public class LogikalConnectionHostedService : IHostedService
{
    private readonly ILogikalService _logikalService;
    private readonly ILogger<LogikalConnectionHostedService> _logger;

    public LogikalConnectionHostedService(ILogikalService logikalService, ILogger<LogikalConnectionHostedService> logger)
    {
        _logikalService = logikalService;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Logikal connection in background...");
        // Fire and forget - don't block app startup
        _ = Task.Run(async () =>
        {
            try
            {
                await _logikalService.ConnectAsync(cancellationToken);
                _logger.LogInformation("Logikal connection established");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Logikal. Use POST /api/connect to retry.");
            }
        }, cancellationToken);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Disconnecting from Logikal...");
        await _logikalService.DisconnectAsync();
    }
}
