using System.Threading.Tasks;
using Xunit;
using VerifyCS = Ivy.Analyser.Test.CSharpAnalyzerVerifier<
    Ivy.Analyser.Analyzers.LeafWidgetAnalyzer>;

namespace Ivy.Analyser.Test;

public class LeafWidgetAnalyzerTests
{
    private const string Stubs = @"
namespace Ivy
{
    public abstract class AbstractWidget
    {
        public object[] Children { get; set; }
        public static AbstractWidget operator |(AbstractWidget widget, object child) => widget;
    }

    public class Button : AbstractWidget
    {
        public Button(string text) { }
    }

    public class Badge : AbstractWidget
    {
        public Badge(string text) { }
    }

    public class Progress : AbstractWidget { }
    public class Field : AbstractWidget { }
    public class Detail : AbstractWidget { }
    public class Dialog : AbstractWidget { }
    public class DialogHeader : AbstractWidget { }
    public class HeaderLayout : AbstractWidget { }
    public class SidebarLayout : AbstractWidget { }
    public class SidebarMenu : AbstractWidget { }
    public class FooterLayout : AbstractWidget { }
    public class MenuItem : AbstractWidget
    {
        public MenuItem(string text) { }
    }

    [ChildType(typeof(MenuItem))]
    public class DropDownMenu : AbstractWidget { }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class ChildTypeAttribute : System.Attribute
    {
        public ChildTypeAttribute(System.Type type) { Type = type; }
        public System.Type Type { get; }
    }
    public class DataTable : AbstractWidget { }
    public class LineChart : AbstractWidget { }
    public class PieChart : AbstractWidget { }
    public class BarChart : AbstractWidget { }
    public class AreaChart : AbstractWidget { }
    public class Tooltip : AbstractWidget { }

    public interface IInput<T> { }
    public class TextInput : AbstractWidget, IInput<string> { }

    public class Card : AbstractWidget { }
    public class Sheet : AbstractWidget { }
    public class Confetti : AbstractWidget { }
    public class FloatingPanel : AbstractWidget { }

    public class Row : AbstractWidget { }
    public class Column : AbstractWidget { }
    public class Stack : AbstractWidget { }
    public class Text : AbstractWidget
    {
        public Text(string text) { }
    }
    public class Spacer : AbstractWidget { }
}
";

    // ── IVYCHILD001: Leaf widget errors ──

    [Fact]
    public async Task Button_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.Button(""Click"") | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Badge_WithWidgetChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.Badge(""x"") | new Ivy.Text(""y"")|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Progress_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.Progress() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Dialog_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.Dialog() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task DataTable_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.DataTable() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task LineChart_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.LineChart() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task PieChart_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.PieChart() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task BarChart_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.BarChart() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AreaChart_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.AreaChart() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Tooltip_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.Tooltip() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Field_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.Field() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Detail_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.Detail() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task DialogHeader_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.DialogHeader() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task HeaderLayout_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.HeaderLayout() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task SidebarLayout_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.SidebarLayout() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task FooterLayout_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.FooterLayout() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Spacer_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.Spacer() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    // ── IVYCHILD003: Wrong child type errors ──

    [Fact]
    public async Task DropDownMenu_WithMenuItem_NoDiagnostic()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = new Ivy.DropDownMenu() | new Ivy.MenuItem(""x"");
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task DropDownMenu_WithText_ReportsWrongChildType()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD003:new Ivy.DropDownMenu() | new Ivy.Text(""x"")|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task DropDownMenu_WithString_ReportsWrongChildType()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD003:new Ivy.DropDownMenu() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task DropDownMenu_WithMenuItemArray_NoDiagnostic()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var items = new Ivy.MenuItem[] { new Ivy.MenuItem(""a""), new Ivy.MenuItem(""b"") };
        var result = new Ivy.DropDownMenu() | items;
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task DropDownMenu_WithStringArray_ReportsWrongChildType()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var items = new string[] { ""a"", ""b"" };
        var result = {|IVYCHILD003:new Ivy.DropDownMenu() | items|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task DropDownMenu_WithMultipleMenuItems_NoDiagnostic()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = new Ivy.DropDownMenu() | new Ivy.MenuItem(""a"") | new Ivy.MenuItem(""b"");
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task SidebarMenu_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.SidebarMenu() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ButtonVariable_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var btn = new Ivy.Button(""x"");
        var result = {|IVYCHILD001:btn | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task IInputImplementor_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var input = new Ivy.TextInput();
        var result = {|IVYCHILD001:input | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task DerivedLeafWidget_WithChild_ReportsError()
    {
        var test = Stubs + @"
namespace Ivy
{
    public class PrimaryButton : Button
    {
        public PrimaryButton() : base(""primary"") { }
    }
}

public class Test
{
    public void M()
    {
        var result = {|IVYCHILD001:new Ivy.PrimaryButton() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    // ── No diagnostic: widgets that support children ──

    [Fact]
    public async Task Row_WithChild_NoDiagnostic()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = new Ivy.Row() | ""child"";
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Column_WithChild_NoDiagnostic()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = new Ivy.Column() | ""child"";
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Stack_WithMultipleChildren_NoDiagnostic()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = new Ivy.Stack() | ""child1"" | ""child2"" | ""child3"";
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    // ── IVYCHILD002: Single-child widget warnings ──

    [Fact]
    public async Task Card_SingleChild_NoDiagnostic()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = new Ivy.Card() | ""child"";
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Card_MultipleChildren_ReportsWarning()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD002:new Ivy.Card() | ""child1"" | ""child2""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Sheet_MultipleChildren_ReportsWarning()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD002:new Ivy.Sheet() | ""child1"" | ""child2""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Confetti_MultipleChildren_ReportsWarning()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD002:new Ivy.Confetti() | ""child1"" | ""child2""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task FloatingPanel_MultipleChildren_ReportsWarning()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD002:new Ivy.FloatingPanel() | ""child1"" | ""child2""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Card_ThreeChildren_ReportsWarningOnSecondAndThird()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = {|IVYCHILD002:{|IVYCHILD002:new Ivy.Card() | ""child1"" | ""child2""|}  | ""child3""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    // ── Method return and chained expressions ──

    [Fact]
    public async Task ButtonFromMethodReturn_WithChild_ReportsError()
    {
        var test = Stubs + @"
public class Test
{
    private Ivy.Button CreateButton() => new Ivy.Button(""x"");

    public void M()
    {
        var result = {|IVYCHILD001:CreateButton() | ""child""|};
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }

    // ── Integer bitwise OR should not trigger ──

    [Fact]
    public async Task IntegerBitwiseOr_NoDiagnostic()
    {
        var test = Stubs + @"
public class Test
{
    public void M()
    {
        var result = 1 | 2;
    }
}
";
        await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
