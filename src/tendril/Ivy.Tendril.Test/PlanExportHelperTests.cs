using Ivy.Tendril.Apps.Plans;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class PlanExportHelperTests
{
    private static PlanFile CreateTestPlan(string yamlRaw, string revisionContent)
    {
        var metadata = new PlanMetadata(1, "Test", "NiceToHave", "Test Plan", PlanStatus.Draft,
            [], [], [], [], [], [], DateTime.UtcNow, DateTime.UtcNow, null, null);
        return new PlanFile(metadata, revisionContent, "/tmp/test", yamlRaw);
    }

    [Fact]
    public void ExportToClipboard_ContainsBothDelimiters()
    {
        var plan = CreateTestPlan("state: Draft\ntitle: Test", "# Test\n\nContent");

        var result = PlanExportHelper.ExportToClipboard(plan);

        Assert.Contains(PlanExportHelper.YamlDelimiter, result);
        Assert.Contains(PlanExportHelper.RevisionDelimiter, result);
    }

    [Fact]
    public void ExportToClipboard_ContainsYamlContent()
    {
        var yaml = "state: Draft\ntitle: Test Plan";
        var plan = CreateTestPlan(yaml, "# Revision");

        var result = PlanExportHelper.ExportToClipboard(plan);

        Assert.Contains("state: Draft", result);
        Assert.Contains("title: Test Plan", result);
    }

    [Fact]
    public void ExportToClipboard_ContainsRevisionContent()
    {
        var plan = CreateTestPlan("state: Draft", "# My Plan\n\n## Problem\n\nSomething");

        var result = PlanExportHelper.ExportToClipboard(plan);

        Assert.Contains("# My Plan", result);
        Assert.Contains("## Problem", result);
    }

    [Fact]
    public void ExportToClipboard_YamlAppearsBeforeRevision()
    {
        var plan = CreateTestPlan("state: Draft", "# Revision");

        var result = PlanExportHelper.ExportToClipboard(plan);

        var yamlIndex = result.IndexOf(PlanExportHelper.YamlDelimiter);
        var revisionIndex = result.IndexOf(PlanExportHelper.RevisionDelimiter);
        Assert.True(yamlIndex < revisionIndex);
    }

    [Fact]
    public void ExportToClipboard_TrimsWhitespace()
    {
        var plan = CreateTestPlan("  state: Draft  \n", "\n  # Revision  \n");

        var result = PlanExportHelper.ExportToClipboard(plan);

        Assert.StartsWith(PlanExportHelper.YamlDelimiter + "\nstate: Draft", result);
        Assert.EndsWith("# Revision", result);
    }

    [Fact]
    public void ExportToClipboard_CorrectFormat()
    {
        var yaml = "state: Draft\ntitle: Test";
        var revision = "# Plan Content";
        var plan = CreateTestPlan(yaml, revision);

        var result = PlanExportHelper.ExportToClipboard(plan);

        var expected = $"---PLAN-YAML---\n{yaml}\n---PLAN-REVISION---\n{revision}";
        Assert.Equal(expected, result);
    }
}
