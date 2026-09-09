using System.Xml.Linq;

namespace Ivy.Test.Widgets.External;

public class ExternalWidgetBuildTargetsTests
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
    public void BuildTargetFilesExistOnDisk()
    {
        var ivyTargets = Path.Combine(SrcDirectory, "Ivy", "Build", "Ivy.targets");
        var externalWidgetTargets = Path.Combine(SrcDirectory, "Ivy", "Build", "Ivy.ExternalWidget.targets");

        Assert.True(File.Exists(ivyTargets), $"Expected '{ivyTargets}' to exist.");
        Assert.True(File.Exists(externalWidgetTargets), $"Expected '{externalWidgetTargets}' to exist.");
    }

    [Fact]
    public void BuildTargetFilesAreValidXml()
    {
        var ivyTargets = Path.Combine(SrcDirectory, "Ivy", "Build", "Ivy.targets");
        var externalWidgetTargets = Path.Combine(SrcDirectory, "Ivy", "Build", "Ivy.ExternalWidget.targets");

        var doc1 = XDocument.Load(ivyTargets);
        var doc2 = XDocument.Load(externalWidgetTargets);

        Assert.NotNull(doc1.Root);
        Assert.NotNull(doc2.Root);
    }

    [Fact]
    public void IvyTargetsImportsExternalWidgetTargets()
    {
        var ivyTargets = Path.Combine(SrcDirectory, "Ivy", "Build", "Ivy.targets");
        var doc = XDocument.Load(ivyTargets);

        var import = doc.Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "Import" &&
                                (e.Attribute("Project")?.Value.Contains("Ivy.ExternalWidget.targets") ?? false));

        Assert.NotNull(import);
        Assert.Equal("$(MSBuildThisFileDirectory)Ivy.ExternalWidget.targets", import.Attribute("Project")?.Value);
        Assert.Equal("'$(IvyEnableExternalWidgets)' != 'false'", import.Attribute("Condition")?.Value);
    }

    [Fact]
    public void IvyCsprojPacksTargetFilesUnderBuildPackagePath()
    {
        var ivyCsproj = Path.Combine(SrcDirectory, "Ivy", "Ivy.csproj");
        var doc = XDocument.Load(ivyCsproj);

        var noneElements = doc.Descendants()
            .Where(e => e.Name.LocalName == "None")
            .ToList();

        var ivyTargetsItem = noneElements.FirstOrDefault(e => e.Attribute("Include")?.Value == "Build/Ivy.targets");
        Assert.NotNull(ivyTargetsItem);
        Assert.Equal("true", ivyTargetsItem.Attribute("Pack")?.Value);
        Assert.Equal("build", ivyTargetsItem.Attribute("PackagePath")?.Value);

        var externalWidgetTargetsItem = noneElements.FirstOrDefault(e => e.Attribute("Include")?.Value == "Build/Ivy.ExternalWidget.targets");
        Assert.NotNull(externalWidgetTargetsItem);
        Assert.Equal("true", externalWidgetTargetsItem.Attribute("Pack")?.Value);
        Assert.Equal("build", externalWidgetTargetsItem.Attribute("PackagePath")?.Value);
    }

    [Fact]
    public void IvyExternalWidgetTargetsUsesForwardSlashesAndGuardsDistResources()
    {
        var externalWidgetTargets = Path.Combine(SrcDirectory, "Ivy", "Build", "Ivy.ExternalWidget.targets");
        var content = File.ReadAllText(externalWidgetTargets);

        Assert.DoesNotContain("frontend\\", content);

        var doc = XDocument.Load(externalWidgetTargets);
        var distTarget = doc.Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "Target" && e.Attribute("Name")?.Value == "IncludeFrontendDistResources");

        Assert.NotNull(distTarget);
        Assert.Equal("Exists('frontend/package.json')", distTarget.Attribute("Condition")?.Value);
    }
}
