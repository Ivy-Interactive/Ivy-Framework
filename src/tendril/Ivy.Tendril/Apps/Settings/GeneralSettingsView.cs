using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Settings;

public class GeneralSettingsView : ViewBase
{
    public override object? Build()
    {
        var config = UseService<IConfigService>();
        var client = UseService<IClientProvider>();
        var agentCommand = UseState(config.Settings.AgentCommand);
        var planTemplate = UseState(config.Settings.PlanTemplate);

        var form = Layout.Vertical().Gap(4).Padding(4).Width(Size.Auto().Max(Size.Units(120)))
            | Text.Block("General Settings").Bold()
            | agentCommand.ToTextInput("Agent command...")
                .WithField().Label("Agent Command")
            | planTemplate.ToCodeInput("Plan template...")
                .Language(Languages.Markdown)
                .Height(Size.Units(40))
                .WithField().Label("Plan Template")
            | new Button("Save").Primary()
                .Disabled(agentCommand.Value == config.Settings.AgentCommand
                          && planTemplate.Value == config.Settings.PlanTemplate)
                .OnClick(() =>
                {
                    config.Settings.AgentCommand = agentCommand.Value;
                    config.Settings.PlanTemplate = planTemplate.Value;
                    config.SaveSettings();
                    client.Toast("Settings saved successfully", "Saved");
                });

        return form;
    }
}
