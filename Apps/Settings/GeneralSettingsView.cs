using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Settings;

public class GeneralSettingsView : ViewBase
{
    public override object? Build()
    {
        var config = UseService<ConfigService>();
        var client = UseService<IClientProvider>();
        var agentCommand = UseState(config.Settings.AgentCommand);

        var form = Layout.Vertical().Gap(4).Padding(4).Width(Size.Auto().Max(Size.Units(120)))
            | Text.Block("General Settings").Bold()
            | agentCommand.ToTextInput("Agent command...")
                .WithField().Label("Agent Command")
            | Layout.Horizontal().Gap(2)
                | new Button("Save").Primary().OnClick(() =>
                {
                    config.Settings.AgentCommand = agentCommand.Value;
                    config.SaveSettings();
                    client.Toast("Settings saved successfully", "Saved");
                })
                | new Button("Reset").Outline().OnClick(() =>
                {
                    agentCommand.Set(config.Settings.AgentCommand);
                });

        return form;
    }
}
