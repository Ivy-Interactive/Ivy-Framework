using Ivy;
using Ivy.Tendril.Apps.Plans;

namespace Ivy.Tendril.Services;

public static class PlanDownloadHelper
{
    public static IState<string?> UsePlanDownload(IViewContext context, PlanReaderService planService, PlanFile? plan)
    {
        // Always call UseDownload to maintain consistent hook count.
        // Pass a no-op factory when plan is null.
        var pdfService = new PlanPdfService();
        var fileName = plan != null
            ? Path.GetFileNameWithoutExtension(plan.FileName) + ".pdf"
            : "empty.pdf";
        return context.UseDownload(
            () => plan != null
                ? Task.FromResult(pdfService.GeneratePdf(plan.Title, plan.Id, planService.ReadRawPlan(plan.FileName)))
                : Task.FromResult(Array.Empty<byte>()),
            "application/pdf",
            fileName
        );
    }
}
