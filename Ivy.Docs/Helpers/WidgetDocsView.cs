using System.Reflection;
using Ivy.Tables;
using Newtonsoft.Json;

namespace Ivy.Docs.Helpers;

public class WidgetDocsView(string typeName, string? extensionsTypeName, string? sourceUrl) : ViewBase
{
    public override object? Build()
    {
        var type = TypeUtils.GetTypeFromName(typeName);
        Type[] extensionTypes =
        [
            ..extensionsTypeName.Split(';').Select(TypeUtils.GetTypeFromName).ToArray()!,
            typeof(WidgetBaseExtensions)
        ];

        if (type == null) return Text.Danger($"WidgetDocsView:Unable to find type {typeName}.");

        object? defaultValueProvider = TypeUtils.TryToActivate(type);

        object? constructorSection = null;

        SignatureRecord[] constructors =
        [
            ..type.GetConstructors()
                .Select(TypeUtils.GetSignatureRecord)
                .ToList(),
            ..extensionTypes.SelectMany(t => t.GetMethods())
                .Where(m => m.Name.StartsWith("To") && m.GetParameters().FirstOrDefault()?.ParameterType == typeof(IAnyState))
                .Select(TypeUtils.GetSignatureRecord)
                .ToList()
        ];

        if (constructors.Any())
        {
            constructorSection = Layout.Vertical().Gap(8)
                                 | Text.H3("Constructors")
                                 | constructors.ToTable().Width(Size.Full());
        }

        object? supportedTypesSection = null;
        if (defaultValueProvider is IAnyInput anyInput)
        {
            var allowedStates = anyInput.SupportedStateTypes().Select(TypeUtils.GetSupportedTypeRecord).ToList();
            if (allowedStates.Any())
            {
                supportedTypesSection = Layout.Vertical().Gap(8)
                               | Text.H3("Supported Types")
                               | allowedStates.ToTable();
            }
        }


        var properties = type.GetProperties()
            .Where(p => p.GetCustomAttribute<PropAttribute>() != null)
            .Select(e => TypeUtils.GetPropRecord(e, defaultValueProvider, type, extensionTypes))
            .OrderBy(e => e.Name);

        var propertySection = Layout.Vertical().Gap(8)
                              | Text.H3("Properties")
                              | properties.ToTable().Width(Size.Full())
            ;

        var events = type.GetProperties()
            .Where(p => p.GetCustomAttribute<EventAttribute>() != null)
            .Select(e => TypeUtils.GetEventRecord(e, type, extensionTypes))
            .OrderBy(e => e.Name)
            .ToList();

        object? eventSection = null;

        if (events.Any())
        {
            eventSection = Layout.Vertical().Gap(8)
                           | Text.H3("Events")
                           | events.ToTable().Width(Size.Full())
                ;
        }

        string? fileName = sourceUrl != null ? System.IO.Path.GetFileName(sourceUrl) : null;

        return Layout.Vertical().Gap(8)
               | Text.H2("API")
               | (fileName != null
                   ? (Layout.Horizontal().Align(Align.Left).Gap(0) | Icons.Github.ToIcon() |
                      new Button(fileName).Link().Url(sourceUrl))
                   : null)
               | constructorSection
               | supportedTypesSection
               | propertySection
               | eventSection
            ;
    }
}