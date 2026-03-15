
namespace Ivy.Samples.Shared.Apps.Widgets.Layouts;

[App(icon: Icons.Grid3x3, searchHints: ["layout", "grid", "heatmap", "cohort", "opacity"], isVisible: false)]
public class GridLayoutTestApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            .Gap(10)
            .Padding(5)
            | Text.H2("ColumnWidths (different column widths)")
            | Text.P("100px | 1fr | 150px")
            | (Layout.Grid()
                .Columns(3)
                .ColumnWidths(Size.Px(100), Size.Fraction(1), Size.Px(150))
                | "100px".WithCell().Background(Colors.Blue)
                | "1fr".WithCell().Background(Colors.Green)
                | "150px".WithCell().Background(Colors.Orange)
              )

            | Text.H2("RowHeights (different row heights)")
            | Text.P("Header 60px | Remaining 1fr")
            | (Layout.Grid()
                .Columns(3)
                .RowHeights(Size.Px(60), Size.Fraction(1), Size.Fraction(1))
                .Gap(5)
                | "Header (60px)".WithCell().Background(Colors.Green)
                | "Header".WithCell().Background(Colors.Green)
                | "Header".WithCell().Background(Colors.Green)
                | "Row 1fr".WithCell()
                | "Row 1fr".WithCell()
                | "Row 1fr".WithCell()
                | "Row 1fr".WithCell()
                | "Row 1fr".WithCell()
                | "Row 1fr".WithCell()
              )

            | Text.H2("HeaderBuilder")
            | Text.P("First row automatically styled")
            | (Layout.Grid()
                .Columns(4)
                .HeaderBuilder((i, e) => e.WithCell().Background(Colors.Green).Content($"Header {i}"))
                | "Cell 1" | "Cell 2" | "Cell 3" | "Cell 4"
                | "Cell 5" | "Cell 6" | "Cell 7" | "Cell 8"
              )

            | Text.H2("FooterBuilder")
            | Text.P("Last complete row styled as footer")
            | (Layout.Grid()
                .Columns(3)
                .FooterBuilder((i, e) => e.WithCell().Background(Colors.Green).Content($"Footer {i}"))
                .CellBuilder(e => e.WithCell())
                | "Cell 1" | "Cell 2" | "Cell 3"
                | "Cell 4" | "Cell 5" | "Cell 6"
                | "Footer 0" | "Footer 1" | "Footer 2"
              )

            | Text.H2("CellBuilder + override")
            | Text.P("All cells plain except one Rose")
            | (Layout.Grid()
                .Columns(3)
                .CellBuilder(e => e is Box ? e : e.WithCell().Content("Plain cell"))
                | "A"
                | "B"
                | "C".WithCell().Background(Colors.Rose).Content("Rose cell")
                | "D"
                | "E"
                | "F"
              )

            | Text.H2("Complex example (all together)")
            | Text.P("ColumnWidths + RowHeights + HeaderBuilder + CellBuilder")
            | (Layout.Grid()
                .Columns(4)
                .ColumnWidths(Size.Px(50), Size.Fraction(1), Size.Fraction(1), Size.Px(100))
                .HeaderBuilder((i, e) => e.WithCell().Background(Colors.Green).Content(i == 0 ? "ID" : i == 1 ? "Name" : i == 2 ? "Email" : "Actions"))
                .CellBuilder(e => e.WithCell())
                | "1" | "Alice" | "alice@example.com" | "Edit"
                | "2" | "Bob" | "bob@example.com" | "Edit"
                | "3" | "Charlie" | "charlie@example.com" | "Edit"
              )

            | Text.H2("WithCell() extension")
            | Text.P("Plain box without borders, fills cell completely")
            | (Layout.Grid()
                .Columns(3)
                .Gap(20)
                | "WithCell()".WithCell()
                | "WithCell().Background(Colors.Blue)".WithCell().Background(Colors.Blue)
                | "WithCell().Background(Green)".WithCell().Background(Colors.Green)
              )

            | Text.H2("Cohort Analysis")
            | Text.P("Demonstrating Color opacity for heatmaps (1.0 = strong, 0.1 = weak)")
            | (Layout.Grid()
                .Columns(6)
                .ColumnWidths(Size.Px(100), Size.Fraction(1), Size.Fraction(1), Size.Fraction(1), Size.Fraction(1), Size.Fraction(1))
                .Gap(2)
                // Header row
                | "Cohort".WithCell() | "Month 0".WithCell() | "Month 1".WithCell() | "Month 2".WithCell() | "Month 3".WithCell() | "Month 4".WithCell()
                // Row 1
                | "Jan 2024".WithCell()
                | "100%".WithCell().Background(Colors.Orange, 1.0f)
                | "85%".WithCell().Background(Colors.Orange, 0.85f)
                | "70%".WithCell().Background(Colors.Orange, 0.7f)
                | "55%".WithCell().Background(Colors.Orange, 0.55f)
                | "40%".WithCell().Background(Colors.Orange, 0.4f)
                // Row 2
                | "Feb 2024".WithCell()
                | "100%".WithCell().Background(Colors.Orange, 1.0f)
                | "80%".WithCell().Background(Colors.Orange, 0.8f)
                | "60%".WithCell().Background(Colors.Orange, 0.6f)
                | "40%".WithCell().Background(Colors.Orange, 0.4f)
                | "".WithCell().Background(Colors.Gray, 0.1f)
                // Row 3
                | "Mar 2024".WithCell()
                | "100%".WithCell().Background(Colors.Orange, 1.0f)
                | "90%".WithCell().Background(Colors.Orange, 0.9f)
                | "75%".WithCell().Background(Colors.Orange, 0.75f)
                | "".WithCell().Background(Colors.Gray, 0.1f)
                | "".WithCell().Background(Colors.Gray, 0.1f)
              )
            ;
    }
}
