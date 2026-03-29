using Ivy;
using Ivy.Tendril.Apps.Plans;

namespace Ivy.Tendril.Services;

public static class PlanDownloadHelper
{
    public static IState<string?> UsePlanDownload(IViewContext context, PlanReaderService planService, PlanFile? plan)
    {
        var pdfService = new PlanPdfService();
        var fileName = plan != null
            ? plan.FolderName + ".pdf"
            : "empty.pdf";
        return context.UseDownload(
            () => plan != null
                ? Task.FromResult(pdfService.GeneratePdf(plan.Title, plan.Id, planService.ReadRawPlan(plan.FolderName)))
                : Task.FromResult(Array.Empty<byte>()),
            "application/pdf",
            fileName
        );
    }
}
