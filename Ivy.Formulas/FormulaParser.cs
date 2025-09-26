using Antlr4.Runtime;

namespace Ivy.Formulas;

/// <summary>
/// Main entry point for parsing advanced filter formulas
/// </summary>
public class FormulaParser
{
    private readonly IDictionary<string, FieldMeta> _fieldsByDisplayName;

    /// <summary>
    /// Creates a new formula parser with the specified column metadata
    /// </summary>
    /// <param name="fields">Available columns mapped by their display names</param>
    public FormulaParser(IEnumerable<FieldMeta> fields)
    {
        _fieldsByDisplayName = fields.ToDictionary(c => c.DisplayName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parses a formula string into an AST and filter model
    /// </summary>
    /// <param name="formula">The formula text to parse</param>
    /// <returns>Parse result containing AST, model, and diagnostics</returns>
    public FormulaParseResult Parse(string formula)
    {
        var errorListener = new FormulaErrorListener();

        try
        {
            var inputStream = new AntlrInputStream(formula);
            var lexer = new FormulasLexer(inputStream);
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(errorListener);

            var tokenStream = new CommonTokenStream(lexer);
            var parser = new FormulasParser(tokenStream);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(errorListener);
            
            var parseTree = parser.formula();
            
            if (errorListener.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                return new FormulaParseResult
                {
                    Diagnostics = errorListener.Diagnostics
                };
            }
            
            var visitor = new FormulaAstVisitor(_fieldsByDisplayName, errorListener);
            var ast = visitor.Visit(parseTree);
            
            if (errorListener.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                return new FormulaParseResult
                {
                    Ast = ast,
                    Diagnostics = errorListener.Diagnostics
                };
            }
            
            var converter = new FilterConverter();
            var model = converter.ConvertToModel(ast);

            return new FormulaParseResult
            {
                Ast = ast,
                Model = model,
                Diagnostics = errorListener.Diagnostics
            };
        }
        catch (Exception ex)
        {
            // Handle unexpected parsing errors
            errorListener.AddSemanticError($"Unexpected error during parsing: {ex.Message}");

            return new FormulaParseResult
            {
                Diagnostics = errorListener.Diagnostics
            };
        }
    }

    /// <summary>
    /// Quick validation method that returns true if the formula is syntactically and semantically valid
    /// </summary>
    /// <param name="formula">The formula to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid(string formula)
    {
        var result = Parse(formula);
        return !result.HasErrors;
    }

    /// <summary>
    /// Gets all available fields
    /// </summary>
    public IEnumerable<FieldMeta> GetAvailableFields()
    {
        return _fieldsByDisplayName.Values;
    }
}