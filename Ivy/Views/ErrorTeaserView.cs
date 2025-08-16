using Ivy.Core;
using Ivy.Helpers;
using Ivy.Shared;

namespace Ivy.Views;

public class ErrorTeaserView(Exception ex) : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
               | Text.Muted(ex.Message)
               | new Button("Read More").Variant(ButtonVariant.Primary).WithSheet(() => new ErrorView(ex), width: Size.Half());
    }
}