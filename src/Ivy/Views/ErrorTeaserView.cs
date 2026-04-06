// ReSharper disable once CheckNamespace
namespace Ivy;

public class ErrorTeaserView(Exception ex) : ViewBase
{
    public override object? Build()
    {
        ex = ex.UnwrapAggregate();

        return Layout.Vertical()
               | Text.Muted(ex.Message)
               | new Button("Read More").Variant(ButtonVariant.Primary).WithSheet(() => new ErrorView(ex), width: Size.Half());
    }
}
