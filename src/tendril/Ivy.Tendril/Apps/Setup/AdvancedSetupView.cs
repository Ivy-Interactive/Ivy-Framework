using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Setup;

public class AdvancedSetupView : ViewBase
{
    public override object Build()
    {
        var config = UseService<IConfigService>();
        var client = UseService<IClientProvider>();

        var jobTimeout = UseState(config.Settings.JobTimeout);
        var staleOutputTimeout = UseState(config.Settings.StaleOutputTimeout);
        var maxConcurrentJobs = UseState(config.Settings.MaxConcurrentJobs);
        var editorCommand = UseState(config.Settings.Editor.Command);
        var editorLabel = UseState(config.Settings.Editor.Label);

        var hasChanges = jobTimeout.Value != config.Settings.JobTimeout
                         || staleOutputTimeout.Value != config.Settings.StaleOutputTimeout
                         || maxConcurrentJobs.Value != config.Settings.MaxConcurrentJobs
                         || editorCommand.Value != config.Settings.Editor.Command
                         || editorLabel.Value != config.Settings.Editor.Label;

        var form = Layout.Vertical().Gap(4).Padding(4).Width(Size.Auto().Max(Size.Units(120)))
                   | Text.Block("Timeouts").Bold()
                   | jobTimeout.ToNumberInput().Min(1).Max(120).Suffix("min")
                       .WithField().Label("Job Timeout")
                   | staleOutputTimeout.ToNumberInput().Min(1).Max(60).Suffix("min")
                       .WithField().Label("Stale Output Timeout")
                   | maxConcurrentJobs.ToNumberInput().Min(1).Max(100)
                       .WithField().Label("Max Concurrent Jobs")
                   | Text.Block("Editor").Bold()
                   | editorCommand.ToTextInput("e.g. code, vim")
                       .WithField().Label("Command")
                   | editorLabel.ToTextInput("e.g. VS Code, Vim")
                       .WithField().Label("Label")
                   | new Button("Save").Primary()
                       .Disabled(!hasChanges)
                       .OnClick(() =>
                       {
                           config.Settings.JobTimeout = jobTimeout.Value;
                           config.Settings.StaleOutputTimeout = staleOutputTimeout.Value;
                           config.Settings.MaxConcurrentJobs = maxConcurrentJobs.Value;
                           config.Settings.Editor.Command = editorCommand.Value;
                           config.Settings.Editor.Label = editorLabel.Value;
                           config.SaveSettings();
                           client.Toast("Settings saved and applied", "Saved");
                       });

        return form;
    }
}
