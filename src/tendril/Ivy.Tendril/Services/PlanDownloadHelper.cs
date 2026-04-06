using Ivy.Tendril.Apps.Plans;

namespace Ivy.Tendril.Services;

public static class PlanDownloadHelper
{
    public static IState<string?> UsePlanDownload(IViewContext context, IPlanReaderService planService, PlanFile? plan)
    {
        var pdfService = new PlanPdfService();
        var planRef = context.UseRef<PlanFile?>(plan);
        planRef.Value = plan;

        var planIdState = context.UseState(plan?.Id ?? -1);
        if (planIdState.Value != (plan?.Id ?? -1))
            planIdState.Set(plan?.Id ?? -1);

        var url = context.UseState<string?>();
        var downloadService = context.UseService<IDownloadService>();

        context.UseEffect(() =>
        {
            var currentPlan = planRef.Value;
            var fileName = currentPlan != null ? currentPlan.FolderName + ".pdf" : "empty.pdf";
            var (cleanup, downloadUrl) = downloadService.AddDownload(
                () => currentPlan != null
                    ? Task.FromResult(pdfService.GeneratePdf(currentPlan.Title, currentPlan.Id, planService.ReadRawPlan(currentPlan.FolderName)))
                    : Task.FromResult(Array.Empty<byte>()),
                "application/pdf",
                fileName
            );
            url.Set(downloadUrl);
            return cleanup;
        }, planIdState, EffectTrigger.OnMount());

        return url;
    }
}
