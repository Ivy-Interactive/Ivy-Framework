using System.Text;
using Ivy;
using Ivy.Tendril.Apps.Plans;

namespace Ivy.Tendril.Services;

public static class PlanDownloadHelper
{
    public static IState<string?> UsePlanDownload(IViewContext context, PlanReaderService planService, PlanFile? plan)
    {
        // Always call UseDownload to maintain consistent hook count.
        // Pass a no-op factory when plan is null.
        var fileName = plan != null ? Path.GetFileName(plan.FileName) : "empty.md";
        return context.UseDownload(
            () => plan != null
                ? Task.FromResult(Encoding.UTF8.GetBytes(planService.ReadRawPlan(plan.FileName)))
                : Task.FromResult(Array.Empty<byte>()),
            "text/markdown",
            fileName
        );
    }
}
