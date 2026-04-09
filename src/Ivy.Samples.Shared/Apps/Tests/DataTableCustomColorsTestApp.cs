namespace Ivy.Samples.Shared.Apps.Tests;

[App(icon: Icons.Palette, group: ["Tests"], isVisible: true, searchHints: ["datatable", "badge", "colors", "labels", "mapping"])]
public class DataTableCustomColorsTestApp : SampleBase
{
    private sealed record RowData(int Id, string Name, string[] Tags, string[] Categories);

    private static readonly RowData[] Rows =
    [
        new(1, "Aurora", ["Feature", "UI"], ["Critical", "Customer"]),
        new(2, "Nimbus", ["Bug", "Backend"], ["High", "Internal"]),
        new(3, "Solstice", ["Docs", "Onboarding"], ["Medium", "Community"])
    ];

    protected override object? BuildSample()
    {
        var data = Rows.AsQueryable();

        var badgesColor = data.ToDataTable()
            .Header(x => x.Id, "ID")
            .Header(x => x.Name, "Name")
            .Header(x => x.Tags, "Tags via .Badges(Colors)")
            .Badges(x => x.Tags, Colors.Success)
            .Width(x => x.Id, Size.Px(60))
            .Width(x => x.Name, Size.Px(160))
            .Height(Size.Units(40));

        var rendererColorMapping = data.ToDataTable()
            .Header(x => x.Id, "ID")
            .Header(x => x.Name, "Name")
            .Header(x => x.Categories, "Tags via BadgeColorMapping")
            .Renderer(x => x.Categories, new LabelsDisplayRenderer
            {
                Color = Colors.Info,
                BadgeColorMapping = new Dictionary<string, string>
                {
                    ["Critical"] = nameof(Colors.Destructive),
                    ["High"] = nameof(Colors.Warning),
                    ["Medium"] = nameof(Colors.Info),
                    ["Customer"] = nameof(Colors.Success),
                    ["Internal"] = nameof(Colors.Secondary),
                    ["Community"] = nameof(Colors.IvyGreen)
                }
            })
            .Width(x => x.Id, Size.Px(60))
            .Width(x => x.Name, Size.Px(160))
            .Height(Size.Units(40));

        return Layout.Vertical()
               | Text.H1("DataTable Badge Colors Test")
               | Text.P("Coverage app for enum-based DataTable label colors.")
               | Text.H2("Badges(field, Colors)")
               | badgesColor
               | Text.H2("LabelsDisplayRenderer.Color + BadgeColorMapping")
               | rendererColorMapping;
    }
}
