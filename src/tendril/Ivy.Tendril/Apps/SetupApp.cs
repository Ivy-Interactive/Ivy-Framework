using Ivy;
using Ivy.Tendril.Apps.Settings;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Settings", icon: Icons.Settings, isVisible: false)]
public class SetupApp : ViewBase
{
    public override object? Build()
    {
        var selectedTab = UseState(0);

        return new TabsLayout(
            onSelect: e => selectedTab.Set(e.Value),
            onClose: null,
            onRefresh: null,
            onReorder: null,
            selectedIndex: selectedTab.Value,
            new Tab("General", new GeneralSettingsView()),
            new Tab("Levels", new LevelsSettingsView()),
            new Tab("Verifications", new VerificationsSettingsView()),
            new Tab("Projects", new ProjectsSettingsView())
        ).Variant(TabsVariant.Content);
    }
}
