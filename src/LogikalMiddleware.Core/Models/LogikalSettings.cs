namespace LogikalMiddleware.Core.Models;

public class LogikalSettings
{
    public string LauncherPath { get; set; } = @"D:\LOGIKAL\LOGIKAL\winstart.exe";
    public string? ProjectCenterName { get; set; }
    public int BridgePort { get; set; } = 5100;
    /// <summary>
    /// Comma-separated list of project center folder names to scan.
    /// If empty/null, all centers are scanned.
    /// </summary>
    public string? ProjectCenterFilter { get; set; }
}
