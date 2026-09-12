using System.Xml.Linq;

namespace Ivy.Test;

public class SolutionMembershipTests
{
    private static readonly string SrcDirectory = FindSrcDirectory();

    private static string FindSrcDirectory()
    {
        var dir = new DirectoryInfo(System.AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Ivy-Framework.slnx")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return dir.FullName;
    }

    [Fact]
    public void EveryTestProjectDirectlyUnderSrcIsASolutionMember()
    {
        var slnxPath = Path.Combine(SrcDirectory, "Ivy-Framework.slnx");
        var doc = XDocument.Load(slnxPath);

        var solutionProjects = doc.Descendants()
            .Where(e => e.Name.LocalName == "Project")
            .Select(e => e.Attribute("Path")?.Value)
            .Where(p => p != null)
            .Select(p => p!.Replace('\\', '/'))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var testDirectories = Directory.GetDirectories(SrcDirectory)
            .Select(d => new DirectoryInfo(d))
            .Where(d => d.Name.EndsWith(".Test", StringComparison.Ordinal) ||
                       d.Name.EndsWith(".Tests", StringComparison.Ordinal))
            .ToList();

        var missingProjects = new List<string>();

        foreach (var testDir in testDirectories)
        {
            var projectFile = Path.Combine(testDir.FullName, $"{testDir.Name}.csproj");
            if (File.Exists(projectFile))
            {
                var relativePath = $"{testDir.Name}/{testDir.Name}.csproj";
                if (!solutionProjects.Contains(relativePath))
                {
                    missingProjects.Add(relativePath);
                }
            }
        }

        Assert.Empty(missingProjects);
    }
}
