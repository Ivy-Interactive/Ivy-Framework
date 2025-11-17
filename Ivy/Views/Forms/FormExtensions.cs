using Ivy.Core.Hooks;
using Ivy.Services;

namespace Ivy.Views.Forms;

/// <summary>Provides extension methods for creating form builders from state objects with fluent syntax.</summary>
public static class FormExtensions
{
    /// <returns>A new FormBuilder instance configured for the specified model type with automatic field discovery.</returns>
    /// <seealso cref="FormBuilder{T}"/>
    public static FormBuilder<T> ToForm<T>(this IState<T> obj, string submitTitle = "Save")
    {
        return new FormBuilder<T>(obj, submitTitle);
    }
}