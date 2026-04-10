using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Settings;

public class GeneralSettingsView : ViewBase
{
    private static readonly string[] CodingAgentOptions = ["claude", "codex", "gemini"];
    private static readonly string[] EffortOptions = ["low", "medium", "high", "max"];

    public override object Build()
    {
        var config = UseService<IConfigService>();
        var client = UseService<IClientProvider>();
        var codingAgent = UseState(string.IsNullOrWhiteSpace(config.Settings.CodingAgent)
            ? "claude"
            : config.Settings.CodingAgent);
        var defaultEffort = UseState(string.IsNullOrWhiteSpace(config.Settings.DefaultEffort)
            ? "high"
            : config.Settings.DefaultEffort);
        var planTemplate = UseState(config.Settings.PlanTemplate);
        var currentCodingAgent = string.IsNullOrWhiteSpace(config.Settings.CodingAgent)
            ? "claude"
            : config.Settings.CodingAgent;
        var currentDefaultEffort = string.IsNullOrWhiteSpace(config.Settings.DefaultEffort)
            ? "high"
            : config.Settings.DefaultEffort;

        var hasChanges = codingAgent.Value != currentCodingAgent
                          || defaultEffort.Value != currentDefaultEffort
                          || planTemplate.Value != config.Settings.PlanTemplate;

        var form = Layout.Vertical().Gap(4).Padding(4).Width(Size.Auto().Max(Size.Units(120)))
                   | Text.Block("General Settings").Bold()
                   | codingAgent.ToSelectInput(CodingAgentOptions)
                       .WithField().Label("Coding Agent")
                   | defaultEffort.ToSelectInput(EffortOptions)
                       .WithField().Label("Default Effort")
                   | planTemplate.ToCodeInput("Plan template...")
                       .Language(Languages.Markdown)
                       .Height(Size.Units(40))
                       .WithField().Label("Plan Template")
                   | new Button("Save").Primary()
                       .Disabled(!hasChanges)
                       .OnClick(() =>
                       {
                           config.Settings.CodingAgent = codingAgent.Value;
                           config.Settings.DefaultEffort = defaultEffort.Value;
                           config.Settings.PlanTemplate = planTemplate.Value;
                           config.SaveSettings();
                           client.Toast("Settings saved successfully", "Saved");
                       });

        return form;
    }
}
