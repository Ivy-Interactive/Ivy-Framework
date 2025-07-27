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
    [Prop] public int? MaxFiles { get; set; }
    [Event] public Action<Event<IAnyInput>>? OnBlur { get; set; }
    public Type[] SupportedStateTypes() => [];

    /// <summary>
    /// Validates the current value and returns a validation result
    /// </summary>
    /// <param name="value">The current value to validate</param>
    /// <returns>Validation result</returns>
    public ValidationResult ValidateValue(object? value)
    {
        if (value == null) return ValidationResult.Success();

        if (value is FileInput file)
        {
            return FileInputValidation.ValidateFileType(file, Accept);
        }
        else if (value is IEnumerable<FileInput> files)
        {
            var filesList = files.ToList();

            // Validate file count first if MaxFiles is set
            if (MaxFiles.HasValue)
            {
                var countValidation = FileInputValidation.ValidateFileCount(filesList, MaxFiles);
                if (!countValidation.IsValid)
                {
                    return countValidation;
                }
            }

            // Then validate file types if Accept is set
            if (!string.IsNullOrWhiteSpace(Accept))
            {
                return FileInputValidation.ValidateFileTypes(filesList, Accept);
            }
        }

        return ValidationResult.Success();
    }
}

public record FileInput<TValue> : FileInputBase, IInput<TValue>, IAnyFileInput
{
    public FileInput(IAnyState state, string? placeholder = null, bool disabled = false, FileInputs variant = FileInputs.Drop)
        : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TValue>();
        Value = typedState.Value;
        OnChange = e =>
        {
            typedState.Set(e.Value);

            // Auto-validate if Accept or MaxFiles is set
            if (!string.IsNullOrWhiteSpace(Accept) || MaxFiles.HasValue)
            {
                var validation = ValidateValue(e.Value);
                if (!validation.IsValid)
                {
                    Invalid = validation.ErrorMessage;
                }
                else
                {
                    Invalid = null;
                }
            }
        };
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
        var isCollection = type.IsGenericType &&
                          type.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                          type.GetGenericArguments()[0] == typeof(FileInput);
        var isValid = type == typeof(FileInput) || isCollection;

        if (!isValid)
        {
            throw new Exception("Invalid type for FileInput");
        }

        Type genericType = typeof(FileInput<>).MakeGenericType(type);
        FileInputBase input = (FileInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled, variant)!;

        // Set Multiple based on type
        input = input with { Multiple = isCollection };

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

    public static FileInputBase MaxFiles(this FileInputBase widget, int maxFiles)
    {
        if (widget.Multiple != true)
        {
            throw new InvalidOperationException("MaxFiles can only be set on a multi-file input (IEnumerable<FileInput>). Use a collection state type for multiple files.");
        }
        return widget with { MaxFiles = maxFiles };
    }

    /// <summary>
    /// Validates a single file against the widget's accept pattern
    /// </summary>
    /// <param name="widget">The file input widget</param>
    /// <param name="file">The file to validate</param>
    /// <returns>Validation result</returns>
    public static ValidationResult ValidateFile(this FileInputBase widget, FileInput file)
    {
        return FileInputValidation.ValidateFileType(file, widget.Accept);
    }

    /// <summary>
    /// Validates multiple files against the widget's accept pattern and max files limit
    /// </summary>
    /// <param name="widget">The file input widget</param>
    /// <param name="files">The files to validate</param>
    /// <returns>Validation result</returns>
    public static ValidationResult ValidateFiles(this FileInputBase widget, IEnumerable<FileInput> files)
    {
        var filesList = files.ToList();

        // Validate file count first
        var countValidation = FileInputValidation.ValidateFileCount(filesList, widget.MaxFiles);
        if (!countValidation.IsValid)
        {
            return countValidation;
        }

        // Then validate file types
        return FileInputValidation.ValidateFileTypes(filesList, widget.Accept);
    }


}