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
    [Event] public Func<Event<Form>, ValueTask>? HandleSubmit { get; set; }
}