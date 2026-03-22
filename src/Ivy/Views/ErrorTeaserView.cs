using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class ErrorTeaserView(Exception ex) : ViewBase
{
    public override object? Build()
    {
        ex = ex.UnwrapAggregate();

        return Layout.Vertical()
               | Text.Muted(ex.Message)
               | new Baton("Read More").Variant(BatonVariant.Primary).WithSheet(() => new ErrorView(ex), width: Size.Half());
    }
}
