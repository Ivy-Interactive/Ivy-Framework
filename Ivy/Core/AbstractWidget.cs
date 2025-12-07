using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ivy.Core.Helpers;
using Ivy.Widgets.Inputs;

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
                new ValueTupleConverterFactory(),
                new PrefixSuffixJsonConverterFactory()
            }
        };

        var props = new JsonObject();
        foreach (var property in propProperties)
        {
            var value = GetPropertyValue(property);
            if (value == null) //small optimization to avoid serializing null values 
                continue;
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

    public async Task<bool> InvokeEventAsync(string eventName, JsonArray args)
    {
        var type = GetType();
        var property = type.GetProperty(eventName);

        if (property == null)
            return false;

        var eventDelegate = property.GetValue(this);

        if (eventDelegate == null)
            return false;

        if (IsFunc(eventDelegate, out Type? eventType, out Type? returnType) && returnType == typeof(ValueTask))
        {
            var eventInstance = eventType!.IsGenericType switch
            {
                true when eventType.GetGenericTypeDefinition() == typeof(Event<>) =>
                    Activator.CreateInstance(eventType, eventName, this),
                true when eventType.GetGenericTypeDefinition() == typeof(Event<,>) =>
                    CreateEventWithValue(eventType, eventName, this, args),
                _ => null
            };

            if (eventInstance == null) return false;

            // Invoke the event handler
            var result = ((Delegate)eventDelegate).DynamicInvoke(eventInstance);
            if (result is ValueTask valueTask)
            {
                // Properly await the async event handler instead of blocking
                await valueTask;
            }
            return true;
        }

        return false;
    }

    private static object? CreateEventWithValue(Type eventType, string eventName, object sender, JsonArray args)
    {
        var valueType = eventType.GetGenericArguments()[1];
        var value = ConvertToValue(valueType, args);
        // Create the event even if value is null - null is a valid value for nullable types
        return Activator.CreateInstance(eventType, eventName, sender, value);
    }

    private static object? ConvertToValue(Type valueType, JsonArray args)
    {
        // Handle tuples with multiple arguments
        if (IsValueTuple(valueType) && args.Count() > 1)
        {
            var tupleTypes = valueType.GetGenericArguments();
            if (args.Count() == tupleTypes.Length)
            {
                var tupleArgs = new object[tupleTypes.Length];
                for (int i = 0; i < tupleTypes.Length; i++)
                {
                    tupleArgs[i] = Utils.ConvertJsonNode(args[i], tupleTypes[i])!;
                }
                return Activator.CreateInstance(valueType, tupleArgs);
            }
            return null;
        }

        // Handle single argument
        if (args.Count == 1)
        {
            return Utils.ConvertJsonNode(args[0], valueType);
        }

        return null;
    }

    private static bool IsValueTuple(Type t) =>
        t is { IsValueType: true, IsGenericType: true } && t.FullName?.StartsWith("System.ValueTuple") == true;
    private static bool IsFunc(object eventDelegate, out Type? eventType, out Type? returnType)
    {
        eventType = null;
        returnType = null;

        var delegateType = eventDelegate.GetType();

        if (!typeof(Delegate).IsAssignableFrom(delegateType))
            return false;

        var invokeMethod = delegateType.GetMethod("Invoke");

        var parameters = invokeMethod!.GetParameters();
        if (parameters.Length != 1)
            return false;

        eventType = parameters[0].ParameterType;
        returnType = invokeMethod.ReturnType;

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
