using Ivy.Tendril.Apps.Setup;

namespace Ivy.Tendril.Apps;

[App(title: "Setup", icon: Icons.Construction, isVisible: false)]
public class SetupApp : ViewBase
{
    public override object Build()
    {
        var selectedTab = UseState(0);

        return new TabsLayout(
            e => selectedTab.Set(e.Value),
            null,
            null,
            null,
            selectedTab.Value,
            new Tab("General", new GeneralSetupView()),
            new Tab("Levels", new LevelsSetupView()),
            new Tab("Verifications", new VerificationsSetupView()),
            new Tab("Promptwares", new PromptwaresSetupView()),
            new Tab("Projects", new ProjectsSetupView()),
            new Tab("Advanced", new AdvancedSetupView())
        ).Variant(TabsVariant.Content);
    }
}
