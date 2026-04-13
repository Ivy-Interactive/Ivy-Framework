using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

public class ConfigErrorApp(IConfigService config) : ViewBase
{
    public override object Build()
    {
        var showDetails = UseState(false);
        var client = UseService<IClientProvider>();
        var parseError = config.ParseError;

        if (parseError == null)
        {
            client.Redirect("/", true);
            return Text.P("Redirecting...");
        }

        var errorSummary = parseError.Message.Length > 200
            ? parseError.Message[..200] + "..."
            : parseError.Message;

        var content = Layout.Vertical().Gap(4);

        content |= new Image("/tendril/assets/Tendril.svg").Width(Size.Units(20)).Height(Size.Auto());

        content |= Callout.Error(
            $"The configuration file could not be parsed:\n\n`{errorSummary}`\n\nA backup of the broken file has been saved.",
            "Configuration Error"
        );

        content |= Layout.Horizontal().Gap(2)
                   | new Button("Edit Config")
                       .Icon(Icons.FileText)
                       .OnClick(() => config.OpenInEditor(config.ConfigPath))
                   | new Button("Reload Config")
                       .Icon(Icons.RefreshCw)
                       .Variant(ButtonVariant.Outline)
                       .OnClick(() =>
                       {
                           config.RetryLoadConfig();
                           if (config.ParseError == null)
                               client.Redirect("/", true);
                       })
                   | new Button("Reset to Defaults")
                       .Icon(Icons.RotateCcw)
                       .Variant(ButtonVariant.Destructive)
                       .OnClick(() =>
                       {
                           config.ResetToDefaults();
                           client.Redirect("/", true);
                       })
                   | new Button(showDetails.Value ? "Hide Details" : "View Details")
                       .Icon(Icons.Info)
                       .Variant(ButtonVariant.Ghost)
                       .OnClick(() => showDetails.Set(!showDetails.Value));

        if (showDetails.Value)
        {
            var details = $"**File:** `{parseError.FilePath}`\n\n" +
                          $"**Error:**\n```\n{parseError.Message}\n```";
            if (parseError.InnerException != null)
                details += $"\n\n**Stack Trace:**\n```\n{parseError.InnerException.StackTrace}\n```";

            content |= new Callout(details, "Error Details");
        }

        content |= Text.Muted("Broken config files are backed up with a `.broken.{timestamp}.bak` extension.").Small();

        return Layout.TopCenter()
               | (content.Margin(0, 20).Width(150));
    }
}
