using System.Text;
using System.Xml.Linq;
using LogikalMiddleware.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogikalMiddleware.Core.Services;

public class FileDataService : IFileDataService
{
    private readonly ILogger<FileDataService> _logger;
    private readonly string _dirsPath;

    public FileDataService(ILogger<FileDataService> logger, IOptions<LogikalSettings> settings)
    {
        _logger = logger;

        // Derive data directory from LauncherPath: {parent}/objekte/DIRS
        var launcherDir = Path.GetDirectoryName(settings.Value.LauncherPath) ?? @"D:\LOGIKAL\LOGIKAL";
        _dirsPath = Path.Combine(launcherDir, "objekte", "DIRS");

        _logger.LogInformation("FileDataService initialized with data path: {Path}", _dirsPath);
    }

    public List<FileProjectDto> GetAllProjects()
    {
        var projects = new List<FileProjectDto>();

        if (!Directory.Exists(_dirsPath))
        {
            _logger.LogWarning("Data directory does not exist: {Path}", _dirsPath);
            return projects;
        }

        foreach (var centerDir in Directory.GetDirectories(_dirsPath))
        {
            var centerName = Path.GetFileName(centerDir);
            ScanForProjects(centerDir, centerName, projects);

            // Also scan QUOTATION subfolder if it exists
            var quotationDir = Path.Combine(centerDir, "QUOTATION");
            if (Directory.Exists(quotationDir))
            {
                ScanForProjects(quotationDir, centerName, projects);
            }
        }

        return projects;
    }

    private void ScanForProjects(string parentDir, string centerName, List<FileProjectDto> projects)
    {
        foreach (var objDir in Directory.GetDirectories(parentDir, "OBJ*"))
        {
            try
            {
                var project = ParseProject(objDir, centerName);
                if (project != null)
                    projects.Add(project);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse project in {Dir}", objDir);
            }
        }
    }

    private FileProjectDto? ParseProject(string objDir, string centerName)
    {
        // Build relative folder path from DIRS root
        var relativePath = Path.GetRelativePath(_dirsPath, objDir).Replace('\\', '/');

        var project = new FileProjectDto
        {
            FolderPath = relativePath,
            ProjectCenter = centerName,
            Name = Path.GetFileName(objDir)
        };

        // Parse PROJECT.PRJ for guid, dates, user
        var prjPath = Path.Combine(objDir, "PROJECT.PRJ");
        if (File.Exists(prjPath))
        {
            try
            {
                var doc = LoadXml(prjPath);
                var projectEl = doc?.Root;
                if (projectEl != null)
                {
                    var guidStr = projectEl.Attribute("Guid")?.Value;
                    if (Guid.TryParse(guidStr, out var guid))
                        project.ProjectGuid = guid;

                    // Get date/user from Head > Version
                    var headVersion = projectEl.Element("Head")?.Element("Version");
                    if (headVersion != null)
                    {
                        project.DateCreated = headVersion.Attribute("DateCreated")?.Value;
                        project.UserCreated = headVersion.Attribute("UserCreated")?.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse PROJECT.PRJ in {Dir}", objDir);
            }
        }

        // Parse POSITIONINFO.XML for elevations and project name
        var posInfoPath = Path.Combine(objDir, "POSITIONINFO.XML");
        if (File.Exists(posInfoPath))
        {
            try
            {
                var doc = LoadXml(posInfoPath);
                var positions = doc?.Root?.Element("Positions")?.Elements("TPositionInformation").ToList();
                if (positions != null)
                {
                    project.ElevationCount = positions.Count;

                    // Use first elevation's Los/ShortDescription/ModelDescription as project name
                    var first = positions.FirstOrDefault();
                    if (first != null)
                    {
                        var name = first.Attribute("ShortDescription")?.Value
                                ?? first.Attribute("ModelDescription")?.Value
                                ?? first.Attribute("System")?.Value;
                        if (!string.IsNullOrWhiteSpace(name))
                            project.Name = name;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse POSITIONINFO.XML in {Dir}", objDir);
            }
        }

        // Parse ANSCHR.XML for customer name
        var anschrPath = Path.Combine(objDir, "ANSCHR.XML");
        if (File.Exists(anschrPath))
        {
            try
            {
                var doc = LoadXml(anschrPath);
                var address = doc?.Root?.Element("Address");
                if (address != null)
                {
                    var kfmContact = address.Attribute("KfmContact")?.Value;
                    var name1 = address.Attribute("Name1")?.Value;
                    project.CustomerName = !string.IsNullOrWhiteSpace(name1) ? name1
                        : !string.IsNullOrWhiteSpace(kfmContact) ? kfmContact
                        : null;

                    var order = address.Attribute("Order")?.Value;
                    if (!string.IsNullOrWhiteSpace(order))
                        project.JobNumber = order;

                    var offer = address.Attribute("Offer")?.Value;
                    if (!string.IsNullOrWhiteSpace(offer))
                        project.OfferNumber = offer;

                    // Use Order as project name if we only have folder name
                    if (!string.IsNullOrWhiteSpace(order) && project.Name == Path.GetFileName(objDir))
                        project.Name = order;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse ANSCHR.XML in {Dir}", objDir);
            }
        }

        return project;
    }

    public List<FileElevationDto> GetElevations(string folderPath)
    {
        var elevations = new List<FileElevationDto>();
        var fullPath = Path.GetFullPath(Path.Combine(_dirsPath, folderPath));

        // Security: ensure resolved path is within _dirsPath
        if (!fullPath.StartsWith(Path.GetFullPath(_dirsPath), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Path traversal attempt blocked: {Path}", folderPath);
            return elevations;
        }

        var posInfoPath = Path.Combine(fullPath, "POSITIONINFO.XML");
        if (!File.Exists(posInfoPath))
        {
            _logger.LogWarning("POSITIONINFO.XML not found in {Path}", fullPath);
            return elevations;
        }

        try
        {
            var doc = LoadXml(posInfoPath);
            var positions = doc?.Root?.Element("Positions")?.Elements("TPositionInformation");
            if (positions == null) return elevations;

            foreach (var pos in positions)
            {
                var guidEl = pos.Element("GUID");
                var guidStr = guidEl?.Attribute("Value")?.Value;
                Guid.TryParse(guidStr, out var guid);

                var stk = pos.Attribute("Stk")?.Value;
                int.TryParse(stk, out var quantity);
                if (quantity == 0) quantity = 1;

                var widthStr = pos.Attribute("Width")?.Value;
                double.TryParse(widthStr, out var width);

                var heightStr = pos.Attribute("Height")?.Value;
                double.TryParse(heightStr, out var height);

                var areaStr = pos.Attribute("Area")?.Value;
                double.TryParse(areaStr, out var area);

                var hasWindowStr = pos.Attribute("HasWindow")?.Value;
                bool.TryParse(hasWindowStr, out var hasWindow);

                var name = pos.Attribute("ShortDescription")?.Value
                        ?? pos.Attribute("ModelDescription")?.Value
                        ?? pos.Attribute("System")?.Value
                        ?? $"Position {pos.Attribute("PosNr")?.Value}";

                elevations.Add(new FileElevationDto
                {
                    Guid = guid,
                    PositionNumber = pos.Attribute("PosNr")?.Value ?? "",
                    Name = name,
                    Width = width,
                    Height = height,
                    System = pos.Attribute("System")?.Value,
                    SystemKey = pos.Attribute("SystemKey")?.Value,
                    State = pos.Attribute("State")?.Value,
                    ColorWindow = pos.Attribute("ColorWindow")?.Value,
                    HasWindow = hasWindow,
                    Area = area,
                    Quantity = quantity
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse elevations from {Path}", posInfoPath);
        }

        return elevations;
    }

    /// <summary>
    /// Load XML with BOM detection to handle both UTF-8 and UTF-16 encoded files.
    /// </summary>
    private static XDocument? LoadXml(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length == 0) return null;

        // Detect UTF-16 LE BOM (FF FE) or UTF-16 BE BOM (FE FF)
        Encoding encoding;
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            encoding = Encoding.Unicode; // UTF-16 LE
        else if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            encoding = Encoding.BigEndianUnicode; // UTF-16 BE
        else
            encoding = Encoding.UTF8;

        var text = encoding.GetString(bytes);

        // Remove BOM character if present
        if (text.Length > 0 && text[0] == '\uFEFF')
            text = text[1..];

        // Some files may have null characters from UTF-16 without BOM detection
        text = text.Replace("\0", "");

        return XDocument.Parse(text);
    }
}
