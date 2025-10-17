using Ivy.Core;

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
}