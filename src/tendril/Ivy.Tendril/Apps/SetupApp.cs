using Ivy.Tendril.Apps.Settings;

namespace Ivy.Tendril.Apps;

[App(title: "Setup", icon: Icons.Construction, isVisible: false)]
public class SetupApp : ViewBase
{
    public override object? Build()
    {
        var selectedTab = UseState(0);

        return new TabsLayout(
            e => selectedTab.Set(e.Value),
            null,
            null,
            null,
            selectedTab.Value,
            new Tab("General", new GeneralSettingsView()),
            new Tab("Levels", new LevelsSettingsView()),
            new Tab("Verifications", new VerificationsSettingsView()),
            new Tab("Projects", new ProjectsSettingsView())
        ).Variant(TabsVariant.Content);
    }
}