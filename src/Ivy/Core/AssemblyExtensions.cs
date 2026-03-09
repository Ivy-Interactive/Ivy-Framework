using System.Reflection;

namespace Ivy.Core;

internal static class AssemblyExtensions
{
    /// <summary>
    /// Returns all loadable types from an assembly, gracefully handling
    /// ReflectionTypeLoadException when some referenced assemblies are missing.
    /// </summary>
    public static Type[] GetLoadableTypes(this Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null).ToArray()!;
        }
    }
}
