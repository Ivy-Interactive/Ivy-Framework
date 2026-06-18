namespace Ivy.Test.Widgets;

public class SheetResizableTests
{
    private static Sheet MakeSheet() =>
        new Sheet((Action?)null, content: "content");

    [Fact]
    public void Resizable_PreservesEveryResponsiveBreakpoint()
    {
        // Arrange: a responsive width with NO Default — only per-breakpoint rules.
        // This is the shape that previously collapsed to the fallback default width.
        Responsive<Size> responsiveWidth =
            Size.Full().At(Breakpoint.Mobile)
                .And(Breakpoint.Tablet, Size.Fraction(0.75f))
                .And(Breakpoint.Desktop, Size.Fraction(0.5f))
                .And(Breakpoint.Wide, Size.Fraction(0.4f));

        // Act
        var sheet = MakeSheet().Width(responsiveWidth).Resizable();

        // Assert: every breakpoint survives (no collapse to a single default).
        Assert.NotNull(sheet.Width);
        Assert.Null(sheet.Width!.Default);
        Assert.Equal("Full,Px:200,Px:1200", sheet.Width.Mobile!.ToString());
        Assert.Equal("Fraction:0.75,Px:200,Px:1200", sheet.Width.Tablet!.ToString());
        Assert.Equal("Fraction:0.5,Px:200,Px:1200", sheet.Width.Desktop!.ToString());
        Assert.Equal("Fraction:0.4,Px:200,Px:1200", sheet.Width.Wide!.ToString());
    }

    [Fact]
    public void Resizable_KeepsExplicitMinMaxPerBreakpoint()
    {
        // Arrange: a breakpoint that already specifies its own Min/Max must not be overridden.
        Responsive<Size> responsiveWidth =
            Size.Fraction(0.5f).Min(Size.Px(300)).Max(Size.Px(900)).At(Breakpoint.Desktop)
                .And(Breakpoint.Wide, Size.Fraction(0.4f));

        // Act
        var sheet = MakeSheet().Width(responsiveWidth).Resizable();

        // Assert: explicit constraints preserved; missing ones filled with defaults.
        Assert.Equal("Fraction:0.5,Px:300,Px:900", sheet.Width!.Desktop!.ToString());
        Assert.Equal("Fraction:0.4,Px:200,Px:1200", sheet.Width.Wide!.ToString());
    }

    [Fact]
    public void Resizable_NoWidthSet_FallsBackToConstrainedDefault()
    {
        // Act: a sheet with the default (non-responsive) width gets constraints on its Default.
        var sheet = MakeSheet().Resizable();

        // Assert: Sheet.DefaultWidth (Rem 24) with default min/max, on Default.
        Assert.Equal("Rem:24,Px:200,Px:1200", sheet.Width!.Default!.ToString());
        Assert.Null(sheet.Width.Mobile);
    }

    [Fact]
    public void Resizable_Vertical_PreservesResponsiveHeightBreakpoints()
    {
        // Arrange: top/bottom sheets resize on the height axis.
        Responsive<Size> responsiveHeight =
            Size.Full().At(Breakpoint.Mobile)
                .And(Breakpoint.Desktop, Size.Fraction(0.5f));

        // Act
        var sheet = MakeSheet().Side(SheetSide.Bottom).Height(responsiveHeight).Resizable();

        // Assert: height ramp preserved with vertical min/max defaults (100 / 900).
        Assert.Equal("Full,Px:100,Px:900", sheet.Height!.Mobile!.ToString());
        Assert.Equal("Fraction:0.5,Px:100,Px:900", sheet.Height.Desktop!.ToString());
    }
}
