using System.Globalization;
using System.Text;
using Antlr4.Runtime.Tree;

namespace Ivy.Filters;

/// <summary>
/// Visitor that builds an AST from the ANTLR parse tree and performs semantic validation
/// </summary>
public class FilterAstVisitor(
    IDictionary<string, FieldMeta> fieldsByDisplayName,
    FilterErrorListener errorListener)
    : FiltersBaseVisitor<Node>
{
    public override Node VisitFormula(FiltersParser.FormulaContext context)
    {
        return Visit(context.expr());
    }

    public override Node VisitExpr(FiltersParser.ExprContext context)
    {
        return Visit(context.orExpr());
    }

    public override Node VisitOrExpr(FiltersParser.OrExprContext context)
    {
        var left = Visit(context.andExpr(0));

        for (int i = 1; i < context.andExpr().Length; i++)
        {
            var right = Visit(context.andExpr(i));
            left = new Or(left, right);
        }

        return left;
    }

    public override Node VisitAndExpr(FiltersParser.AndExprContext context)
    {
        var left = Visit(context.unaryExpr(0));

        for (int i = 1; i < context.unaryExpr().Length; i++)
        {
            var right = Visit(context.unaryExpr(i));
            left = new And(left, right);
        }

        return left;
    }

    public override Node VisitUnaryExpr(FiltersParser.UnaryExprContext context)
    {
        if (context.NOT() != null && context.unaryExpr() != null)
        {
            var inner = Visit(context.unaryExpr());
            return new Not(inner);
        }

        if (context.primary() != null)
        {
            return Visit(context.primary());
        }

        // This shouldn't happen but handle it gracefully
        return CreateErrorNode();
    }

    public override Node VisitGroup(FiltersParser.GroupContext context)
    {
        return Visit(context.expr());
    }

    public override Node VisitComparison(FiltersParser.ComparisonContext context)
    {
        var fieldRef = context.fieldRef();
        var field = ResolveField(fieldRef);
        if (field == null) return CreateErrorNode();

        var op = NormalizeComparisonOperator(context.compOp());
        var operand = ExtractOperandValue(context.operand(), field.Type);

        if (!IsOperatorValidForType(op, field.Type))
        {
            errorListener.AddSemanticError(
                $"Operator '{op}' is not valid for field type '{field.Type}'",
                context.compOp().Start);
            return CreateErrorNode();
        }

        return new Leaf(field.DisplayName, field.ColId, field.Type, op, operand);
    }

    public override Node VisitTextOperation(FiltersParser.TextOperationContext context)
    {
        var fieldRef = context.fieldRef();
        var field = ResolveField(fieldRef);
        if (field == null) return CreateErrorNode();

        // Text operations are only valid for text fields
        if (field.Type != FieldType.Text)
        {
            errorListener.AddSemanticError(
                $"Text operation can only be used on text fields, but '{field.DisplayName}' is of type '{field.Type}'",
                fieldRef.Start);
            return CreateErrorNode();
        }

        var op = NormalizeTextOperator(context.textOp());
        var stringValue = ExtractStringValue(context.stringLiteral());

        // Check if this is a NOT text operation
        if (context.NOT() != null)
        {
            // Flip the operator for NOT
            op = op switch
            {
                Op.Contains => Op.NotContains,
                Op.NotContains => Op.Contains,
                // StartsWith and EndsWith don't have direct negations
                _ => op
            };
        }

        return new Leaf(field.DisplayName, field.ColId, field.Type, op, stringValue);
    }

    public override Node VisitExistenceOperation(FiltersParser.ExistenceOperationContext context)
    {
        var fieldRef = context.fieldRef();
        var field = ResolveField(fieldRef);
        if (field == null) return CreateErrorNode();

        var op = context.NOT() != null ? Op.NotBlank : Op.Blank;

        return new Leaf(field.DisplayName, field.ColId, field.Type, op);
    }

    private FieldMeta? ResolveField(FiltersParser.FieldRefContext fieldRef)
    {
        // Extract field name from FIELD token which includes the brackets
        var fieldToken = fieldRef.FIELD().GetText();
        var displayName = fieldToken.Substring(1, fieldToken.Length - 2).Trim();

        if (!fieldsByDisplayName.TryGetValue(displayName, out var field))
        {
            errorListener.AddSemanticError(
                $"Unknown column '{displayName}'",
                fieldRef.Start);
            return null;
        }

        return field;
    }

    private Op NormalizeComparisonOperator(FiltersParser.CompOpContext compOp)
    {
        var text = compOp.GetText().ToLowerInvariant().Replace(" ", "");

        return text switch
        {
            "=" or "==" or "equals" => Op.Equals,
            "!=" or "notequal" => Op.NotEqual,
            ">" or "greaterthan" => Op.GreaterThan,
            ">=" or "greaterthanorequal" or "greaterorequal" => Op.GreaterThanOrEqual,
            "<" or "lessthan" => Op.LessThan,
            "<=" or "lessthanorequal" or "lessorequal" => Op.LessThanOrEqual,
            _ => Op.Equals // Default fallback
        };
    }

    private Op NormalizeTextOperator(FiltersParser.TextOpContext textOp)
    {
        var tokens = new List<string>();
        for (int i = 0; i < textOp.ChildCount; i++)
        {
            if (textOp.GetChild(i) is ITerminalNode terminal)
            {
                tokens.Add(terminal.GetText().ToLowerInvariant());
            }
        }

        var combined = string.Join("", tokens);

        return combined switch
        {
            "contains" => Op.Contains,
            "notcontains" => Op.NotContains,
            "startswith" => Op.StartsWith,
            "endswith" => Op.EndsWith,
            _ => Op.Contains // Default fallback
        };
    }

    private object? ExtractOperandValue(FiltersParser.OperandContext operand, FieldType fieldType)
    {
        if (operand.number() != null)
        {
            return ExtractNumberValue(operand.number(), fieldType);
        }

        if (operand.stringLiteral() != null)
        {
            var stringValue = ExtractStringValue(operand.stringLiteral());

            // Validate date strings for date/datetime fields
            if ((fieldType == FieldType.Date || fieldType == FieldType.DateTime) && !string.IsNullOrEmpty(stringValue))
            {
                if (!DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    errorListener.AddSemanticError(
                        $"Invalid date format: '{stringValue}'",
                        operand.Start);
                    return null;
                }
            }

            return stringValue;
        }

        if (operand.booleanLiteral() != null)
        {
            var boolText = operand.booleanLiteral().GetText().ToLowerInvariant();
            return boolText == "true";
        }

        return null;
    }

    private object? ExtractNumberValue(FiltersParser.NumberContext number, FieldType fieldType)
    {
        var text = number.GetText();

        if (fieldType == FieldType.Number)
        {
            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue))
            {
                return decimalValue;
            }
        }

        // For dates, keep as string for later parsing by the evaluator
        return text;
    }

    private string ExtractStringValue(FiltersParser.StringLiteralContext stringLiteral)
    {
        var text = stringLiteral.GetText();

        // Remove surrounding quotes
        if (text.Length >= 2 && text.StartsWith('"') && text.EndsWith('"'))
        {
            text = text[1..^1];
        }

        // Process escape sequences
        var result = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\\' && i + 1 < text.Length)
            {
                switch (text[i + 1])
                {
                    case '"':
                        result.Append('"');
                        i++; // Skip the next character
                        break;
                    case '\\':
                        result.Append('\\');
                        i++; // Skip the next character
                        break;
                    default:
                        result.Append(text[i]);
                        break;
                }
            }
            else
            {
                result.Append(text[i]);
            }
        }

        return result.ToString();
    }

    private static bool IsOperatorValidForType(Op op, FieldType fieldType)
    {
        return op switch
        {
            Op.Contains or Op.NotContains or Op.StartsWith or Op.EndsWith => fieldType == FieldType.Text,
            Op.Equals or Op.NotEqual => true, // Valid for all types
            Op.GreaterThan or Op.GreaterThanOrEqual or Op.LessThan or Op.LessThanOrEqual =>
                fieldType is FieldType.Number or FieldType.Date or FieldType.DateTime,
            Op.Blank or Op.NotBlank => true, // Valid for all types
            _ => false
        };
    }

    private Node CreateErrorNode()
    {
        // Return a dummy node that can be used in case of errors
        // In a real implementation, you might want to have an explicit Error node type
        return new Leaf("__error__", "__error__", FieldType.Text, Op.Equals, null);
    }
}