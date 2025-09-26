using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ivy.Core.Helpers;

namespace Ivy.Core;

/// <summary>Abstract base class for all widgets providing core functionality for serialization, event handling, attached properties, and child management.</summary>
public abstract record AbstractWidget : IWidget
{
    private string? _id;
    private readonly Dictionary<(Type, string), object?> _attachedProps = new();

    /// <summary>Initializes AbstractWidget with specified child widgets.</summary>
    /// <param name="children">Child widgets to be contained within this widget.</param>
    protected AbstractWidget(params object[] children)
    {
        Children = children;
    }

    /// <summary>Sets attached property value for this widget used by parent widgets to store layout or behavior information.</summary>
    /// <param name="parentType">Type of parent widget defining attached property.</param>
    /// <param name="name">Name of attached property.</param>
    /// <param name="value">Value to set for attached property.</param>
    public void SetAttachedValue(Type parentType, string name, object? value)
    {
        _attachedProps[(parentType, name)] = value;
    }

    /// <summary>Gets attached property value for this widget. Returns null if not set.</summary>
    /// <param name="t">Type of parent widget defining attached property.</param>
    /// <param name="name">Name of attached property to retrieve.</param>
    /// <returns>Value of attached property, or null if not set.</returns>
    public object? GetAttachedValue(Type t, string name)
    {
        return _attachedProps.GetValueOrDefault((t, name));
    }

    /// <summary>Unique identifier for widget instance required for serialization. Must be set before accessing.</summary>
    /// <exception cref="InvalidOperationException">Thrown when trying to access uninitialized Id.</exception>
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

    /// <summary>Optional key for widget identification and optimization during rendering and updates.</summary>
    public string? Key { get; set; }

    /// <summary>Child widgets contained within this widget.</summary>
    public object[] Children { get; set; }

    /// <summary>Serializes widget and children to JSON processing properties marked with <see cref="PropAttribute"/> and events marked with <see cref="EventAttribute"/>.</summary>
    /// <returns>JSON node representing serialized widget.</returns>
    /// <exception cref="InvalidOperationException">Thrown when children contain non-widget objects.</exception>
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

    /// <summary>Gets property value for serialization handling regular and attached properties.</summary>
    /// <param name="property">Property to get value for.</param>
    /// <returns>Property value, or array of values for attached properties.</returns>
    /// <exception cref="InvalidOperationException">Thrown when attached properties are not arrays of nullable types.</exception>
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

    /// <summary>Invokes event handler on widget with specified arguments supporting Event&lt;T&gt; and Event&lt;T, TValue&gt; patterns.</summary>
    /// <param name="eventName">Name of event to invoke.</param>
    /// <param name="args">Arguments to pass to event handler.</param>
    /// <returns>true if event was successfully invoked; otherwise, false.</returns>
    public bool InvokeEvent(string eventName, JsonArray args)
    {
        var type = GetType();
        var property = type.GetProperty(eventName);

        if (property == null)
            return false;

        var eventDelegate = property.GetValue(this);

        if (eventDelegate == null)
            return false;

        if (IsFunc(eventDelegate, out Type? eventType, out Type? returnType))
        {
            if (returnType == typeof(ValueTask))
            {
                if (eventType!.IsGenericType && eventType.GetGenericTypeDefinition() == typeof(Event<>))
                {
                    var eventInstance = Activator.CreateInstance(eventType, eventName, this);
                    var result = ((Delegate)eventDelegate).DynamicInvoke(eventInstance);
                    if (result is ValueTask valueTask)
                    {
                        valueTask.AsTask().GetAwaiter().GetResult();
                    }
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
                        var result = ((Delegate)eventDelegate).DynamicInvoke(eventInstance);
                        if (result is ValueTask valueTask)
                        {
                            valueTask.AsTask().GetAwaiter().GetResult();
                        }
                        return true;
                    }
                }
            }
        }

        return false;
    }
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

    /// <summary>Overloads | operator for convenient syntax to add children supporting single objects, arrays, or enumerables.</summary>
    /// <param name="widget">Widget to add children to.</param>
    /// <param name="child">Child object, array, or enumerable to add.</param>
    /// <returns>New widget instance with additional children.</returns>
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

