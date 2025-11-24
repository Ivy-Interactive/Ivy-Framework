using Ivy.Views.Forms;

namespace Ivy.Samples.Shared.Apps.Tests;

public record FormRecord(string Name, int Age);

[App()]
public class FormSizeApp : ViewBase
{
    public override object? Build()
    {
        var formState = UseState(() => new FormRecord("Niels", 42));

        return formState.ToForm().Large();
    }
}