namespace LogikalMiddleware.Core.Models;

public class LogikalSettings
{
    public string LauncherPath { get; set; } = @"D:\LOGIKAL\LOGIKAL\winstart.exe";
    public string? ProjectCenterName { get; set; }
    public int BridgePort { get; set; } = 5100;
}
