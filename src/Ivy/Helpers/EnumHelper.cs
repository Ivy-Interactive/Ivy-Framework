using System.ComponentModel;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class EnumHelper
{
    /// <summary>
    /// Gets the display name for an enum value.
    /// Uses the [Description] attribute if present, otherwise falls back to SplitPascalCase.
    /// </summary>
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var description = field?.GetCustomAttribute<DescriptionAttribute>()?.Description;
        return description ?? StringHelper.SplitPascalCase(value.ToString()) ?? value.ToString();
    }
}
