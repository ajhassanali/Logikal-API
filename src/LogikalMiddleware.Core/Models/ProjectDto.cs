namespace LogikalMiddleware.Core.Models;

public class ProjectDto
{
    public Guid Guid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? JobNumber { get; set; }
    public string? OfferNumber { get; set; }
    public string? CustomerName { get; set; }
}
