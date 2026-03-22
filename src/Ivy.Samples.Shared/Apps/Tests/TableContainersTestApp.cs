namespace Ivy.Samples.Shared.Apps.Tests;

[App(icon: Icons.Table, group: ["Tests"], isVisible: false, searchHints: ["table", "blade", "sheet", "card", "box", "container", "width"])]
public class TableContainersTestApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("Blades", new BladesTest()),
            new Tab("Sheets", new SheetsTest()),
            new Tab("Cards", new CardsTest()),
            new Tab("Boxes", new BoxesTest())
        ).Variant(TabsVariant.Content);
    }
}

// Test tables in blades with different widths
public class BladesTest : ViewBase
{
    public override object? Build()
    {
        var blades = UseBlades(() => new BladeContent(), "Tables in Blades");
        return blades;
    }
}

public class BladeContent : ViewBase
{
    public override object? Build()
    {
        var bladeController = UseContext<IBladeService>();

        void PushNoWidth(Event<Baton> e) => bladeController.Push(this, new BladeTableNoWidth(), "No Width");
        void PushFull(Event<Baton> e) => bladeController.Push(this, new BladeTableFull(), "Width: Full");
        void PushUnits(Event<Baton> e) => bladeController.Push(this, new BladeTableUnits(), "Width: Units(80)");
        void PushPx(Event<Baton> e) => bladeController.Push(this, new BladeTablePx(), "Width: Px(400)");
        void PushFit(Event<Baton> e) => bladeController.Push(this, new BladeTableFit(), "Width: Fit");

        return Layout.Vertical()
            | Text.H3("Tables in Blades")
            | Text.P("Click buttons to open blades with different table widths")
            | new Baton("No Width Set", PushNoWidth)
            | new Baton("Width: Full", PushFull)
            | new Baton("Width: Units(80)", PushUnits)
            | new Baton("Width: Px(400)", PushPx)
            | new Baton("Width: Fit", PushFit);
    }
}

public class BladeTableNoWidth : ViewBase
{
    private record TestData(string Name, string Status, int Count);

    private static TestData[] GetSampleData() => new[]
    {
        new TestData("Item Alpha", "Active", 42),
        new TestData("Item Beta", "Pending", 15),
        new TestData("Item Gamma", "Complete", 89),
    };

    public override object? Build()
    {
        return Layout.Vertical()
            | Text.Label("Table with no explicit width (should default to Full)")
            | GetSampleData().ToTable();
    }
}

public class BladeTableFull : ViewBase
{
    private record TestData(string Name, string Status, int Count);

    private static TestData[] GetSampleData() => new[]
    {
        new TestData("Item Alpha", "Active", 42),
        new TestData("Item Beta", "Pending", 15),
        new TestData("Item Gamma", "Complete", 89),
    };

    public override object? Build()
    {
        return Layout.Vertical()
            | Text.Label("Table with Width(Size.Full())")
            | GetSampleData().ToTable().Width(Size.Full());
    }
}

public class BladeTableUnits : ViewBase
{
    private record TestData(string Name, string Status, int Count);

    private static TestData[] GetSampleData() => new[]
    {
        new TestData("Item Alpha", "Active", 42),
        new TestData("Item Beta", "Pending", 15),
        new TestData("Item Gamma", "Complete", 89),
    };

    public override object? Build()
    {
        return Layout.Vertical()
            | Text.Label("Table with Width(Size.Units(80))")
            | GetSampleData().ToTable().Width(Size.Units(80));
    }
}

public class BladeTablePx : ViewBase
{
    private record TestData(string Name, string Status, int Count);

    private static TestData[] GetSampleData() => new[]
    {
        new TestData("Item Alpha", "Active", 42),
        new TestData("Item Beta", "Pending", 15),
        new TestData("Item Gamma", "Complete", 89),
    };

    public override object? Build()
    {
        return Layout.Vertical()
            | Text.Label("Table with Width(Size.Px(400))")
            | GetSampleData().ToTable().Width(Size.Px(400));
    }
}

public class BladeTableFit : ViewBase
{
    private record TestData(string Name, string Status, int Count);

    private static TestData[] GetSampleData() => new[]
    {
        new TestData("Item Alpha", "Active", 42),
        new TestData("Item Beta", "Pending", 15),
        new TestData("Item Gamma", "Complete", 89),
    };

    public override object? Build()
    {
        return Layout.Vertical()
            | Text.Label("Table with Width(Size.Fit())")
            | GetSampleData().ToTable().Width(Size.Fit());
    }
}

// Test tables in sheets
public class SheetsTest : ViewBase
{
    private record TestData(string Name, string Status, int Count);

    private static TestData[] GetSampleData() => new[]
    {
        new TestData("Item Alpha", "Active", 42),
        new TestData("Item Beta", "Pending", 15),
        new TestData("Item Gamma", "Complete", 89),
    };

    public override object? Build()
    {
        var sheetNoWidth = UseState<bool>(false);
        var sheetFull = UseState<bool>(false);
        var sheetUnits = UseState<bool>(false);
        var sheetPx = UseState<bool>(false);
        var sheetFit = UseState<bool>(false);

        return new Fragment(
            Layout.Vertical()
                | Text.H3("Tables in Sheets")
                | Text.P("Click buttons to open sheets with different table widths")
                | Layout.Horizontal(
                    new Baton("No Width", _ => sheetNoWidth.Set(true)),
                    new Baton("Width: Full", _ => sheetFull.Set(true)),
                    new Baton("Width: Units(80)", _ => sheetUnits.Set(true)),
                    new Baton("Width: Px(400)", _ => sheetPx.Set(true)),
                    new Baton("Width: Fit", _ => sheetFit.Set(true))
                ).Gap(4),

            sheetNoWidth.Value ? new Sheet(
                onClose: () => sheetNoWidth.Set(false),
                content: Layout.Vertical()
                    | Text.H4("Table with No Width")
                    | Text.Label("Table with no explicit width (should default to Full)")
                    | GetSampleData().ToTable()
            ) : null,

            sheetFull.Value ? new Sheet(
                onClose: () => sheetFull.Set(false),
                content: Layout.Vertical()
                    | Text.H4("Table with Full Width")
                    | Text.Label("Table with Width(Size.Full())")
                    | GetSampleData().ToTable().Width(Size.Full())
            ) : null,

            sheetUnits.Value ? new Sheet(
                onClose: () => sheetUnits.Set(false),
                content: Layout.Vertical()
                    | Text.H4("Table with Units(80)")
                    | Text.Label("Table with Width(Size.Units(80))")
                    | GetSampleData().ToTable().Width(Size.Units(80))
            ) : null,

            sheetPx.Value ? new Sheet(
                onClose: () => sheetPx.Set(false),
                content: Layout.Vertical()
                    | Text.H4("Table with Px(400)")
                    | Text.Label("Table with Width(Size.Px(400))")
                    | GetSampleData().ToTable().Width(Size.Px(400))
            ) : null,

            sheetFit.Value ? new Sheet(
                onClose: () => sheetFit.Set(false),
                content: Layout.Vertical()
                    | Text.H4("Table with Fit")
                    | Text.Label("Table with Width(Size.Fit())")
                    | GetSampleData().ToTable().Width(Size.Fit())
            ) : null
        );
    }
}

// Test tables in cards
public class CardsTest : ViewBase
{
    private record TestData(string Name, string Status, int Count);

    private static TestData[] GetSampleData() => new[]
    {
        new TestData("Item Alpha", "Active", 42),
        new TestData("Item Beta", "Pending", 15),
        new TestData("Item Gamma", "Complete", 89),
    };

    public override object? Build()
    {
        return Layout.Vertical()
            | Text.H3("Tables in Cards")
            | Text.P("Testing table width behavior in card containers")

            | Layout.Vertical(
                new Card(
                    content: Layout.Vertical()
                        | Text.Label("Table with no explicit width (should default to Full)")
                        | GetSampleData().ToTable()
                ).Title("Card: Table with No Width"),

                new Card(
                    content: Layout.Vertical()
                        | Text.Label("Table with Width(Size.Full())")
                        | GetSampleData().ToTable().Width(Size.Full())
                ).Title("Card: Table with Full Width"),

                new Card(
                    content: Layout.Vertical()
                        | Text.Label("Table with Width(Size.Units(80))")
                        | GetSampleData().ToTable().Width(Size.Units(80))
                ).Title("Card: Table with Units(80)"),

                new Card(
                    content: Layout.Vertical()
                        | Text.Label("Table with Width(Size.Px(400))")
                        | GetSampleData().ToTable().Width(Size.Px(400))
                ).Title("Card: Table with Px(400)"),

                new Card(
                    content: Layout.Vertical()
                        | Text.Label("Table with Width(Size.Fit())")
                        | GetSampleData().ToTable().Width(Size.Fit())
                ).Title("Card: Table with Fit")
            ).Gap(4);
    }
}

// Long text for box width tests
internal static class BoxTestContent
{
    public const string LongText =
        "This is a long paragraph inside a box to test how width behaves. " +
        "Fixed widths (Units, Px) constrain the box; Fit sizes to content.";
}

// Test boxes with long text: default, Full, fixed, Fit
public class BoxesTest : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            | Text.H3("Boxes with Long Text")
            | Text.P("Testing box width behavior: default, Full, fixed (Units/Px), Fit")

            | Layout.Vertical(
                new Card(
                    content: Layout.Vertical()
                        | Text.Label("Box with no explicit width (default)")
                        | new Box(Text.P(BoxTestContent.LongText)).Background(Colors.Primary)
                ).Title("Card: Box with No Width"),

                new Card(
                    content: Layout.Vertical()
                        | Text.Label("Box with Width(Size.Full())")
                        | new Box(Text.P(BoxTestContent.LongText)).Width(Size.Full()).Background(Colors.Primary)
                ).Title("Card: Box with Full Width"),

                new Card(
                    content: Layout.Vertical()
                        | Text.Label("Box with Width(Size.Units(80))")
                        | new Box(Text.P(BoxTestContent.LongText)).Width(Size.Units(80)).Background(Colors.Primary)
                ).Title("Card: Box with Units(80)"),

                new Card(
                    content: Layout.Vertical()
                        | Text.Label("Box with Width(Size.Px(400))")
                        | new Box(Text.P(BoxTestContent.LongText)).Width(Size.Px(400)).Background(Colors.Primary)
                ).Title("Card: Box with Px(400)"),

                new Card(
                    content: Layout.Vertical()
                        | Text.Label("Box with Width(Size.Fit())")
                        | new Box(Text.P(BoxTestContent.LongText)).Width(Size.Fit()).Background(Colors.Primary)
                ).Title("Card: Box with Fit")
            ).Gap(4);
    }
}
