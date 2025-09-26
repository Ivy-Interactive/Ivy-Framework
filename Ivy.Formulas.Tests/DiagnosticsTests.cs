using Ivy.Formulas;
using Antlr4.Runtime;

namespace Ivy.Formulas.Tests;

public class DiagnosticsTests
{
    [Fact]
    public void Diagnostic_Creation_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var diagnostic = new Diagnostic(
            DiagnosticSeverity.Error,
            "Test error message",
            5,
            10,
            3
        );

        // Assert
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal("Test error message", diagnostic.Message);
        Assert.Equal(5, diagnostic.Line);
        Assert.Equal(10, diagnostic.Column);
        Assert.Equal(3, diagnostic.Length);
    }

    [Fact]
    public void FormulaErrorListener_SyntaxError_ShouldAddDiagnostic()
    {
        // Arrange
        var errorListener = new FormulaErrorListener();
        var mockToken = new CommonToken(1, "test");

        // Act
        errorListener.SyntaxError(
            TextWriter.Null,
            null!,
            mockToken,
            1,
            5,
            "test error",
            null!
        );

        // Assert
        Assert.Single(errorListener.Diagnostics);
        var diagnostic = errorListener.Diagnostics.First();
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(1, diagnostic.Line);
        Assert.Equal(5, diagnostic.Column);
    }

    [Fact]
    public void FormulaErrorListener_LexerSyntaxError_ShouldAddDiagnostic()
    {
        // Arrange
        var errorListener = new FormulaErrorListener();

        // Act
        errorListener.SyntaxError(
            TextWriter.Null,
            null!,
            42, // offending symbol (int for lexer)
            2,
            8,
            "lexer error",
            null!
        );

        // Assert
        Assert.Single(errorListener.Diagnostics);
        var diagnostic = errorListener.Diagnostics.First();
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(2, diagnostic.Line);
        Assert.Equal(8, diagnostic.Column);
    }

    [Fact]
    public void FormulaErrorListener_AddSemanticError_ShouldAddDiagnostic()
    {
        // Arrange
        var errorListener = new FormulaErrorListener();
        var token = new CommonToken(1, "test") { Line = 3, Column = 15 };

        // Act
        errorListener.AddSemanticError("Column not found", token);

        // Assert
        Assert.Single(errorListener.Diagnostics);
        var diagnostic = errorListener.Diagnostics.First();
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal("Column not found", diagnostic.Message);
        Assert.Equal(3, diagnostic.Line);
        Assert.Equal(15, diagnostic.Column);
    }

    [Fact]
    public void FormulaErrorListener_AddWarning_ShouldAddWarningDiagnostic()
    {
        // Arrange
        var errorListener = new FormulaErrorListener();

        // Act
        errorListener.AddWarning("This is a warning");

        // Assert
        Assert.Single(errorListener.Diagnostics);
        var diagnostic = errorListener.Diagnostics.First();
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal("This is a warning", diagnostic.Message);
        Assert.Equal(1, diagnostic.Line);
        Assert.Equal(0, diagnostic.Column);
    }

    [Fact]
    public void FormulaErrorListener_Clear_ShouldRemoveAllDiagnostics()
    {
        // Arrange
        var errorListener = new FormulaErrorListener();
        errorListener.AddSemanticError("Error 1");
        errorListener.AddWarning("Warning 1");

        // Act
        errorListener.Clear();

        // Assert
        Assert.Empty(errorListener.Diagnostics);
    }

    [Fact]
    public void FormulaErrorListener_MultipleDiagnostics_ShouldTrackAll()
    {
        // Arrange
        var errorListener = new FormulaErrorListener();

        // Act
        errorListener.AddSemanticError("Error 1");
        errorListener.AddWarning("Warning 1");
        errorListener.AddSemanticError("Error 2");

        // Assert
        Assert.Equal(3, errorListener.Diagnostics.Count);
        Assert.Equal(2, errorListener.Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Single(errorListener.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning));
    }

    [Theory]
    [InlineData(DiagnosticSeverity.Error)]
    [InlineData(DiagnosticSeverity.Warning)]
    [InlineData(DiagnosticSeverity.Information)]
    public void DiagnosticSeverity_AllValues_ShouldBeSupported(DiagnosticSeverity severity)
    {
        // Arrange & Act
        var diagnostic = new Diagnostic(severity, "Test", 1, 0);

        // Assert
        Assert.Equal(severity, diagnostic.Severity);
    }

    [Fact]
    public void FormulaErrorListener_Diagnostics_ShouldBeReadOnly()
    {
        // Arrange
        var errorListener = new FormulaErrorListener();

        // Act
        var diagnostics = errorListener.Diagnostics;

        // Assert
        Assert.IsType<System.Collections.ObjectModel.ReadOnlyCollection<Diagnostic>>(diagnostics);
    }
}