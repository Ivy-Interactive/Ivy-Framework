using Ivy.Tendril.Apps.Plans;

namespace Ivy.Tendril.Services;

public static class PlanExportHelper
{
    public const string YamlDelimiter = "---PLAN-YAML---";
    public const string RevisionDelimiter = "---PLAN-REVISION---";

    public static string ExportToClipboard(PlanFile plan)
    {
        return $"{YamlDelimiter}\n{plan.PlanYamlRaw.Trim()}\n{RevisionDelimiter}\n{plan.LatestRevisionContent.Trim()}";
    }
}
