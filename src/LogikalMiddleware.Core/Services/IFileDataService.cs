using LogikalMiddleware.Core.Models;

namespace LogikalMiddleware.Core.Services;

public interface IFileDataService
{
    List<FileProjectDto> GetAllProjects();
    List<FileElevationDto> GetElevations(string folderPath);
}
