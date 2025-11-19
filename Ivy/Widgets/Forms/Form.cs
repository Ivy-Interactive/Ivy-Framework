using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

public record Form : WidgetBase<Form>
{
    internal Form(params object[] children) : base(children)
    {

    }

    /// <summary>Event handler called when form is submitted via Enter key on last field.</summary>
    [Event] public Func<Event<Form>, ValueTask>? OnSubmit { get; set; }

    /// <summary>Default is Medium.</summary>
    [Prop] public Sizes Size { get; set; } = Sizes.Medium;
}

public static class FormExtensions
{
    public static Form HandleSubmit(this Form form, Func<Event<Form>, ValueTask> onSubmit)
    {
        return form with { OnSubmit = onSubmit };
    }

    public static Form HandleSubmit(this Form form, Action<Event<Form>> onSubmit)
    {
        return form with { OnSubmit = onSubmit.ToValueTask() };
    }

    public static Form HandleSubmit(this Form form, Action onSubmit)
    {
        return form with { OnSubmit = _ => { onSubmit(); return ValueTask.CompletedTask; } };
    }

    public static Form HandleSubmit(this Form form, Func<ValueTask> onSubmit)
    {
        return form with { OnSubmit = _ => onSubmit() };
    }

    public static Form Size(this Form form, Sizes size)
    {
        return form with { Size = size };
    }
}