using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Nodes;
using Ivy.Core.Helpers;

namespace Ivy.Core;

public abstract record AbstractWidget : IWidget
{
    private readonly record struct CachedProperty(PropertyInfo? Value);

    private string? _id;
    private readonly ConcurrentDictionary<(Type, string), object?> _attachedProps = new();
    private static readonly ConcurrentDictionary<(Type, string), CachedProperty> _eventPropertyCache = new();

#if DEBUG
    /// <summary>
    /// Tracks the current view's callsite during Build() execution.
    /// Widgets created during Build() will use this as a fallback if they don't have their own user-code callsite.
    /// </summary>
    internal static readonly AsyncLocal<CallSite?> CurrentViewCallSite = new();
#endif

    protected AbstractWidget(params object[] children)
    {
        Children = children;
#if DEBUG
        // Try to capture the widget's own callsite from the stack trace first
        // This gives us the exact line where the widget was instantiated
        CallSite = CallSite.From(new System.Diagnostics.StackTrace(fNeedFileInfo: true))
                   ?? CurrentViewCallSite.Value;
#endif
    }

    public void SetAttachedValue(Type parentType, string name, object? value)
    {
        _attachedProps[(parentType, name)] = value;
    }

    public object? GetAttachedValue(Type t, string name)
    {
        return _attachedProps.GetValueOrDefault((t, name));
    }

    [ScaffoldColumn(false)]
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

    [ScaffoldColumn(false)]
    public string? Key { get; set; }

    public string? Path { get; set; }

    public CallSite? CallSite { get; set; }

    [ScaffoldColumn(false)]
    public object[] Children { get; set; }

    public JsonNode Serialize() => WidgetSerializer.Serialize(this);

    public sealed override string ToString() => GetType().Name;

    public async Task<bool> InvokeEventAsync(string eventName, JsonArray args)
    {
        var type = GetType();
        var property = _eventPropertyCache.GetOrAdd(
            (type, eventName),
            static key => new CachedProperty(key.Item1.GetProperty(key.Item2))
        ).Value;

        if (property == null)
            return false;

        var eventDelegate = property.GetValue(this);

        if (eventDelegate == null)
            return false;

        // Unwrap EventHandler<T> wrapper to get the underlying Func delegate
        var eventDelegateType = eventDelegate.GetType();
        if (eventDelegateType.IsGenericType && eventDelegateType.GetGenericTypeDefinition() == typeof(EventHandler<>))
        {
            eventDelegate = eventDelegateType.GetProperty("Handler")!.GetValue(eventDelegate);
            if (eventDelegate == null) return false;
        }

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
        if (IsValueTuple(valueType) && args.Count > 1)
        {
            var tupleTypes = valueType.GetGenericArguments();
            if (args.Count == tupleTypes.Length)
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
