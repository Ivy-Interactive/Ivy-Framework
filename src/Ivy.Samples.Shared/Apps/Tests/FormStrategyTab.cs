namespace Ivy.Samples.Shared.Apps.Tests;

public class FormStrategyTab : ViewBase
{
    private readonly FormSubmitStrategy _strategy;

    public FormStrategyTab(FormSubmitStrategy strategy)
    {
        _strategy = strategy;
    }

    public override object? Build()
    {
        var settings = UseState(() => new SettingsModel("Default", "Light", 14));
        var client = UseService<IClientProvider>();
        var submitCount = UseState(0);

        UseEffect(() =>
        {
            if (!string.IsNullOrEmpty(settings.Value.Name))
            {
                submitCount.Set(submitCount.Value + 1);
                client.Toast($"Settings saved! (#{submitCount.Value})");
            }
        }, settings);

        return Layout.Vertical().Gap(4)
            | Text.Block($"Strategy: {_strategy}")
            | settings.ToForm()
                .SubmitStrategy(_strategy)
                .Label(m => m.Name, "Display Name")
                .Label(m => m.Theme, "Theme")
                .Label(m => m.FontSize, "Font Size")
            | Text.Block($"Current: {settings.Value.Name}, {settings.Value.Theme}, {settings.Value.FontSize}px — Submits: {submitCount.Value}");
    }
}

public class DynamicFormStrategyTab : ViewBase
{
    public override object? Build()
    {
        var settings = UseState(() => new SettingsModel("Default", "Light", 14));
        var strategy = UseState(FormSubmitStrategy.OnSubmit);
        var client = UseService<IClientProvider>();
        var submitCount = UseState(0);

        UseEffect(() =>
        {
            if (!string.IsNullOrEmpty(settings.Value.Name))
            {
                submitCount.Set(submitCount.Value + 1);
                client.Toast($"Settings saved! (#{submitCount.Value})");
            }
        }, settings);

        var strategyOptions = Enum.GetValues<FormSubmitStrategy>().ToOptions();

        return Layout.Vertical().Gap(4)
            | Layout.Horizontal()
                | Text.Block("Submit Strategy:")
                | new SelectInput<FormSubmitStrategy>(strategy.Value, e => strategy.Set(e.Value), strategyOptions)
            | settings.ToForm()
                .SubmitStrategy(strategy.Value)
                .Label(m => m.Name, "Display Name")
                .Label(m => m.Theme, "Theme")
                .Label(m => m.FontSize, "Font Size")
            | Text.Block($"Current: {settings.Value.Name}, {settings.Value.Theme}, {settings.Value.FontSize}px — Submits: {submitCount.Value}");
    }
}
