using Ivy.Core;
using Ivy.Shared;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Container widget that groups form fields and input controls.
/// </summary>
public record Form : WidgetBase<Form>
{
    /// <summary>
    /// Initializes a new instance of the Form class.
    /// </summary>
    /// <param name="children">The form content.</param>
    internal Form(params object[] children) : base(children)
    {

    }

    /// <summary>Event handler called when form is submitted via Enter key on last field.</summary>
    [Event] public Func<Event<Form>, ValueTask>? OnSubmit { get; set; }

    /// <summary>The size of the form affecting spacing between fields. Default is Medium.</summary>
    [Prop] public Sizes Size { get; set; } = Sizes.Medium;
}

public static class FormExtensions
{
    /// <summary>Sets the submit event handler for the form.</summary>
    public static Form HandleSubmit(this Form form, Func<Event<Form>, ValueTask> onSubmit)
    {
        return form with { OnSubmit = onSubmit };
    }

    /// <summary>Sets the submit event handler for the form.</summary>
    public static Form HandleSubmit(this Form form, Action<Event<Form>> onSubmit)
    {
        return form with { OnSubmit = onSubmit.ToValueTask() };
    }

    /// <summary>Sets a simple submit event handler for the form.</summary>
    public static Form HandleSubmit(this Form form, Action onSubmit)
    {
        return form with { OnSubmit = _ => { onSubmit(); return ValueTask.CompletedTask; } };
    }

    /// <summary>Sets a simple submit event handler for the form.</summary>
    public static Form HandleSubmit(this Form form, Func<ValueTask> onSubmit)
    {
        return form with { OnSubmit = _ => onSubmit() };
    }

    /// <summary>Sets the size of the form affecting spacing between fields.</summary>
    /// <param name="form">The form to configure.</param>
    /// <param name="size">The size of the form (Small, Medium, Large).</param>
    public static Form Size(this Form form, Sizes size)
    {
        return form with { Size = size };
    }
}