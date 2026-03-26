using System.Text;
using Ivy;
using Ivy.Tendril.Apps.Plans;

namespace Ivy.Tendril.Services;

public static class PlanDownloadHelper
{
    public static IState<string?> UsePlanDownload(ViewBase view, PlanReaderService planService, PlanFile? plan)
    {
        if (plan == null)
            return view.Context.UseState<string?>();

        var fileName = Path.GetFileName(plan.FileName);
        return view.Context.UseDownload(
            () => Task.FromResult(Encoding.UTF8.GetBytes(planService.ReadRawPlan(plan.FileName))),
            "text/markdown",
            fileName
        );
    }
}
