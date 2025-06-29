using System.Text;
using System.Text.Json.Serialization;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Shared;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record FileInput
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public int Size { get; init; }
    public DateTime LastModified { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public byte[]? Content { get; init; }
}

public enum FileInputs
{
    Drop
}

public interface IAnyFileInput : IAnyInput
{
    public string? Placeholder { get; set; }
    public FileInputs Variant { get; set; }
}

public abstract record FileInputBase : WidgetBase<FileInputBase>, IAnyFileInput
{
    [Prop] public bool Disabled { get; set; }
    [Prop] public string? Invalid { get; set; }
    [Prop] public string? Placeholder { get; set; }
    [Prop] public FileInputs Variant { get; set; }
    [Prop] public string? Accept { get; set; }
    [Prop] public bool Multiple { get; set; }
    [Event] public Action<Event<IAnyInput>>? OnBlur { get; set; }
    public Type[] SupportedStateTypes() => [];
}

public record FileInput<TValue> : FileInputBase, IInput<TValue>, IAnyFileInput
{
    public FileInput(IAnyState state, string? placeholder = null, bool disabled = false, FileInputs variant = FileInputs.Drop)
        : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TValue>();
        Value = typedState.Value;
        OnChange = e => typedState.Set(e.Value);
    }

    public FileInput(TValue value, Action<Event<IInput<TValue>, TValue>>? onChange, string? placeholder = null, bool disabled = false, FileInputs variant = FileInputs.Drop)
        : this(placeholder, disabled, variant)
    {
        OnChange = onChange;
        Value = value;
    }

    public FileInput(string? placeholder = null, bool disabled = false, FileInputs variant = FileInputs.Drop)
    {
        Placeholder = placeholder;
        Variant = variant;
        Disabled = disabled;
        Width = Size.Full();
        Height = Size.Units(50);
    }

    [Prop] public TValue Value { get; } = default!;

    [Event] public Action<Event<IInput<TValue>, TValue>>? OnChange { get; }
}

public static class FileInputExtensions
{
    public static string? ToPlainText(this FileInput file)
    {
        if (file.Content == null)
        {
            return null;
        }
        return file.Content.Length switch
        {
            0 => null,
            _ => Encoding.UTF8.GetString(file.Content)
        };
    }

    public static FileInputBase ToFileInput(this IAnyState state, string? placeholder = null, bool disabled = false, FileInputs variant = FileInputs.Drop)
    {
        var type = state.GetStateType();

        //Check that type is FileInput, FileInput? or IEnumerable<FileInput>
        var isValid = type == typeof(FileInput)
                      || (type.IsGenericType &&
                          type.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                          type.GetGenericArguments()[0] == typeof(FileInput));

        if (!isValid)
        {
            throw new Exception("Invalid type for FileInput");
        }

        Type genericType = typeof(FileInput<>).MakeGenericType(type);
        FileInputBase input = (FileInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled, variant)!;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            input = input.Multiple();
        }

        return input;
    }

    public static FileInputBase Placeholder(this FileInputBase widget, string title)
    {
        return widget with { Placeholder = title };
    }

    public static FileInputBase Disabled(this FileInputBase widget, bool disabled = true)
    {
        return widget with { Disabled = disabled };
    }

    public static FileInputBase Variant(this FileInputBase widget, FileInputs variant)
    {
        return widget with { Variant = variant };
    }

    public static FileInputBase Invalid(this FileInputBase widget, string? invalid)
    {
        return widget with { Invalid = invalid };
    }

    public static FileInputBase Accept(this FileInputBase widget, string accept)
    {
        return widget with { Accept = accept };
    }

    public static FileInputBase Multiple(this FileInputBase widget, bool multiple = true)
    {
        return widget with { Multiple = multiple };
    }
}