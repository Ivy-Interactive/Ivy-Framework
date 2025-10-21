using Antlr4.Runtime;

namespace Ivy.Filters;

/// <summary>
/// Main entry point for parsing advanced filter filters
/// </summary>
public class FilterParser
{
    private readonly IDictionary<string, FieldMeta> _fieldsByDisplayName;

    /// <summary>
    /// Creates a new filter parser with the specified column metadata
    /// </summary>
    /// <param name="fields">Available columns mapped by their display names</param>
    public FilterParser(IEnumerable<FieldMeta> fields)
    {
        _fieldsByDisplayName = fields.ToDictionary(c => c.DisplayName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parses a filter string into an AST and filter model
    /// </summary>
    /// <param name="filter">The filter text to parse</param>
    /// <returns>Parse result containing AST, model, and diagnostics</returns>
    public FilterParseResult Parse(string filter)
    {
        var errorListener = new FilterErrorListener();

        try
        {
            var inputStream = new AntlrInputStream(filter);
            var lexer = new FiltersLexer(inputStream);
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(errorListener);

            var tokenStream = new CommonTokenStream(lexer);
            var parser = new FiltersParser(tokenStream);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(errorListener);

            var parseTree = parser.formula();

            if (errorListener.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                return new FilterParseResult
                {
                    Filter = filter,
                    Diagnostics = errorListener.Diagnostics
                };
            }

            var visitor = new FilterAstVisitor(_fieldsByDisplayName, errorListener);
            var ast = visitor.Visit(parseTree);

            if (errorListener.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                return new FilterParseResult
                {
                    Filter = filter,
                    Ast = ast,
                    Diagnostics = errorListener.Diagnostics
                };
            }

            var converter = new FilterConverter();
            var model = converter.ConvertToModel(ast);

            return new FilterParseResult
            {
                Filter = filter,
                Ast = ast,
                Model = model,
                Diagnostics = errorListener.Diagnostics
            };
        }
        catch (Exception ex)
        {
            // Handle unexpected parsing errors
            errorListener.AddSemanticError($"Unexpected error during parsing: {ex.Message}");

            return new FilterParseResult
            {
                Filter = filter,
                Diagnostics = errorListener.Diagnostics
            };
        }
    }

    /// <summary>
    /// Quick validation method that returns true if the filter is syntactically and semantically valid
    /// </summary>
    /// <param name="filter">The filter to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid(string filter)
    {
        var result = Parse(filter);
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