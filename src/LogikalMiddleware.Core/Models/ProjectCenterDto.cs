namespace LogikalMiddleware.Core.Models;

public class ProjectCenterDto
{
    public string DirectoryName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public int TypeId { get; set; }
    public bool IsRecycleBin { get; set; }
}
