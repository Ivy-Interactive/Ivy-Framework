using System.Runtime.CompilerServices;
using Ivy.Apps;
using Ivy.Core;
using Ivy.Helpers;

namespace Ivy.Views;

public class DemoView : ViewBase
{
    private readonly object _content;
    private readonly string? _code;

    [OverloadResolutionPriority(1)]
    public DemoView(FuncBuilder content, [CallerArgumentExpression(nameof(content))] string? code = null)
    {
        _content = content;
        _code = FormatExpression(code!);
    }

    public DemoView(object content, [CallerArgumentExpression(nameof(content))] string? code = null)
    {
        _content = content;
        _code = code;
    }

    public override object? Build()
    {
        return Layout.Vertical()
               | _content
               | Text.Code(_code!, Languages.Csharp)
            ;
    }

    // Remove any "<csidentifier> =>" or "(<csidentifier>) =>" and braces, then rearrange lines.
    // Ensure the last statement has a semicolon.
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