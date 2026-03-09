using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.CloudAlert, path: ["Widgets", "Primitives"], searchHints: ["alert", "notice", "warning", "info", "banner", "message"])]
public class CalloutApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("Variants", new CalloutVariantsExample()),
            new Tab("Closable", new CalloutClosableExample())
        ).Variant(TabsVariant.Content);
    }
}

public class CalloutVariantsExample : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical()
            | new Callout("Info", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc.", CalloutVariant.Info)
            | new Callout("Success", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc.", CalloutVariant.Success)
            | new Callout("Warning", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc.", CalloutVariant.Warning)
            | new Callout("Error", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc.", CalloutVariant.Error)
            | new Callout("Error", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec purus nec nunc.", CalloutVariant.Error, icon: Icons.Bug);
    }
}

public class CalloutClosableExample : ViewBase
{
    public override object? Build()
    {
        var showUpdateBanner = UseState(true);
        var showTrialBanner = UseState(true);

        return Layout.Vertical().Gap(6)
            | Text.H3("Dismissible callouts")
            | Text.P("Callouts with an OnClose handler show a close button. Click it to dismiss and run your logic (e.g. hide the banner).")
            | (showUpdateBanner.Value
                ? Callout.Info("A new version is available. Refresh to update.", "Update Available")
                    .OnClose(() => showUpdateBanner.Set(false))
                : null)
            | (showTrialBanner.Value
                ? Callout.Warning("Your trial expires in 3 days.")
                    .OnClose(() => showTrialBanner.Set(false))
                : null)
            | (showUpdateBanner.Value || showTrialBanner.Value
                ? null
                : Layout.Vertical().Gap(4)
                    | Text.P("All banners dismissed.").Muted()
                    | new Button("Show callouts again", () =>
                        {
                            showUpdateBanner.Set(true);
                            showTrialBanner.Set(true);
                        })
                        .Variant(ButtonVariant.Secondary));
    }
}
