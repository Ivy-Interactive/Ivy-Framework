using System.Runtime.CompilerServices;
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

    /// <summary>Gets or sets the maximum file size in bytes.</summary>
    [Prop] public long? MaxFileSize { get; set; }

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

    /// <summary>Gets or sets the event handler called when a file upload is canceled (passes FileUpload.Id as parameter).</summary>
    [Event] public Func<Event<IAnyInput, Guid>, ValueTask>? OnCancel { get; set; }

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

        if (value is IFileUpload file)
        {
            // Validate file type
            var typeValidation = FileInputValidation.ValidateFileType(file, Accept);
            if (!typeValidation.IsValid) return typeValidation;

            // Validate file size
            return FileInputValidation.ValidateFileSize(file, MaxFileSize);
        }

        if (value is IEnumerable<IFileUpload> files)
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

            // Validate file types if Accept is set
            if (!string.IsNullOrWhiteSpace(Accept))
            {
                var typeValidation = FileInputValidation.ValidateFileTypes(filesList, Accept);
                if (!typeValidation.IsValid) return typeValidation;
            }

            // Validate file sizes
            foreach (var f in filesList)
            {
                var sizeValidation = FileInputValidation.ValidateFileSize(f, MaxFileSize);
                if (!sizeValidation.IsValid) return sizeValidation;
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
    [Obsolete("ToFileInput now requires an UploadContext. Use state.ToFileInput(uploadContext, ...).", true)]
    public static FileInputBase ToFileInput(this IAnyState state, string? placeholder = null, bool disabled = false, FileInputs variant = FileInputs.Drop)
    {
        throw new NotSupportedException("ToFileInput now requires an UploadContext. Use state.ToFileInput(uploadContext, ...).");
    }

    /// <summary>
    /// Creates a file input that automatically uploads files using the provided upload context
    /// and wires cancellation with state reset.
    /// </summary>
    /// <param name="state">The state to bind the file input to.</param>
    /// <param name="uploadContext">The upload context state from UseUpload hook.</param>
    /// <param name="placeholder">Optional placeholder text displayed when no files are selected.</param>
    /// <param name="disabled">Whether the input should be disabled initially.</param>
    /// <param name="variant">The visual variant of the file input.</param>
    public static FileInputBase ToFileInput(this IAnyState state, IState<UploadContext> uploadContext, string? placeholder = null, bool disabled = false, FileInputs variant = FileInputs.Drop)
    {
        static bool IsFileUploadType(Type t)
        {
            if (t == typeof(FileUpload)) return true;
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(FileUpload<>)) return true;
            return typeof(IFileUpload).IsAssignableFrom(t);
        }

        static bool IsEnumerableOfFileUpload(Type t)
        {
            if (t == typeof(string)) return false;
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var arg = t.GetGenericArguments()[0];
                return IsFileUploadType(arg);
            }
            foreach (var it in t.GetInterfaces())
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    var arg = it.GetGenericArguments()[0];
                    if (IsFileUploadType(arg)) return true;
                }
            }
            return false;
        }

        static FileUpload Project(IFileUpload f) => new()
        {
            Id = f.Id,
            FileName = f.FileName,
            ContentType = f.ContentType,
            Length = f.Length,
            Progress = f.Progress,
            Status = f.Status
        };

        var stateType = state.GetStateType();
        var isCollection = IsEnumerableOfFileUpload(stateType);

        FileInputBase input;
        if (isCollection)
        {
            var valueObj = state.As<object>().Value;
            IEnumerable<FileUpload> projected = Array.Empty<FileUpload>();
            if (valueObj is IEnumerable<IFileUpload> list)
            {
                projected = list.Select(Project).ToArray();
            }
            input = new FileInput<IEnumerable<FileUpload>>(projected, placeholder, disabled, variant) with { Multiple = true };
        }
        else
        {
            var valueObj = state.As<object>().Value;
            FileUpload? single = valueObj is IFileUpload f ? Project(f) : null;
            input = new FileInput<FileUpload?>(single, placeholder, disabled, variant);
        }

        var ctx = uploadContext.Value;
        input = input with
        {
            UploadUrl = ctx.UploadUrl,
            Accept = ctx.Accept ?? input.Accept,
            MaxFileSize = ctx.MaxFileSize,
            MaxFiles = ctx.MaxFiles ?? input.MaxFiles
        };

        input = input with
        {
            OnCancel = e =>
            {
                var fileId = e.Value;
                uploadContext.Value.Cancel(fileId);

                try
                {
                    // Handle common immutable collection cases by removing the canceled file
                    if (stateType == typeof(System.Collections.Immutable.ImmutableArray<Ivy.Services.FileUpload>))
                    {
                        var s = state.As<System.Collections.Immutable.ImmutableArray<Ivy.Services.FileUpload>>();
                        s.Set(list =>
                        {
                            var builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<Ivy.Services.FileUpload>(list.Length);
                            foreach (var f in list)
                            {
                                if (f.Id != fileId) builder.Add(f);
                            }
                            return builder.ToImmutable();
                        });
                    }
                    else if (stateType == typeof(System.Collections.Immutable.ImmutableArray<Ivy.Services.FileUpload<byte[]>>))
                    {
                        var s = state.As<System.Collections.Immutable.ImmutableArray<Ivy.Services.FileUpload<byte[]>>>();
                        s.Set(list =>
                        {
                            var builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<Ivy.Services.FileUpload<byte[]>>(list.Length);
                            foreach (var f in list)
                            {
                                if (f.Id != fileId) builder.Add(f);
                            }
                            return builder.ToImmutable();
                        });
                    }
                    else
                    {
                        // Fallback: reset state (works for single-file or unsupported collections)
                        state.As<object>().Reset();
                    }
                }
                catch
                {
                    // As a last resort, reset
                    state.As<object>().Reset();
                }

                return ValueTask.CompletedTask;
            }
        };

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

    /// <summary>Sets the maximum file size in bytes for the file input.</summary>
    /// <param name="widget">The file input to configure.</param>
    /// <param name="maxFileSize">The maximum file size in bytes.</param>
    public static FileInputBase MaxFileSize(this FileInputBase widget, long maxFileSize)
    {
        return widget with { MaxFileSize = maxFileSize };
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
    public static ValidationResult ValidateFile(this FileInputBase widget, IFileUpload file)
    {
        return FileInputValidation.ValidateFileType(file, widget.Accept);
    }

    /// <summary>
    /// Validates multiple files against the widget's accept pattern and maximum file count limit.
    /// </summary>
    /// <param name="widget">The file input widget containing validation rules.</param>
    /// <param name="files">The files to validate.</param>
    public static ValidationResult ValidateFiles(this FileInputBase widget, IEnumerable<IFileUpload> files)
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
    /// Sets the cancel event handler for the file input.
    /// </summary>
    /// <param name="widget">The file input to configure.</param>
    /// <param name="onCancel">The event handler to call when a file is canceled, receives the FileUpload.Id.</param>
    [OverloadResolutionPriority(1)]
    public static FileInputBase HandleCancel(this FileInputBase widget, Func<Event<IAnyInput, Guid>, ValueTask> onCancel)
    {
        return widget with { OnCancel = onCancel };
    }

    /// <summary>
    /// Sets the cancel event handler for the file input.
    /// </summary>
    /// <param name="widget">The file input to configure.</param>
    /// <param name="onCancel">The event handler to call when a file is canceled, receives the FileUpload.Id.</param>
    public static FileInputBase HandleCancel(this FileInputBase widget, Action<Event<IAnyInput, Guid>> onCancel)
    {
        return widget.HandleCancel(onCancel.ToValueTask());
    }

    /// <summary>
    /// Sets a simple cancel event handler for the file input.
    /// </summary>
    /// <param name="widget">The file input to configure.</param>
    /// <param name="onCancel">The simple action to perform when a file is canceled, receives the FileUpload.Id.</param>
    public static FileInputBase HandleCancel(this FileInputBase widget, Action<Guid> onCancel)
    {
        return widget.HandleCancel(e => { onCancel(e.Value); return ValueTask.CompletedTask; });
    }
}
