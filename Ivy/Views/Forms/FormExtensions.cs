using Ivy.Core.Hooks;

namespace Ivy.Views.Forms;

/// <summary>
/// Provides extension methods for creating form builders from state objects with fluent syntax.
/// </summary>
/// <remarks>
/// This class enables the conversion of reactive state objects into form builders using
/// a simple and intuitive extension method syntax. It serves as the entry point for
/// creating forms with automatic scaffolding and intelligent input selection.
/// </remarks>
public static class FormExtensions
{
    /// <summary>
    /// Creates a form builder from a reactive state object with automatic field scaffolding.
    /// </summary>
    /// <typeparam name="T">The type of the model object contained in the state.</typeparam>
    /// <param name="obj">The reactive state object containing the model to be edited by the form.</param>
    /// <param name="submitTitle">The text displayed on the form's submit button. Default is "Save".</param>
    /// <returns>A new FormBuilder instance configured for the specified model type with automatic field discovery.</returns>
    /// <remarks>
    /// <para>This extension method provides a fluent entry point for form creation by converting
    /// any IState&lt;T&gt; object into a FormBuilder&lt;T&gt;. The resulting form builder automatically
    /// inspects the model type using reflection to discover all public fields and properties,
    /// then creates appropriate input controls based on intelligent heuristics.</para>
    /// 
    /// <para><strong>Automatic Features:</strong></para>
    /// <list type="bullet">
    /// <item><description><strong>Field Discovery:</strong> Automatically finds all public fields and properties</description></item>
    /// <item><description><strong>Label Generation:</strong> Converts PascalCase names to readable labels</description></item>
    /// <item><description><strong>Input Selection:</strong> Chooses appropriate input types based on field names and types</description></item>
    /// <item><description><strong>Validation Setup:</strong> Applies required field validation based on attributes</description></item>
    /// <item><description><strong>Layout Defaults:</strong> Provides sensible default field ordering and layout</description></item>
    /// </list>
    /// 
    /// <para><strong>Usage Examples:</strong></para>
    /// <code>
    /// // Simple form with automatic scaffolding
    /// var userState = UseState(new User());
    /// return userState.ToForm();
    /// 
    /// // Form with customization
    /// return userState.ToForm()
    ///     .Label(x => x.FirstName, "Given Name")
    ///     .Required(x => x.Email)
    ///     .Place(0, x => x.FirstName, x => x.LastName)
    ///     .Group("Contact", x => x.Email, x => x.Phone);
    /// </code>
    /// 
    /// <para>The form builder supports extensive customization through its fluent API while
    /// maintaining the convenience of automatic scaffolding for rapid development.</para>
    /// </remarks>
    /// <seealso cref="FormBuilder{T}"/>
    public static FormBuilder<T> ToForm<T>(this IState<T> obj, string submitTitle = "Save")
    {
        return new FormBuilder<T>(obj, submitTitle);
    }
}