using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// A container for grouping input fields and validation logic.
/// </summary>
public record Form : WidgetBase<Form>
{
    internal Form(params object[] children) : base(children)
    {

    }

    internal Form() { }

    [Event] public EventHandler<Event<Form>>? OnSubmit { get; set; }
}

public static partial class FormExtensions
{
    public static Form OnSubmit(this Form form, Func<Event<Form>, ValueTask> onSubmit)
    {
        return form with { OnSubmit = new(onSubmit) };
    }

    public static Form OnSubmit(this Form form, Action<Event<Form>> onSubmit)
    {
        return form with { OnSubmit = new(onSubmit.ToValueTask()) };
    }

    public static Form OnSubmit(this Form form, Action onSubmit)
    {
        return form with { OnSubmit = new(_ => { onSubmit(); return ValueTask.CompletedTask; }) };
    }

    public static Form OnSubmit(this Form form, Func<ValueTask> onSubmit)
    {
        return form with { OnSubmit = new(_ => onSubmit()) };
    }
}