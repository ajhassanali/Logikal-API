namespace LogikalMiddleware.Core.Models;

public class FileProjectDto
{
    public string FolderPath { get; set; } = string.Empty;
    public Guid? ProjectGuid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? JobNumber { get; set; }
    public string? OfferNumber { get; set; }
    public string? ProjectCenter { get; set; }
    public string? DateCreated { get; set; }
    public string? UserCreated { get; set; }
    public int ElevationCount { get; set; }
}
