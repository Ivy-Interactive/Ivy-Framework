using Ivy.Plugins;
using static Ivy.Layout;

namespace Ivy.Apps;

public class PluginConfigurationView(string pluginId, PluginConfigurationSchema schema, IIvyPluginConfigFactory configFactory) : ViewBase
{
    public override object? Build()
    {
        var config = configFactory.Create(pluginId);
        var fields = schema.Fields;
        var states = fields.Select(f =>
            UseState(config.GetValue(f.Key) ?? f.DefaultValue ?? "")
        ).ToArray();
        var statusMessage = UseState<string?>(null);

        var fieldWidgets = fields.Select((field, i) =>
        {
            var state = states[i];
            var input = BuildInputForField(field, state);
            return (object)new Field(input, label: field.Key, description: field.Description, required: field.IsRequired);
        }).ToArray();

        return Vertical().Gap(4)
            | fieldWidgets
            | (Horizontal().Gap(2)
                | new Button("Save", onClick: _ =>
                {
                    for (var i = 0; i < fields.Count; i++)
                    {
                        var value = states[i].Value;
                        if (!string.IsNullOrEmpty(value))
                            config.SetValue(fields[i].Key, value);
                        else
                            config.RemoveValue(fields[i].Key);
                    }
                    config.Save();
                    statusMessage.Set("Configuration saved.");
                    return ValueTask.CompletedTask;
                }, icon: Icons.Save))
            | (statusMessage.Value is not null
                ? new Badge(statusMessage.Value, BadgeVariant.Success)
                : null);
    }

    private static IAnyInput BuildInputForField(ConfigFieldDefinition field, IState<string> state)
    {
        return field.Type switch
        {
            ConfigFieldType.Boolean => state.ToSelectInput(["true", "false"], placeholder: "Select..."),
            ConfigFieldType.Secret => state.ToTextInput(placeholder: field.Description ?? field.Key, variant: TextInputVariant.Password),
            _ => state.ToTextInput(placeholder: field.Description ?? field.Key),
        };
    }
}
