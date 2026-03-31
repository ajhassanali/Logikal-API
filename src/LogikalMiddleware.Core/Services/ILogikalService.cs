using LogikalMiddleware.Core.Models;

namespace LogikalMiddleware.Core.Services;

public interface ILogikalService
{
    bool IsConnected { get; }
    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync();
    List<ProjectCenterDto> GetProjectCenters();
    List<ProjectDto> SearchProjects(string searchTerm);
    List<ElevationDto> GetElevations(Guid projectGuid);
    byte[] GetThumbnail(Guid elevationGuid);
    string GetPartsList(Guid elevationGuid);
}
