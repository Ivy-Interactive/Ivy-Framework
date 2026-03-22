using System.Linq.Expressions;

namespace Ivy.Core.Helpers;

public class ExpressionNameHelper
{
    public static string? SuggestName<T>(Expression<Func<T, object>> expression)
    {
        // Remove any conversion (e.g. boxing) to get to the “real” body.
        Expression body = RemoveConversion(expression.Body);

        // If the expression is a simple member access, return the member name.
        if (body is MemberExpression memberExpr)
        {
            return memberExpr.Member.Name;
        }

        // If the expression is a method call, we might want to be a bit smarter.
        if (body is MethodCallExpression methodCall)
        {
            return SuggestNameFromMethodCall(methodCall);
        }

        // Fallback
        return null;
    }

    private static Expression RemoveConversion(Expression expr)
    {
        // Sometimes value types are boxed so there is a Convert node we can skip.
        if (expr is UnaryExpression unary && expr.NodeType == ExpressionType.Convert)
        {
            return RemoveConversion(unary.Operand);
        }
        return expr;
    }

    private static string SuggestNameFromMethodCall(MethodCallExpression call)
    {
        // When the source of an aggregate is a .Where() call (e.g. e.Where(f => f.FlowType == "Capital Call").Sum(f => f.Amount)),
        // extract the filter literal value as the series name instead of the aggregate property name.
        var filterLiterals = ExtractWhereFilterLiterals(call);
        if (filterLiterals.Count > 0)
        {
            return string.Join(" ", filterLiterals.Select(StringHelper.ToTitleCase));
        }

        // Example: for a call like e.Sum(f => f.X to f.Y)
        // we check if one of the arguments is a lambda that produces two member accesses.
        foreach (var arg in call.Arguments)
        {
            var lambda = GetLambda(arg);
            if (lambda != null)
            {
                // Remove conversion from the lambda’s body
                var lambdaBody = RemoveConversion(lambda.Body);
                // If the lambda body is a binary expression, try to extract two member names.
                if (lambdaBody is BinaryExpression binary)
                {
                    string left = GetMemberName(binary.Left)!;
                    string right = GetMemberName(binary.Right)!;
                    if (!string.IsNullOrEmpty(left) && !string.IsNullOrEmpty(right))
                    {
                        return $"{left}And{right}";
                    }
                }
                // If the lambda body is a simple member access (e.g. q.Sum(x => x.Speed)),
                // return the member name instead of the aggregation method name.
                var memberName = GetMemberName(lambdaBody);
                if (!string.IsNullOrEmpty(memberName))
                {
                    return memberName;
                }
            }
        }
        // Fallback: return the method name itself.
        return call.Method.Name;
    }

    /// <summary>
    /// Walks the expression tree to find .Where() calls and extracts string literal values
    /// from equality comparisons (e.g. f => f.FlowType == "Capital Call" yields "Capital Call").
    /// </summary>
    private static List<string> ExtractWhereFilterLiterals(MethodCallExpression call)
    {
        var literals = new List<string>();

        // For extension methods, Arguments[0] is the source expression.
        // Walk the chain looking for .Where() calls.
        var current = call.Arguments.Count > 0 ? call.Arguments[0] : null;

        while (current is MethodCallExpression sourceCall)
        {
            if (sourceCall.Method.Name == "Where")
            {
                foreach (var arg in sourceCall.Arguments)
                {
                    var lambda = GetLambda(arg);
                    if (lambda == null) continue;

                    var body = RemoveConversion(lambda.Body);
                    if (body is BinaryExpression { NodeType: ExpressionType.Equal } eq)
                    {
                        var literal = GetStringConstant(eq.Right) ?? GetStringConstant(eq.Left);
                        if (literal != null)
                            literals.Add(literal);
                    }
                }
            }

            // Continue walking the chain (e.g. .Where().Where().Sum())
            current = sourceCall.Arguments.Count > 0 ? sourceCall.Arguments[0] : null;
        }

        return literals;
    }

    private static string? GetStringConstant(Expression expr)
    {
        expr = RemoveConversion(expr);
        if (expr is ConstantExpression { Value: string s })
            return s;
        return null;
    }

    private static LambdaExpression? GetLambda(Expression expr)
    {
        if (expr is LambdaExpression lambda)
        {
            return lambda;
        }
        if (expr is UnaryExpression unary)
        {
            return GetLambda(unary.Operand);
        }
        return null;
    }

    private static string? GetMemberName(Expression expr)
    {
        expr = RemoveConversion(expr);
        if (expr is MemberExpression member)
        {
            return member.Member.Name;
        }
        return null;
    }
}