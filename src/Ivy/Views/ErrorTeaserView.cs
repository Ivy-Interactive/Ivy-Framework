using Ivy.Core;
using Ivy.Helpers;
using Ivy.Shared;

namespace Ivy.Views;

/// <summary>
/// A summary view for displaying a brief exception message with a "Read More" button.
/// Opens a detailed `ErrorView` in a sheet when interacted with, suitable for logs or list error states.
/// </summary>
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