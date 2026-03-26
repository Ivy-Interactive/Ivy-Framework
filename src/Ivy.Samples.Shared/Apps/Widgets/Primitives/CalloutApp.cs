
namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.CloudAlert, group: ["Widgets", "Primitives"], searchHints: ["alert", "notice", "warning", "info", "banner", "message"])]
public class CalloutApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("Variants", new CalloutVariantsExample()),
            new Tab("Closable", new CalloutClosableExample()),
            new Tab("Multi-line", new CalloutMultiLineExample()),
            new Tab("Density", new CalloutDensityExample())
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

public class CalloutMultiLineExample : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(6)
            | new Callout(
                "This is a callout with a lot of text. It is designed to test the vertical alignment of the icon when the description spans multiple lines. " +
                "According to the reported issue, the icon should be top-aligned with the first line of text rather than being centered vertically within the entire callout box. " +
                "This ensures the icon remains consistent and easy to find, especially in long alerts or hints.",
                "Hint: BadImageFormatException",
                CalloutVariant.Info
            )
            | new Callout(
                "Another multi-line callout without a title. The icon should be aligned with the first line of this description text. " +
                "This helps maintain visual balance even when the content is extensive.",
                variant: CalloutVariant.Warning
            );
    }
}

public class CalloutDensityExample : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(6)
            | Text.H3("Small Density (20px Icon)")
            | Layout.Vertical().Gap(2)
                | Callout.Info("Small density with a title. Everything should be tight and perfectly aligned.", "Small Info").Small()
                | Callout.Warning("Small density without a title. Icon center should match the first line center.").Small()
            | Text.H3("Medium Density (Default - 24px Icon)")
            | Layout.Vertical().Gap(2)
                | Callout.Info("Medium density with a title. The standard look for alerts and hints.", "Medium Info").Medium()
                | Callout.Warning("Medium density without a title. Balanced and professional alignment.").Medium()
            | Text.H3("Large Density (28px Icon)")
            | Layout.Vertical().Gap(2)
                | Callout.Info("Large density for high-impact zones. Larger text and more generous padding.", "Large Info").Large()
                | Callout.Warning("Large density without a title. High visibility with perfect baseline balance.").Large();
    }
}
