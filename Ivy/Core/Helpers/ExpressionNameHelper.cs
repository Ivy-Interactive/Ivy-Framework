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
            }
        }
        // Fallback: return the method name itself.
        return call.Method.Name;
    }

    /// <summary>
    /// If the expression is (or contains) a lambda, return it.
    /// </summary>
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

    /// <summary>
    /// If the expression represents a member access (or a converted member access), return the member name.
    /// </summary>
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