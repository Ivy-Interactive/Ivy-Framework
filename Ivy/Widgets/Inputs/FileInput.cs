using System.Runtime.CompilerServices;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Services;
using Ivy.Shared;
using Ivy.Widgets.Inputs;

// ReSharper disable once CheckNamespace
namespace Ivy;

public enum FileInputs
{
    Drop
}

public interface IAnyFileInput : IAnyInput
{
    public FileInputs Variant { get; set; }
}

public abstract record FileInputBase : WidgetBase<FileInputBase>, IAnyFileInput
{
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public FileInputs Variant { get; set; }

    [Prop] public string? Accept { get; set; }

    [Prop] public long? MaxFileSize { get; set; }

    [Prop] public bool Multiple { get; set; }

    [Prop] public int? MaxFiles { get; set; }

    [Prop] public string? UploadUrl { get; set; }

    [Prop] public new Scale? Scale { get; set; }

    [Event] public Func<Event<IAnyInput>, ValueTask>? OnBlur { get; set; }

    [Event] public Func<Event<IAnyInput, Guid>, ValueTask>? OnCancel { get; set; }

    public Type[] SupportedStateTypes() => [];

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

public record FileInput<TValue> : FileInputBase, IInput<TValue>, IAnyFileInput
{
    [OverloadResolutionPriority(1)]
    public FileInput(IAnyState state, string? placeholder = null, bool disabled = false, FileInputs variant = FileInputs.Drop)
        : this(placeholder, disabled, variant)
    {
        var typedState = state.As<TValue>();
        Value = typedState.Value;
    }

    [OverloadResolutionPriority(1)]
    public FileInput(TValue value, string? placeholder = null, bool disabled = false, FileInputs variant = FileInputs.Drop)
        : this(placeholder, disabled, variant)
    {
        Value = value;
    }

    public FileInput(string? placeholder = null, bool disabled = false, FileInputs variant = FileInputs.Drop)
    {
        Placeholder = placeholder;
        Variant = variant;
        Disabled = disabled;
        Width = Ivy.Shared.Size.Full();
        Height = Ivy.Shared.Size.Units(50);
    }

    [Prop] public TValue Value { get; } = default!;

    [Event] public Func<Event<IInput<TValue>, TValue>, ValueTask>? OnChange => null;
}

public static class FileInputExtensions
{
    [Obsolete("ToFileInput now requires an UploadContext. Use state.ToFileInput(uploadContext, ...).", true)]
    public static FileInputBase ToFileInput(this IAnyState state, string? placeholder = null, bool disabled = false, FileInputs variant = FileInputs.Drop)
    {
        throw new NotSupportedException("ToFileInput now requires an UploadContext. Use state.ToFileInput(uploadContext, ...).");
    }

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
                    if (stateType == typeof(System.Collections.Immutable.ImmutableArray<FileUpload>))
                    {
                        var s = state.As<System.Collections.Immutable.ImmutableArray<FileUpload>>();
                        s.Set(list =>
                        {
                            var builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<FileUpload>(list.Length);
                            foreach (var f in list)
                            {
                                if (f.Id != fileId) builder.Add(f);
                            }
                            return builder.ToImmutable();
                        });
                    }
                    else if (stateType == typeof(System.Collections.Immutable.ImmutableArray<FileUpload<byte[]>>))
                    {
                        var s = state.As<System.Collections.Immutable.ImmutableArray<FileUpload<byte[]>>>();
                        s.Set(list =>
                        {
                            var builder = System.Collections.Immutable.ImmutableArray.CreateBuilder<FileUpload<byte[]>>(list.Length);
                            foreach (var f in list)
                            {
                                if (f.Id != fileId) builder.Add(f);
                            }
                            return builder.ToImmutable();
                        });
                    }
                    else
                    {
                        state.As<object>().Reset();
                    }
                }
                catch
                {
                    state.As<object>().Reset();
                }

                return ValueTask.CompletedTask;
            }
        };

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

    public static FileInputBase MaxFileSize(this FileInputBase widget, long maxFileSize)
    {
        return widget with { MaxFileSize = maxFileSize };
    }

    public static FileInputBase UploadUrl(this FileInputBase widget, string? uploadUrl)
    {
        return widget with { UploadUrl = uploadUrl };
    }

    public static ValidationResult ValidateFile(this FileInputBase widget, IFileUpload file)
    {
        return FileInputValidation.ValidateFileType(file, widget.Accept);
    }

    public static ValidationResult ValidateFiles(this FileInputBase widget, IEnumerable<IFileUpload> files)
    {
        var filesList = files.ToList();

        var countValidation = FileInputValidation.ValidateFileCount(filesList, widget.MaxFiles);
        if (!countValidation.IsValid)
        {
            return countValidation;
        }

        return FileInputValidation.ValidateFileTypes(filesList, widget.Accept);
    }

    [OverloadResolutionPriority(1)]
    public static FileInputBase HandleBlur(this FileInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = onBlur };
    }

    public static FileInputBase HandleBlur(this FileInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget.HandleBlur(onBlur.ToValueTask());
    }

    public static FileInputBase HandleBlur(this FileInputBase widget, Action onBlur)
    {
        return widget.HandleBlur(_ => { onBlur(); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    public static FileInputBase HandleCancel(this FileInputBase widget, Func<Event<IAnyInput, Guid>, ValueTask> onCancel)
    {
        return widget with { OnCancel = onCancel };
    }

    public static FileInputBase HandleCancel(this FileInputBase widget, Action<Event<IAnyInput, Guid>> onCancel)
    {
        return widget.HandleCancel(onCancel.ToValueTask());
    }

    public static FileInputBase HandleCancel(this FileInputBase widget, Action<Guid> onCancel)
    {
        return widget.HandleCancel(e => { onCancel(e.Value); return ValueTask.CompletedTask; });
    }
}
