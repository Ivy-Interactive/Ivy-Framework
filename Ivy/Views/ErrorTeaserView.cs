using Ivy.Core;
using Ivy.Helpers;
using Ivy.Shared;

namespace Ivy.Views;

/// <summary>
/// Represents an error teaser view that displays a brief error message with
/// an option to view full error details.
/// </summary>
public class ErrorTeaserView(Exception ex) : ViewBase
{
    /// <summary>
    /// Builds the error teaser view layout, displaying the error message
    /// and a "Read More" button that opens a detailed error view in a sheet.
    /// </summary>
    /// <returns>A vertical layout containing the error message and a button
    /// to access full error details.</returns>
    public override object? Build()
    {
        ex = ex.UnwrapAggregate();

        return Layout.Vertical()
               | Text.Muted(ex.Message)
               | new Button("Read More").Variant(ButtonVariant.Primary).WithSheet(() => new ErrorView(ex), width: Size.Half());
    }
}