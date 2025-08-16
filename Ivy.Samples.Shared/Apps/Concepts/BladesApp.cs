using Ivy.Shared;
using Ivy.Views.Blades;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.PanelLeft)]
public class BladesApp : SampleBase
{
    protected override object? BuildSample()
    {
        return this.UseBlades(() => new RootView("A"), "Blade 0");
    }
}

public class RootView(string someId) : ViewBase
{
    public override object? Build()
    {
        var bladeController = this.UseContext<IBladeController>();
        var index = bladeController.GetIndex(this);

        void OnClick(Event<Button> @event)
        {
            bladeController.Push(this, new RootView(@event.Sender.Tag?.ToString() ?? "?"), $"Blade {index + 1}");
        }

        void OnClickWithError(Event<Button> @event)
        {
            bladeController.Push(this, new BladeWithError(), "Blade With Error");
        }

        return Layout.Vertical(
            $"This is blade {index}",
            DateTime.Now.Ticks,
            someId,
            new Button("Push A", OnClick).Tag("A"),
            new Button("Push B", OnClick).Tag("B"),
            new Button("Push C", OnClick).Tag("C"),
            new Button("Blade With Error", OnClickWithError)
        );
    }
}

public class BladeWithError : ViewBase
{
    public override object? Build()
    {
        throw new InvalidOperationException("This is a test error in a blade.");
    }
}