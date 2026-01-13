using System.Diagnostics;

namespace Ivy.Core.Helpers;

/// <summary>
/// Represents the location in source code where a call was made.
/// </summary>
/// <param name="FilePath">The file path of the source file.</param>
/// <param name="LineNumber">The line number in the source file.</param>
/// <param name="MemberName">The name of the member (method/property) that made the call.</param>
/// <param name="DeclaringType">The full name of the type that declares the member.</param>
public record CallSite(string FilePath, int LineNumber, string MemberName, string DeclaringType)
{
    /// <summary>
    /// Creates a <see cref="CallSite"/> from a stack trace by finding the first frame
    /// where the declaring type is not in an Ivy assembly.
    /// </summary>
    /// <param name="trace">The stack trace to analyze.</param>
    /// <returns>A <see cref="CallSite"/> representing the caller's location, or null if not found.</returns>
    public static CallSite? From(StackTrace trace)
    {
        for (var i = 0; i < trace.FrameCount; i++)
        {
            var frame = trace.GetFrame(i);

            var method = frame?.GetMethod();
            if (method is null)
            {
                continue;
            }

            var declaringType = method.DeclaringType;
            if (declaringType is null)
            {
                continue;
            }

            // Skip frames where the declaring type is in the Ivy assembly
            var assemblyName = declaringType.Assembly.GetName().Name;
            if (assemblyName is "Ivy")
            {
                continue;
            }

            var typeName = declaringType.FullName ?? declaringType.Name;
            var filePath = frame?.GetFileName() ?? string.Empty;
            if (frame != null)
            {
                var lineNumber = frame.GetFileLineNumber();
                var memberName = method.Name;

                return new CallSite(filePath, lineNumber, memberName, typeName);
            }
        }

        return null;
    }
}