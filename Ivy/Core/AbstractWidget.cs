using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ivy.Core.Helpers;

namespace Ivy.Core;

public abstract record AbstractWidget : IWidget
{
    private string? _id;
    private readonly Dictionary<(Type, string), object?> _attachedProps = new();

    protected AbstractWidget(params object[] children)
    {
        Children = children;
    }

    public void SetAttachedValue(Type parentType, string name, object? value)
    {
        _attachedProps[(parentType, name)] = value;
    }

    public object? GetAttachedValue(Type t, string name)
    {
        return _attachedProps.GetValueOrDefault((t, name));
    }

    public string? Id
    {
        get
        {
            if (_id == null)
            {
                throw new InvalidOperationException($"Trying to access an uninitialized WidgetBase Id in a {this.GetType().FullName} widget.");
            }
            return _id;
        }
        set => _id = value;
    }

    public string? Key { get; set; }

    public object[] Children { get; set; }

    public JsonNode Serialize()
    {
        if (Children.Any(e => e is not IWidget))
        {
            throw new InvalidOperationException("Only widgets can be serialized.");
        }

        var type = GetType();

        var json = new JsonObject
        {
            ["id"] = Id,
            ["type"] = type.Namespace + "." + Utils.CleanGenericNotation(type.Name),
            ["children"] = new JsonArray(Children.Cast<IWidget>().Select(c => c.Serialize()).ToArray())
        };

        var propProperties = GetType().GetProperties().Where(p => p.GetCustomAttribute<PropAttribute>() != null);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonEnumConverter(),
                new ValueTupleConverterFactory()
            }
        };

        var props = new JsonObject();
        foreach (var property in propProperties)
        {
            var value = GetPropertyValue(property);
            if (value == null) //small optimization to avoid serializing null values 
                continue;

            //todo: We need a better way to handle this
            // Skip TestId property if not in development mode
            // if (property.Name == "TestId" && Environment.GetEnvironmentVariable("ENVIRONMENT") != "development")
            //     continue;

            props[Utils.PascalCaseToCamelCase(property.Name)] = JsonNode.Parse(JsonSerializer.Serialize(value, options));
        }
        json["props"] = props;

        List<string> events = [];
        var eventProperties = GetType().GetProperties().Where(p => p.GetCustomAttribute<EventAttribute>() != null);

        foreach (var property in eventProperties)
        {
            //check if the property is not null 
            if (property.GetValue(this) == null)
                continue;
            events.Add(property.Name);
        }

        json["events"] = JsonNode.Parse(JsonSerializer.Serialize(events));

        return json;
    }

    private object? GetPropertyValue(PropertyInfo property)
    {
        var attribute = property.GetCustomAttribute<PropAttribute>()!;
        if (attribute.IsAttached)
        {
            if (!property.PropertyType.IsArray || !property.PropertyType.GetElementType()!.IsGenericType)
                throw new InvalidOperationException("Attached properties must be arrays of nullable types.");

            List<object?> attachedValues = new();
            foreach (var child in Children)
            {
                if (child is not IWidget widget)
                {
                    attachedValues.Add(null);
                }
                else
                {
                    var attachedValue = widget.GetAttachedValue(this.GetType(), attribute.AttachedName!);
                    attachedValues.Add(attachedValue);
                }
            }
            return attachedValues.ToArray();
        }

        var value = property.GetValue(this);
        return value;
    }

    public bool InvokeEvent(string eventName, JsonArray args)
    {
        var type = GetType();
        var property = type.GetProperty(eventName);

        if (property == null)
            return false;

        var eventDelegate = property.GetValue(this);

        if (eventDelegate == null)
            return false;

        if (IsAction(eventDelegate, out Type? eventType))
        {
            if (eventType!.IsGenericType && eventType.GetGenericTypeDefinition() == typeof(Event<>))
            {
                var eventInstance = Activator.CreateInstance(eventType, eventName, this);
                ((Delegate)eventDelegate).DynamicInvoke(eventInstance);
                return true;
            }
            if (eventType!.IsGenericType && eventType.GetGenericTypeDefinition() == typeof(Event<,>))
            {
                var genericArguments = eventType.GetGenericArguments();
                if (genericArguments.Length == 2)
                {
                    if (args.Count() != 1) return false;

                    var valueType = genericArguments[1];
                    var value = Utils.ConvertJsonNode(args[0], valueType);

                    var eventInstance = Activator.CreateInstance(eventType, eventName, this, value);
                    ((Delegate)eventDelegate).DynamicInvoke(eventInstance);
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsAction(object eventDelegate, out Type? eventType)
    {
        eventType = null;

        var delegateType = eventDelegate.GetType();

        if (!typeof(Delegate).IsAssignableFrom(delegateType))
            return false;

        var invokeMethod = delegateType.GetMethod("Invoke");

        var parameters = invokeMethod!.GetParameters();
        if (parameters.Length != 1)
            return false;

        eventType = parameters[0].ParameterType;

        return true;
    }

    public static AbstractWidget operator |(AbstractWidget widget, object child)
    {
        if (child is object[] array)
        {
            return widget with { Children = [.. widget.Children, .. array] };
        }

        if (child is IEnumerable<object> enumerable)
        {
            return widget with { Children = [.. widget.Children, .. enumerable] };
        }

        return widget with { Children = [.. widget.Children, child] };
    }

}

