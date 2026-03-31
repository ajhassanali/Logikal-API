using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using LogikalMiddleware.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogikalMiddleware.Core.Services;

/// <summary>
/// Connects to Logikal via a .NET Framework 4.7.2 bridge process.
/// The bridge handles the WCF named pipe IPC since full .NET Framework is required.
/// This service communicates with the bridge via HTTP on localhost.
/// </summary>
public class LogikalService : ILogikalService, IDisposable
{
    private readonly ILogger<LogikalService> _logger;
    private readonly LogikalSettings _settings;
    private readonly HttpClient _httpClient;
    private Process? _bridgeProcess;
    private readonly int _bridgePort;

    public bool IsConnected { get; private set; }

    public LogikalService(ILogger<LogikalService> logger, IOptions<LogikalSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
        _bridgePort = _settings.BridgePort > 0 ? _settings.BridgePort : 5100;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri($"http://localhost:{_bridgePort}"),
            Timeout = TimeSpan.FromSeconds(300)
        };
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        // First check if bridge is already running
        if (await CheckBridgeHealthAsync())
        {
            _logger.LogInformation("Bridge already running on port {Port}", _bridgePort);
            IsConnected = true;
            return;
        }

        // Start the bridge process - check multiple locations
        var bridgePath = Path.Combine(AppContext.BaseDirectory, "bridge", "LogikalMiddleware.Bridge.exe");

        if (!File.Exists(bridgePath))
        {
            // Try solution-relative path for development
            var devPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..",
                "LogikalMiddleware.Bridge", "bin", "Debug", "net472", "LogikalMiddleware.Bridge.exe"));
            if (File.Exists(devPath))
            {
                bridgePath = devPath;
            }
            else
            {
                _logger.LogError("Bridge executable not found. Checked: {Path1} and {Path2}. Build the Bridge project first.", bridgePath, devPath);
                throw new FileNotFoundException("Bridge executable not found. Build LogikalMiddleware.Bridge first.", bridgePath);
            }
        }

        _logger.LogInformation("Starting Logikal bridge at {Path}...", bridgePath);

        _bridgeProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = bridgePath,
                Arguments = string.IsNullOrWhiteSpace(_settings.ProjectCenterFilter)
                    ? $"\"{_settings.LauncherPath}\" {_bridgePort}"
                    : $"\"{_settings.LauncherPath}\" {_bridgePort} BIM \"{_settings.ProjectCenterFilter}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        _bridgeProcess.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null) _logger.LogInformation("[Bridge] {Data}", e.Data);
        };
        _bridgeProcess.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null) _logger.LogError("[Bridge] {Data}", e.Data);
        };

        _bridgeProcess.Start();
        _bridgeProcess.BeginOutputReadLine();
        _bridgeProcess.BeginErrorReadLine();

        // Wait for bridge to be ready
        for (int i = 0; i < 60; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(1000, cancellationToken);

            if (await CheckBridgeHealthAsync())
            {
                IsConnected = true;
                _logger.LogInformation("Bridge is ready and connected to Logikal");
                return;
            }
        }

        throw new TimeoutException("Bridge failed to start within 60 seconds");
    }

    private async Task<bool> CheckBridgeHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var health = JsonSerializer.Deserialize<JsonElement>(json);
                return health.GetProperty("status").GetString() == "connected";
            }
        }
        catch
        {
            // Bridge not running
        }
        return false;
    }

    public Task DisconnectAsync()
    {
        if (_bridgeProcess != null && !_bridgeProcess.HasExited)
        {
            _logger.LogInformation("Stopping bridge process...");
            try
            {
                _bridgeProcess.Kill();
                _bridgeProcess.WaitForExit(5000);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error stopping bridge process");
            }
            _bridgeProcess.Dispose();
            _bridgeProcess = null;
        }

        IsConnected = false;
        _logger.LogInformation("Disconnected from Logikal");
        return Task.CompletedTask;
    }

    public List<ProjectCenterDto> GetProjectCenters()
    {
        EnsureConnected();

        var response = _httpClient.GetAsync("/projectcenters").GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        return JsonSerializer.Deserialize<List<ProjectCenterDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<ProjectCenterDto>();
    }

    public List<ProjectDto> SearchProjects(string searchTerm)
    {
        EnsureConnected();

        var encoded = Uri.EscapeDataString(searchTerm);
        var response = _httpClient.GetAsync($"/projects/search?term={encoded}").GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        return JsonSerializer.Deserialize<List<ProjectDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<ProjectDto>();
    }

    public List<ElevationDto> GetElevations(Guid projectGuid)
    {
        EnsureConnected();

        var response = _httpClient.GetAsync($"/projects/{projectGuid}/elevations").GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        return JsonSerializer.Deserialize<List<ElevationDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<ElevationDto>();
    }

    public byte[] GetThumbnail(Guid elevationGuid)
    {
        EnsureConnected();

        var response = _httpClient.GetAsync($"/elevations/{elevationGuid}/thumbnail").GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        return response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
    }

    public string GetPartsList(Guid elevationGuid)
    {
        EnsureConnected();

        var response = _httpClient.GetAsync($"/elevations/{elevationGuid}/partslist").GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    }

    private void EnsureConnected()
    {
        if (!IsConnected)
            throw new InvalidOperationException("Not connected to Logikal. Call ConnectAsync first.");
    }

    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
        _httpClient.Dispose();
    }
}
