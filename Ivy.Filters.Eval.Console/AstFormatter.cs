using Ivy.Filters;
using System.Text;

namespace Ivy.Filters.Eval.Console;

public static class AstFormatter
{
    public static string ToFilterExpression(Node? node)
    {
        if (node == null)
            return "(null)";

        return node switch
        {
            And and => $"{ToFilterExpression(and.Left)} AND {ToFilterExpression(and.Right)}",
            Or or => $"({ToFilterExpression(or.Left)} OR {ToFilterExpression(or.Right)})",
            Not not => $"NOT {ToFilterExpression(not.Inner)}",
            Leaf leaf => FormatLeaf(leaf),
            _ => node.GetType().Name
        };
    }

    private static string FormatLeaf(Leaf leaf)
    {
        var field = $"[{leaf.FieldDisplay}]";

        return leaf.Op switch
        {
            Op.Contains => $"{field} contains {FormatValue(leaf.A)}",
            Op.NotContains => $"{field} not contains {FormatValue(leaf.A)}",
            Op.StartsWith => $"{field} starts with {FormatValue(leaf.A)}",
            Op.EndsWith => $"{field} ends with {FormatValue(leaf.A)}",
            Op.Equals => $"{field} = {FormatValue(leaf.A)}",
            Op.NotEqual => $"{field} != {FormatValue(leaf.A)}",
            Op.GreaterThan => $"{field} > {FormatValue(leaf.A)}",
            Op.GreaterThanOrEqual => $"{field} >= {FormatValue(leaf.A)}",
            Op.LessThan => $"{field} < {FormatValue(leaf.A)}",
            Op.LessThanOrEqual => $"{field} <= {FormatValue(leaf.A)}",
            Op.Blank => $"{field} is blank",
            Op.NotBlank => $"{field} is not blank",
            _ => $"{field} {leaf.Op} {FormatValue(leaf.A)}"
        };
    }

    private static string FormatValue(object? value)
    {
        if (value == null)
            return "null";

        return value switch
        {
            string s => $"\"{s}\"",
            bool b => b.ToString().ToLowerInvariant(),
            _ => value.ToString() ?? "null"
        };
    }
}
