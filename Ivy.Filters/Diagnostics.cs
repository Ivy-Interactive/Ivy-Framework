using Antlr4.Runtime;

namespace Ivy.Filters;

/// <summary>
/// Severity levels for filter diagnostics
/// </summary>
public enum DiagnosticSeverity
{
    Error,
    Warning,
    Information
}

/// <summary>
/// Represents a diagnostic issue found during filter parsing or validation
/// </summary>
/// <param name="Severity">The severity level of the diagnostic</param>
/// <param name="Message">Human-readable description of the issue</param>
/// <param name="Line">Line number where the issue occurs (1-based)</param>
/// <param name="Column">Column number where the issue occurs (0-based)</param>
/// <param name="Length">Length of the problematic text span</param>
public record Diagnostic(
    DiagnosticSeverity Severity,
    string Message,
    int Line,
    int Column,
    int Length = 0
);

/// <summary>
/// Custom error listener for ANTLR parser that collects syntax errors
/// </summary>
public class FilterErrorListener : BaseErrorListener, IAntlrErrorListener<int>
{
    private readonly List<Diagnostic> _diagnostics = new();

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics.AsReadOnly();

    public override void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        var length = offendingSymbol?.Text?.Length ?? 0;
        var diagnostic = new Diagnostic(
            DiagnosticSeverity.Error,
            FormatSyntaxErrorMessage(msg, offendingSymbol),
            line,
            charPositionInLine,
            length
        );

        _diagnostics.Add(diagnostic);
    }

    /// <summary>
    /// Adds a semantic error to the diagnostics collection
    /// </summary>
    public void AddSemanticError(string message, IToken? token = null)
    {
        var line = token?.Line ?? 1;
        var column = token?.Column ?? 0;
        var length = token?.Text?.Length ?? 0;

        var diagnostic = new Diagnostic(
            DiagnosticSeverity.Error,
            message,
            line,
            column,
            length
        );

        _diagnostics.Add(diagnostic);
    }

    /// <summary>
    /// Adds a warning to the diagnostics collection
    /// </summary>
    public void AddWarning(string message, IToken? token = null)
    {
        var line = token?.Line ?? 1;
        var column = token?.Column ?? 0;
        var length = token?.Text?.Length ?? 0;

        var diagnostic = new Diagnostic(
            DiagnosticSeverity.Warning,
            message,
            line,
            column,
            length
        );

        _diagnostics.Add(diagnostic);
    }

    /// <summary>
    /// Clears all diagnostics
    /// </summary>
    public void Clear()
    {
        _diagnostics.Clear();
    }

    public void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        int offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        var diagnostic = new Diagnostic(
            DiagnosticSeverity.Error,
            FormatSyntaxErrorMessage(msg, null),
            line,
            charPositionInLine,
            1
        );

        _diagnostics.Add(diagnostic);
    }

    private static string FormatSyntaxErrorMessage(string msg, IToken? offendingSymbol)
    {
        if (offendingSymbol == null)
            return msg;

        return offendingSymbol.Type switch
        {
            TokenConstants.EOF => "Unexpected end of input",
            _ when msg.Contains("missing") => $"Missing {ExtractExpected(msg)}",
            _ when msg.Contains("extraneous") => $"Unexpected '{offendingSymbol.Text}'",
            _ when msg.Contains("mismatched") => $"Unexpected '{offendingSymbol.Text}', expected {ExtractExpected(msg)}",
            _ => msg
        };
    }

    private static string ExtractExpected(string msg)
    {
        // Try to extract expected tokens from ANTLR error message
        var expectingIndex = msg.IndexOf("expecting", StringComparison.OrdinalIgnoreCase);
        if (expectingIndex >= 0)
        {
            return msg[(expectingIndex + "expecting".Length)..].Trim();
        }

        return "valid token";
    }
}