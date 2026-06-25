using System.Text.Json.Nodes;

namespace Ivy.Test.Widgets;

public class BreakpointListenerTests
{
    [Fact]
    public void OnChange_Event_IsExposedOnWidget()
    {
        var widget = new BreakpointListener
        {
            OnChange = new(_ => ValueTask.CompletedTask)
        };

        Assert.NotNull(widget.OnChange);
    }

    [Theory]
    [InlineData("Mobile", Breakpoint.Mobile)]
    [InlineData("Tablet", Breakpoint.Tablet)]
    [InlineData("Desktop", Breakpoint.Desktop)]
    [InlineData("Wide", Breakpoint.Wide)]
    public async Task OnChange_RoundTripsBreakpointFromFrontendName(string wireName, Breakpoint expected)
    {
        Breakpoint? captured = null;
        var widget = new BreakpointListener
        {
            OnChange = new(e =>
            {
                captured = e.Value;
                return ValueTask.CompletedTask;
            })
        };

        var args = new JsonArray { JsonValue.Create(wireName) };
        var handled = await widget.InvokeEventAsync("OnChange", args);

        Assert.True(handled);
        Assert.Equal(expected, captured);
    }
}
