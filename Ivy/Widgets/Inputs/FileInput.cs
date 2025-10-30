using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Services;
using Ivy.Shared;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Defines the visual variants available for file input controls.
/// </summary>
public enum FileInputs
{
    Drop
}

/// <summary>
/// Interface for file input controls that extends IAnyInput with file-specific properties.
/// </summary>
public interface IAnyFileInput : IAnyInput
{
    /// <summary>Gets or sets the placeholder text displayed when no files are selected.</summary>
    public string? Placeholder { get; set; }

    /// <summary>Gets or sets the visual variant of the file input.</summary>
    public FileInputs Variant { get; set; }
}

/// <summary>
/// Abstract base class for file input controls that provides comprehensive file upload functionality.
/// Supports single and multiple file uploads with file type validation, size limits, and drag-and-drop interfaces.
/// </summary>
public abstract record FileInputBase : WidgetBase<FileInputBase>, IAnyFileInput
{
    /// <summary>Gets or sets whether the input is disabled.</summary>
    [Prop] public bool Disabled { get; set; }

    /// <summary>Gets or sets the validation error message.</summary>
    [Prop] public string? Invalid { get; set; }

    /// <summary>Gets or sets the placeholder text displayed when no files are selected.</summary>
    [Prop] public string? Placeholder { get; set; }

    /// <summary>Gets or sets the visual variant of the file input.</summary>
    [Prop] public FileInputs Variant { get; set; }

    /// <summary>Gets or sets the accepted file types using MIME types or file extensions.</summary>
    [Prop] public string? Accept { get; set; }

    /// <summary>Gets or sets whether multiple files can be selected.</summary>
    [Prop] public bool Multiple { get; set; }

    /// <summary>Gets or sets the maximum number of files that can be selected (only applicable when Multiple is true).</summary>
    [Prop] public int? MaxFiles { get; set; }

    /// <summary>Gets or sets the upload URL for automatic file uploads.</summary>
    [Prop] public string? UploadUrl { get; set; }

    /// <summary>Gets or sets the size of the file input.</summary>
    [Prop] public Sizes Size { get; set; }

    /// <summary>Gets or sets the event handler called when the input loses focus.</summary>
    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    /// <summary>Gets or sets the event handler called when a file is deleted (passes FileInput.Id as parameter).</summary>
    [Event] public Func<Event<IAnyInput, object>, ValueTask>? OnDelete { get; set; }

    /// <summary>
    /// Returns the types that this file input can bind to.
    /// </summary>
    public Type[] SupportedStateTypes() => [];

    /// <summary>
    /// Validates the current file input value against configured restrictions.
    /// </summary>
    /// <param name="value">The current value to validate.</param>
    public ValidationResult ValidateValue(object? value)
    {
        if (value == null) return ValidationResult.Success();

        if (value is FileUpload file)
        {
            return FileInputValidation.ValidateFileType(file, Accept);
        }

        if (value is IEnumerable<FileUpload> files)
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

/// <summary>
/// Generic file input control that provides type-safe file upload functionality.
/// State is managed entirely by the server via the upload handler.
/// </summary>
/// <typeparam name="TValue">The type of the file value.</typeparam>
public record FileInput<TValue> : FileInputBase, IInput<TValue>, IAnyFileInput
{
    /// <summary>
    /// Initializes a new instance bound to a state object.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when no files are selected.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the file input.</param>
    [OverloadResolutionPriority(1)]
    public FileInput(IAnyState state, string? placeholder = null, bool disabled = false, FileInputs variant = FileInputs.Drop)
        : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TValue>();
        Value = typedState.Value;
    }

    /// <summary>
    /// Initializes a new instance with an explicit value.
    /// </summary>
    /// <param name="value">The initial file value.</param>
    /// <param name="placeholder">Optional placeholder text displayed when no files are selected.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the file input.</param>
    [OverloadResolutionPriority(1)]
    public FileInput(TValue value, string? placeholder = null, bool disabled = false, FileInputs variant = FileInputs.Drop)
        : this(placeholder, disabled, variant)
    {
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance with basic configuration.
    /// </summary>
    /// <param name="placeholder">Optional placeholder text displayed when no files are selected.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the file input.</param>
    public FileInput(string? placeholder = null, bool disabled = false, FileInputs variant = FileInputs.Drop)
    {
        Placeholder = placeholder;
        Variant = variant;
        Disabled = disabled;
        Size = Sizes.Medium;
        Width = Ivy.Shared.Size.Full();
        Height = Ivy.Shared.Size.Units(50);
    }

    /// <summary>Gets the current file value.</summary>
    [Prop] public TValue Value { get; } = default!;

    /// <summary>OnChange event is not used - file state is managed by the server.</summary>
    [Event] public Func<Event<IInput<TValue>, TValue>, ValueTask>? OnChange => null;
}

/// <summary>
/// Provides extension methods for creating, configuring, and working with file inputs.
/// </summary>
public static class FileInputExtensions
{
    /// <summary>
    /// Creates a file input from a state object.
    /// </summary>
    /// <param name="state">The state object to bind to.</param>
    /// <param name="placeholder">Optional placeholder text displayed when no files are selected.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the file input.</param>
    public static FileInputBase ToFileInput(this IAnyState state, string? placeholder = null, bool disabled = false, FileInputs variant = FileInputs.Drop)
    {
        var type = state.GetStateType();

        //Check that type is FileUpload, FileUpload? or IEnumerable<FileUpload> (including ImmutableArray, List, etc.)
        var isCollection = typeof(IEnumerable<FileUpload>).IsAssignableFrom(type) && type != typeof(string);
        var isValid = type == typeof(FileUpload) || isCollection;

        if (!isValid)
        {
            throw new Exception("Invalid type for FileInput - must be FileUpload or IEnumerable<FileUpload>");
        }

        Type genericType = typeof(FileInput<>).MakeGenericType(type);
        FileInputBase input = (FileInputBase)Activator.CreateInstance(genericType, state, placeholder, disabled, variant)!;

        // Set Multiple based on type
        input = input with { Multiple = isCollection };

        return input;
    }

    /// <summary>
    /// Creates a file input that automatically uploads files to the specified upload URL.
    /// </summary>
    /// <param name="state">The state to bind the file input to.</param>
    /// <param name="uploadUrl">The upload URL state from UseUpload hook.</param>
    /// <param name="placeholder">Optional placeholder text displayed when no files are selected.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the file input.</param>
    public static FileInputBase ToFileInput(this IAnyState state, IState<string?> uploadUrl, string? placeholder = null, bool disabled = false, FileInputs variant = FileInputs.Drop)
    {
        var input = state.ToFileInput(placeholder, disabled, variant);
        input = input with { UploadUrl = uploadUrl.Value };
        return input;
    }

    /// <summary>Sets the placeholder text for the file input.</summary>
    /// <param name="widget">The file input to configure.</param>
    /// <param name="title">The placeholder text to display when no files are selected.</param>
    public static FileInputBase Placeholder(this FileInputBase widget, string title)
    {
        return widget with { Placeholder = title };
    }

    /// <summary>Sets the disabled state of the file input.</summary>
    /// <param name="widget">The file input to configure.</param>
    /// <param name="disabled">Whether the input should be disabled.</param>
    public static FileInputBase Disabled(this FileInputBase widget, bool disabled = true)
    {
        return widget with { Disabled = disabled };
    }

    /// <summary>Sets the visual variant of the file input.</summary>
    /// <param name="widget">The file input to configure.</param>
    /// <param name="variant">The visual variant (currently only Drop is available).</param>
    public static FileInputBase Variant(this FileInputBase widget, FileInputs variant)
    {
        return widget with { Variant = variant };
    }

    /// <summary>Sets the validation error message for the file input.</summary>
    /// <param name="widget">The file input to configure.</param>
    /// <param name="invalid">The validation error message to display.</param>
    public static FileInputBase Invalid(this FileInputBase widget, string? invalid)
    {
        return widget with { Invalid = invalid };
    }

    /// <summary>Sets the accepted file types for the file input using MIME types or file extensions.</summary>
    /// <param name="widget">The file input to configure.</param>
    /// <param name="accept">A comma-separated list of accepted file types (e.g., "image/*", ".pdf,.doc", "text/plain").</param>
    public static FileInputBase Accept(this FileInputBase widget, string accept)
    {
        return widget with { Accept = accept };
    }

    /// <summary>
    /// Sets the maximum number of files that can be selected (only applicable for multiple file inputs).
    /// </summary>
    /// <param name="widget">The file input to configure.</param>
    /// <param name="maxFiles">The maximum number of files allowed.</param>
    public static FileInputBase MaxFiles(this FileInputBase widget, int maxFiles)
    {
        if (widget.Multiple != true)
        {
            throw new InvalidOperationException("MaxFiles can only be set on a multi-file input (IEnumerable<FileInput>). Use a collection state type for multiple files.");
        }
        return widget with { MaxFiles = maxFiles };
    }

    /// <summary>Sets the upload URL for automatic file uploads.</summary>
    /// <param name="widget">The file input to configure.</param>
    /// <param name="uploadUrl">The upload URL where files should be automatically uploaded.</param>
    public static FileInputBase UploadUrl(this FileInputBase widget, string? uploadUrl)
    {
        return widget with { UploadUrl = uploadUrl };
    }

    /// <summary>Sets the size of the file input.</summary>
    /// <param name="widget">The file input to configure.</param>
    /// <param name="size">The size of the file input.</param>
    public static FileInputBase Size(this FileInputBase widget, Sizes size)
    {
        return widget with { Size = size };
    }

    /// <summary>Sets the file input size to small for compact layouts.</summary>
    /// <param name="widget">The file input to configure.</param>
    public static FileInputBase Small(this FileInputBase widget)
    {
        return widget with { Size = Sizes.Small };
    }

    /// <summary>Sets the file input size to large for prominent display.</summary>
    /// <param name="widget">The file input to configure.</param>
    public static FileInputBase Large(this FileInputBase widget)
    {
        return widget with { Size = Sizes.Large };
    }

    /// <summary>
    /// Validates a single file against the widget's accept pattern.
    /// </summary>
    /// <param name="widget">The file input widget containing validation rules.</param>
    /// <param name="file">The file to validate.</param>
    public static ValidationResult ValidateFile(this FileInputBase widget, FileUpload file)
    {
        return FileInputValidation.ValidateFileType(file, widget.Accept);
    }

    /// <summary>
    /// Validates multiple files against the widget's accept pattern and maximum file count limit.
    /// </summary>
    /// <param name="widget">The file input widget containing validation rules.</param>
    /// <param name="files">The files to validate.</param>
    public static ValidationResult ValidateFiles(this FileInputBase widget, IEnumerable<FileUpload> files)
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

    /// <summary>
    /// Sets the blur event handler for the file input.
    /// </summary>
    /// <param name="widget">The file input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    [OverloadResolutionPriority(1)]
    public static FileInputBase HandleBlur(this FileInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = onBlur };
    }

    /// <summary>
    /// Sets the blur event handler for the file input.
    /// </summary>
    /// <param name="widget">The file input to configure.</param>
    /// <param name="onBlur">The event handler to call when the input loses focus.</param>
    public static FileInputBase HandleBlur(this FileInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    /// <summary>
    /// Sets a simple blur event handler for the file input.
    /// </summary>
    /// <param name="widget">The file input to configure.</param>
    /// <param name="onBlur">The simple action to perform when the input loses focus.</param>
    public static FileInputBase HandleBlur(this FileInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }

    /// <summary>
    /// Sets the delete event handler for the file input.
    /// </summary>
    /// <param name="widget">The file input to configure.</param>
    /// <param name="onDelete">The event handler to call when a file is deleted, receives the FileInput.Id.</param>
    [OverloadResolutionPriority(1)]
    public static FileInputBase HandleDelete(this FileInputBase widget, Func<Event<IAnyInput, object>, ValueTask> onDelete)
    {
        return widget with { OnDelete = onDelete };
    }

    /// <summary>
    /// Sets the delete event handler for the file input.
    /// </summary>
    /// <param name="widget">The file input to configure.</param>
    /// <param name="onDelete">The event handler to call when a file is deleted, receives the FileInput.Id.</param>
    public static FileInputBase HandleDelete(this FileInputBase widget, Action<Event<IAnyInput, object>> onDelete)
    {
        return widget.HandleDelete(onDelete.ToValueTask());
    }

    /// <summary>
    /// Sets a simple delete event handler for the file input.
    /// </summary>
    /// <param name="widget">The file input to configure.</param>
    /// <param name="onDelete">The simple action to perform when a file is deleted, receives the FileInput.Id.</param>
    public static FileInputBase HandleDelete(this FileInputBase widget, Action<object> onDelete)
    {
        return widget.HandleDelete(e => { onDelete(e.Value); return ValueTask.CompletedTask; });
    }
}