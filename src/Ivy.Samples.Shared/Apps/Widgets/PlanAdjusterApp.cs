using Ivy.Widgets.PlanAdjuster;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.PencilLine, group: ["Widgets"], searchHints: ["plan", "adjust", "annotate", "markdown", "review"])]
public class PlanAdjusterApp : SampleBase
{
    const string SamplePlan = """
        # Migration Plan: Move billing service to Postgres

        This plan covers migrating the billing service from MySQL to Postgres
        over the next two sprints. Hover any paragraph and click the pencil
        icon to attach an adjustment note.

        ## Phase 1 — Schema parity

        Translate the existing MySQL schema to Postgres. Most types map
        cleanly; the main exceptions are `TINYINT(1)` (becomes `boolean`)
        and `DATETIME` (becomes `timestamptz`). Foreign keys and indexes
        carry over without changes.

        See the legacy schema dump at [billing.sql](file:///tmp/billing.sql)
        and the proposed Postgres version in
        [the parent plan](plan://billing-migration/postgres-schema).

        ## Phase 2 — Dual writes

        Run both databases in parallel for one week. The application writes
        to MySQL synchronously and to Postgres asynchronously, with a
        reconciler job comparing row counts every hour.

        ```csharp
        await mysql.InsertAsync(invoice);
        _ = postgres.InsertAsync(invoice); // fire-and-forget
        ```

        ## Phase 3 — Cutover

        Once reconciliation has been clean for 48 hours, flip the read path
        to Postgres while keeping MySQL writes for one more day as a
        rollback safety net. After that, MySQL becomes read-only and is
        archived. More context lives in the
        [project tracker](https://example.com/tracker/billing-pg).
        """;

    protected override object? BuildSample()
    {
        var client = UseService<IClientProvider>();

        var content = UseState(SamplePlan);
        var allowLocalFiles = UseState(false);
        var lastUpdate = UseState("(none yet)");
        var lastLinkClick = UseState("(none yet)");

        var adjuster = new PlanAdjuster()
            .Content(content.Value)
            .DangerouslyAllowLocalFiles(allowLocalFiles.Value)
            .OnUpdate(json =>
            {
                lastUpdate.Set(json);
                client.Toast("Plan adjustments submitted");
            })
            .OnLinkClick(url =>
            {
                lastLinkClick.Set(url);
                client.Toast($"Link clicked: {url}");
            });

        var controls = Layout.Vertical().Gap(2)
            | Text.H4("PlanAdjuster")
            | Text.Muted("Hover paragraphs to reveal the pencil icon. Click to attach an "
                      + "adjustment note. The floating Update button submits all adjustments "
                      + "as a JSON event.")
            | allowLocalFiles.ToSwitchInput(label: "DangerouslyAllowLocalFiles")
            | new Button("Reset content", _ => content.Set(SamplePlan))
                .Icon(Icons.RotateCcw)
                .Variant(ButtonVariant.Outline);

        var eventLog = Layout.Vertical().Gap(2)
            | Text.H4("Event log")
            | Text.Strong("Last OnUpdate payload")
            | new CodeBlock(lastUpdate.Value, Languages.Json).ShowCopyButton(false)
            | Text.Strong("Last OnLinkClick URL")
            | Text.Code(lastLinkClick.Value);

        return Layout.Vertical().Gap(4)
            | Text.H1("Plan Adjuster")
            | (Layout.Grid().Columns(2).Gap(4)
                | controls
                | eventLog)
            | adjuster;
    }
}
