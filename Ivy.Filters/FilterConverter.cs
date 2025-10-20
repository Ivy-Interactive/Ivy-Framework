namespace Ivy.Filters;

/// <summary>
/// Converts AST nodes to grid-compatible filter models
/// </summary>
public class FilterConverter
{
    /// <summary>
    /// Converts an AST node to a grid filter model
    /// </summary>
    public FilterModel ConvertToModel(Node node)
    {
        return node switch
        {
            And and => new GroupFilterModel
            {
                Type = "AND",
                Conditions = [ConvertToModel(and.Left), ConvertToModel(and.Right)]
            },

            Or or => new GroupFilterModel
            {
                Type = "OR",
                Conditions = [ConvertToModel(or.Left), ConvertToModel(or.Right)]
            },

            Not not => ConvertNegation(not.Inner),

            Leaf leaf => ConvertLeaf(leaf),

            _ => throw new ArgumentException($"Unknown node type: {node.GetType().Name}")
        };
    }

    private FilterModel ConvertNegation(Node inner)
    {
        // For NOT operations, we try to flip the operator when possible
        // Otherwise, we wrap in a negated join structure

        if (inner is Leaf leaf)
        {
            var negatedOp = GetNegatedOperator(leaf.Op);
            if (negatedOp.HasValue)
            {
                return ConvertLeaf(leaf with { Op = negatedOp.Value });
            }
        }

        return new GroupFilterModel
        {
            Type = "AND",
            Conditions = [ConvertToModel(inner)]
        };
    }

    private FieldFilterModel ConvertLeaf(Leaf leaf)
    {
        var filterType = leaf.Type switch
        {
            FieldType.Text => "text",
            FieldType.Number => "number",
            FieldType.Date => "date",
            FieldType.DateTime => "dateTime",
            FieldType.Boolean => "boolean",
            _ => "text"
        };

        var operationType = leaf.Op switch
        {
            Op.Contains => "contains",
            Op.NotContains => "notContains",
            Op.StartsWith => "startsWith",
            Op.EndsWith => "endsWith",
            Op.Equals => "equals",
            Op.NotEqual => "notEqual",
            Op.GreaterThan => "greaterThan",
            Op.GreaterThanOrEqual => "greaterThanOrEqual",
            Op.LessThan => "lessThan",
            Op.LessThanOrEqual => "lessThanOrEqual",
            Op.Blank => "blank",
            Op.NotBlank => "notBlank",
            _ => "equals"
        };

        var result = new FieldFilterModel(filterType)
        {
            ColId = leaf.FieldId,
            Type = operationType,
            Filter = leaf.A
        };

        return result;
    }

    private static Op? GetNegatedOperator(Op op)
    {
        return op switch
        {
            Op.Contains => Op.NotContains,
            Op.NotContains => Op.Contains,
            Op.Equals => Op.NotEqual,
            Op.NotEqual => Op.Equals,
            Op.GreaterThan => Op.LessThanOrEqual,
            Op.GreaterThanOrEqual => Op.LessThan,
            Op.LessThan => Op.GreaterThanOrEqual,
            Op.LessThanOrEqual => Op.GreaterThan,
            Op.Blank => Op.NotBlank,
            Op.NotBlank => Op.Blank,

            // These don't have direct negations
            Op.StartsWith => null,
            Op.EndsWith => null,

            _ => null
        };
    }
}