using System.Reflection;
using System.Runtime.CompilerServices;
using Ivy.Core;
using Ivy.Core.Helpers;

// ReSharper disable once CheckNamespace
namespace Ivy;

public abstract record ContentInputBase : WidgetBase<ContentInputBase>, IAnyInput
{
    // Text props
    [Prop] public bool Disabled { get; set; }

    [Prop] public string? Invalid { get; set; }

    [Prop] public string? Placeholder { get; set; }

    [Prop] public int? MaxLength { get; set; }

    [Prop] public int? Rows { get; set; }

    [Prop] public bool AutoFocus { get; set; }

    [Prop] public bool Nullable { get; set; }

    // File attachment props
    [Prop] public string? UploadUrl { get; set; }

    [Prop] public string? Accept { get; set; }

    [Prop] public long? MaxFileSize { get; set; }

    [Prop] public int? MaxFiles { get; set; }

    [Prop] public IEnumerable<FileUpload>? Files { get; set; }

    // Events
    [Event] public EventHandler<Event<IAnyInput>>? OnBlur { get; set; }
    [Event] public EventHandler<Event<IAnyInput>>? OnFocus { get; set; }

    [Event] public EventHandler<Event<IAnyInput>>? OnSubmit { get; set; }

    [Event] public EventHandler<Event<IAnyInput, Guid>>? OnCancel { get; set; }

    public Type[] SupportedStateTypes() => [];
}

/// <summary>
/// A text input with integrated file attachment support via drag-and-drop, clipboard paste, and file picker.
/// </summary>
public record ContentInput<TString> : ContentInputBase, IInput<TString>
    where TString : IEquatable<TString>
{
    internal ContentInput(IAnyState state, string? placeholder = null, bool disabled = false)
        : this(placeholder, disabled)
    {
        var typedState = state.As<TString>();
        Value = typedState.Value;
        OnChange = new(e => { typedState.Set(e.Value); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    internal ContentInput(TString value, Func<Event<IInput<TString>, TString>, ValueTask>? onChange = null, string? placeholder = null, bool disabled = false)
        : this(placeholder, disabled)
    {
        OnChange = onChange.ToEventHandler();
        Value = value;
    }

    internal ContentInput(TString value, Action<Event<IInput<TString>, TString>>? onChange = null, string? placeholder = null, bool disabled = false)
        : this(placeholder, disabled)
    {
        OnChange = onChange.ToEventHandler();
        Value = value;
    }

    internal ContentInput(string? placeholder = null, bool disabled = false)
    {
        Placeholder = placeholder;
        Disabled = disabled;
    }

    internal ContentInput() { }

    [Prop(AlwaysSerialize = true)] public TString Value { get; init; } = default!;

    [Prop] public new bool Nullable { get; set; } = typeof(TString).IsNullableType();

    [Event] public EventHandler<Event<IInput<TString>, TString>>? OnChange { get; }
}

public record ContentInput : ContentInput<string>
{
    internal ContentInput(IAnyState state, string? placeholder = null, bool disabled = false)
        : base(state, placeholder, disabled)
    {
    }

    [OverloadResolutionPriority(1)]
    internal ContentInput(string value, Func<Event<IInput<string>, string>, ValueTask>? onChange = null, string? placeholder = null, bool disabled = false)
        : base(value, onChange, placeholder, disabled)
    {
    }

    internal ContentInput(string value, Action<Event<IInput<string>, string>>? onChange = null, string? placeholder = null, bool disabled = false)
        : base(value, onChange?.ToValueTask(), placeholder, disabled)
    {
    }

    internal ContentInput(string? placeholder = null, bool disabled = false)
        : base(placeholder, disabled)
    {
    }
}

public static class ContentInputExtensions
{
    public static ContentInputBase ToContentInput(this IAnyState state, IState<UploadContext> uploadContext, string? placeholder = null, bool disabled = false)
    {
        var type = state.GetStateType();
        Type genericType = typeof(ContentInput<>).MakeGenericType(type);
        ContentInputBase input = (ContentInputBase)Activator.CreateInstance(genericType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new object?[] { state, placeholder, disabled }, null)!;
        var nullableProperty = genericType.GetProperty("Nullable", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        nullableProperty?.SetValue(input, type.IsNullableType());

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
            OnCancel = new(e =>
            {
                var fileId = e.Value;
                uploadContext.Value.Cancel(fileId);
                return ValueTask.CompletedTask;
            })
        };

        return input;
    }

    public static ContentInputBase Files(this ContentInputBase widget, IEnumerable<FileUpload> files)
    {
        return widget with { Files = files };
    }

    public static ContentInputBase Files<T>(this ContentInputBase widget, IEnumerable<FileUpload<T>> files)
    {
        return widget with
        {
            Files = files.Select(f => new FileUpload
            {
                Id = f.Id,
                FileName = f.FileName,
                ContentType = f.ContentType,
                Length = f.Length,
                Progress = f.Progress,
                Status = f.Status
            })
        };
    }

    public static ContentInputBase Placeholder(this ContentInputBase widget, string placeholder)
    {
        return widget with { Placeholder = placeholder };
    }

    public static ContentInputBase Disabled(this ContentInputBase widget, bool disabled = true)
    {
        return widget with { Disabled = disabled };
    }

    public static ContentInputBase Rows(this ContentInputBase widget, int rows)
    {
        return widget with { Rows = rows };
    }

    public static ContentInputBase MaxLength(this ContentInputBase widget, int maxLength)
    {
        return widget with { MaxLength = maxLength };
    }

    public static ContentInputBase Accept(this ContentInputBase widget, string accept)
    {
        return widget with { Accept = accept };
    }

    public static ContentInputBase MaxFileSize(this ContentInputBase widget, long maxFileSize)
    {
        return widget with { MaxFileSize = maxFileSize };
    }

    public static ContentInputBase MaxFiles(this ContentInputBase widget, int maxFiles)
    {
        return widget with { MaxFiles = maxFiles };
    }

    public static ContentInputBase Invalid(this ContentInputBase widget, string invalid)
    {
        return widget with { Invalid = invalid };
    }

    public static ContentInputBase Nullable(this ContentInputBase widget, bool? nullable = true)
    {
        var property = widget.GetType().GetProperty("Nullable", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (property != null && property.CanWrite)
        {
            property.SetValue(widget, nullable ?? true);
            return widget;
        }
        return widget with { Nullable = nullable ?? true };
    }

    [OverloadResolutionPriority(1)]
    public static ContentInputBase OnBlur(this ContentInputBase widget, Func<Event<IAnyInput>, ValueTask> onBlur)
    {
        return widget with { OnBlur = new(onBlur) };
    }

    public static ContentInputBase OnBlur(this ContentInputBase widget, Action<Event<IAnyInput>> onBlur)
    {
        return widget with { OnBlur = new(onBlur.ToValueTask()) };
    }

    public static ContentInputBase OnBlur(this ContentInputBase widget, Action onBlur)
    {
        return widget with { OnBlur = new(_ => { onBlur(); return ValueTask.CompletedTask; }) };
    }

    [OverloadResolutionPriority(1)]
    public static ContentInputBase OnFocus(this ContentInputBase widget, Func<Event<IAnyInput>, ValueTask> onFocus)
    {
        return widget with { OnFocus = new(onFocus) };
    }

    public static ContentInputBase OnFocus(this ContentInputBase widget, Action<Event<IAnyInput>> onFocus)
    {
        return widget with { OnFocus = new(onFocus.ToValueTask()) };
    }

    public static ContentInputBase OnFocus(this ContentInputBase widget, Action onFocus)
    {
        return widget with { OnFocus = new(_ => { onFocus(); return ValueTask.CompletedTask; }) };
    }

    [OverloadResolutionPriority(1)]
    public static ContentInputBase OnSubmit(this ContentInputBase widget, Func<Event<IAnyInput>, ValueTask> onSubmit)
    {
        return widget with { OnSubmit = onSubmit };
    }

    public static ContentInputBase OnSubmit(this ContentInputBase widget, Action<Event<IAnyInput>> onSubmit)
    {
        return widget.OnSubmit(onSubmit.ToValueTask());
    }

    public static ContentInputBase OnSubmit(this ContentInputBase widget, Action onSubmit)
    {
        return widget.OnSubmit(_ => { onSubmit(); return ValueTask.CompletedTask; });
    }

    [OverloadResolutionPriority(1)]
    public static ContentInputBase OnCancel(this ContentInputBase widget, Func<Event<IAnyInput, Guid>, ValueTask> onCancel)
    {
        return widget with { OnCancel = new(onCancel) };
    }

    public static ContentInputBase OnCancel(this ContentInputBase widget, Action<Event<IAnyInput, Guid>> onCancel)
    {
        return widget.OnCancel(onCancel.ToValueTask());
    }

    public static ContentInputBase OnCancel(this ContentInputBase widget, Action<Guid> onCancel)
    {
        return widget.OnCancel(e => { onCancel(e.Value); return ValueTask.CompletedTask; });
    }
}
