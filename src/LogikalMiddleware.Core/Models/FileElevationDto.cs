namespace LogikalMiddleware.Core.Models;

public class FileElevationDto
{
    public Guid Guid { get; set; }
    public string PositionNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Width { get; set; }
    public double Height { get; set; }
    public string? System { get; set; }
    public string? SystemKey { get; set; }
    public string? State { get; set; }
    public string? Los { get; set; }
    public string? ColorWindow { get; set; }
    public bool HasWindow { get; set; }
    public double Area { get; set; }
    public int Quantity { get; set; }
}
