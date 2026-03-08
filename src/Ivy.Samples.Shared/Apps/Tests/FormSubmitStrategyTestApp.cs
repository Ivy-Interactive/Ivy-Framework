namespace Ivy.Samples.Shared.Apps.Tests;

public record SettingsModel(string Name, string Theme, int FontSize);

[App(icon: Icons.Settings, path: ["Tests"], isVisible: true)]
public class FormSubmitStrategyTestApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("OnSubmit", new FormStrategyTab(FormSubmitStrategy.OnSubmit)),
            new Tab("OnBlur", new FormStrategyTab(FormSubmitStrategy.OnBlur)),
            new Tab("OnChange", new FormStrategyTab(FormSubmitStrategy.OnChange)),
            new Tab("Dynamic", new DynamicFormStrategyTab())
        ).Variant(TabsVariant.Content);
    }
}
