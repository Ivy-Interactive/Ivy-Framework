using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets;

[App(icon: Icons.Menu)]
public class DropDownMenuApp : SampleBase
{
    protected override object? BuildSample()
    {
        var client = this.UseService<IClientProvider>();

        var trigger = new Button("Open Menu");

        Action<Event<DropDownMenu, object>> onSelect = @evt =>
        {
            client.Toast(@evt.Value?.ToString() ?? throw new Exception("Missing value in event."));
        };

        return new DropDownMenu(onSelect, trigger)
                   .Header(
                       Layout.Vertical().Gap(0) | Text.Muted("Signed in as") | Text.Small("niels@bosmainteractive.se")
                   )
                   .Bottom()
               | MenuItem.Separator()
               | MenuItem.Default("Ivy Github").Icon(Icons.Github)
               | MenuItem.Default("Theme")
                   .Icon(Icons.SunMoon)
                   .Children(
                       MenuItem.Checkbox("Dark").Checked().Icon(Icons.Moon),
                       MenuItem.Checkbox("Light").Icon(Icons.Sun),
                       MenuItem.Checkbox("System").Icon(Icons.SunMoon)
                   )
               | MenuItem.Default("Logout").Icon(Icons.LogOut);
    }
}