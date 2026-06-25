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
        // Arrange: a listener whose handler captures the breakpoint the frontend reports.
        Breakpoint? captured = null;
        var widget = new BreakpointListener
        {
            OnChange = new(e =>
            {
                captured = e.Value;
                return ValueTask.CompletedTask;
            })
        };

        // Act: simulate the frontend firing OnChange with the PascalCase enum name it sends on the wire.
        var args = new JsonArray { JsonValue.Create(wireName) };
        var handled = await widget.InvokeEventAsync("OnChange", args);

        // Assert: the event resolves to the matching Breakpoint value.
        Assert.True(handled);
        Assert.Equal(expected, captured);
    }
}
