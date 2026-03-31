namespace LogikalMiddleware.Core.Models;

public class ElevationDto
{
    public Guid Guid { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Width { get; set; }
    public double Height { get; set; }
    public string? Description { get; set; }
    public string? ElementType { get; set; }
    public double Amount { get; set; }
    public string? Unit { get; set; }
}
