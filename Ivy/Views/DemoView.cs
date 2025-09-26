using System.Runtime.CompilerServices;
using Ivy.Apps;
using Ivy.Core;
using Ivy.Helpers;

namespace Ivy.Views;

/// <summary>
/// Represents a demo view that displays content alongside its corresponding C# code.
/// This view is designed for documentation and demonstration purposes, showing both
/// the rendered content and the source code that generated it.
/// </summary>
public class DemoView : ViewBase
{
    private readonly object _content;
    private readonly string? _code;

    /// <summary>
    /// Initializes a new instance of the DemoView class with a function builder
    /// and automatically extracts the code expression for display.
    /// </summary>
    /// <param name="content">A function builder that creates the content to display
    /// in the demo view. The content will be rendered above the code display.</param>
    /// <param name="code">The C# code expression that generates the content.
    /// This parameter is automatically populated by the compiler using the
    /// CallerArgumentExpression attribute.</param>
    [OverloadResolutionPriority(1)]
    public DemoView(FuncBuilder content, [CallerArgumentExpression(nameof(content))] string? code = null)
    {
        _content = content;
        _code = FormatExpression(code!);
    }

    /// <summary>
    /// Initializes a new instance of the DemoView class with content and
    /// automatically extracts the code expression for display.
    /// </summary>
    /// <param name="content">The content object to display in the demo view.
    /// This content will be rendered above the code display.</param>
    /// <param name="code">The C# code expression that generates the content.
    /// This parameter is automatically populated by the compiler using the
    /// CallerArgumentExpression attribute.</param>
    public DemoView(object content, [CallerArgumentExpression(nameof(content))] string? code = null)
    {
        _content = content;
        _code = code;
    }

    /// <summary>
    /// Builds the demo view layout, displaying the content above the formatted
    /// C# code in a vertical layout arrangement.
    /// </summary>
    /// <returns>A vertical layout containing the demo content and formatted C# code.</returns>
    public override object? Build()
    {
        return Layout.Vertical()
               | _content
               | Text.Code(_code!, Languages.Csharp)
            ;
    }

    /// <summary>
    /// Formats C# code expressions by removing lambda syntax, cleaning up formatting,
    /// and ensuring proper code structure for display purposes.
    /// </summary>
    /// <param name="input">The raw C# code expression to format and clean up.</param>
    /// <returns>A formatted and cleaned C# code string suitable for display.</returns>
    public static string FormatExpression(string input)
    {
        var lambdaRegex = new System.Text.RegularExpressions.Regex(
            @"^[\s]*\(?[A-Za-z_][A-Za-z0-9_]*\)?\s*=>\s*{?",
            System.Text.RegularExpressions.RegexOptions.Singleline
        );
        var noLambda = lambdaRegex.Replace(input, "").TrimEnd();

        // Remove trailing "}" if it encloses the entire expression
        if (noLambda.EndsWith("}"))
            noLambda = noLambda[..^1].TrimEnd();

        // Split into lines
        var lines = noLambda.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < lines.Length; i++)
            lines[i] = lines[i].Trim();

        // Ensure the last line ends with a semicolon
        if (lines.Length > 0)
        {
            var lastLine = lines[^1];
            if (!string.IsNullOrWhiteSpace(lastLine) && !lastLine.EndsWith(";"))
                lines[^1] += ";";
        }

        // Rebuild output
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < lines.Length; i++)
        {
            if (i == 0)
            {
                sb.AppendLine(lines[i]);
            }
            else
            {
                if (lines[i].StartsWith("."))
                    sb.AppendLine("    " + lines[i]);
                else
                    sb.AppendLine(lines[i]);
            }
        }
        return sb.ToString().TrimEnd();
    }
}