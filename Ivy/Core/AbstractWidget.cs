using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ivy.Core.Helpers;

namespace Ivy.Core;

/// <summary>
/// Abstract base class for all widgets in the Ivy Framework. Provides core functionality for widget serialization,
/// event handling, attached properties, and child widget management.
/// </summary>
public abstract record AbstractWidget : IWidget
{
    private string? _id;
    private readonly Dictionary<(Type, string), object?> _attachedProps = new();

    /// <summary>
    /// Initializes a new instance of the AbstractWidget class with the specified child widgets.
    /// </summary>
    /// <param name="children">The child widgets to be contained within this widget.</param>
    protected AbstractWidget(params object[] children)
    {
        Children = children;
    }

    /// <summary>
    /// Sets an attached property value for this widget. Attached properties are used by parent widgets
    /// to store layout or behavior information on their child widgets.
    /// </summary>
    /// <param name="parentType">The type of the parent widget that defines the attached property.</param>
    /// <param name="name">The name of the attached property.</param>
    /// <param name="value">The value to set for the attached property.</param>
    public void SetAttachedValue(Type parentType, string name, object? value)
    {
        _attachedProps[(parentType, name)] = value;
    }

    /// <summary>
    /// Gets an attached property value for this widget. Returns null if the attached property has not been set.
    /// </summary>
    /// <param name="t">The type of the parent widget that defines the attached property.</param>
    /// <param name="name">The name of the attached property to retrieve.</param>
    /// <returns>The value of the attached property, or null if not set.</returns>
    public object? GetAttachedValue(Type t, string name)
    {
        return _attachedProps.GetValueOrDefault((t, name));
    }

    /// <summary>
    /// Gets or sets the unique identifier for this widget instance. The Id is required for widget serialization
    /// and must be set before accessing this property.
    /// </summary>
    /// <value>The unique identifier of the widget.</value>
    /// <exception cref="InvalidOperationException">Thrown when trying to access an uninitialized Id.</exception>
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

    /// <summary>
    /// Gets or sets an optional key for this widget. Keys are used for widget identification and optimization
    /// during rendering and updates.
    /// </summary>
    /// <value>The key of the widget, or null if no key is set.</value>
    public string? Key { get; set; }

    /// <summary>
    /// Gets or sets the child widgets contained within this widget.
    /// </summary>
    /// <value>An array of child objects, which should be widgets for proper serialization.</value>
    public object[] Children { get; set; }

    /// <summary>
    /// Serializes this widget and its children to a JSON representation. This method processes all properties
    /// marked with <see cref="PropAttribute"/> and events marked with <see cref="EventAttribute"/>.
    /// </summary>
    /// <returns>A JSON node representing the serialized widget.</returns>
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

    /// <summary>
    /// Gets the value of a property for serialization. Handles both regular properties and attached properties.
    /// For attached properties, collects values from all child widgets.
    /// </summary>
    /// <param name="property">The property to get the value for.</param>
    /// <returns>The property value, or an array of values for attached properties.</returns>
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

    /// <summary>
    /// Invokes an event handler on this widget with the specified arguments. Supports both parameterless
    /// and parameterized event handlers using the Event&lt;T&gt; and Event&lt;T, TValue&gt; patterns.
    /// </summary>
    /// <param name="eventName">The name of the event to invoke.</param>
    /// <param name="args">The arguments to pass to the event handler.</param>
    /// <returns>true if the event was successfully invoked; otherwise, false.</returns>
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

    /// <summary>
    /// Determines if the given object is a valid Func delegate and extracts the event type and return type.
    /// </summary>
    /// <param name="eventDelegate">The delegate to check.</param>
    /// <param name="eventType">When this method returns, contains the event type if the delegate is valid.</param>
    /// <param name="returnType">When this method returns, contains the return type of the delegate.</param>
    /// <returns>true if the delegate is a valid Func with a single parameter; otherwise, false.</returns>
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

    /// <summary>
    /// Overloads the | operator to provide a convenient syntax for adding children to a widget.
    /// Supports adding single objects, arrays, or enumerables of children.
    /// </summary>
    /// <param name="widget">The widget to add children to.</param>
    /// <param name="child">The child object, array, or enumerable to add.</param>
    /// <returns>A new widget instance with the additional children.</returns>
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

