using System.Linq.Expressions;

namespace Ivy.Core.Helpers;

public static class ConversionDelegateFactory
{
    public static object CreateConversionDelegate(Type fromType, Type toType)
    {
        var param = Expression.Parameter(fromType, "v");
        var underlyingType = Nullable.GetUnderlyingType(toType) ?? toType;

        var body = CreateConversionExpression(fromType, toType, underlyingType, param);
        return Expression.Lambda(body, param).Compile();
    }

    private static Expression CreateConversionExpression(Type fromType, Type toType, Type underlyingType, ParameterExpression param)
    {
        // Handle conversion to/from object type
        if (fromType == typeof(object) || toType == typeof(object))
        {
            return Expression.Convert(param, toType);
        }

        // Handle string conversions
        if (fromType == typeof(string))
        {
            return CreateStringToTypeExpression(toType, underlyingType, param);
        }

        if (underlyingType == typeof(Guid))
        {
            return CreateGuidConversionExpression(fromType, toType, param);
        }

        if (underlyingType.IsEnum)
        {
            return CreateEnumConversionExpression(toType, underlyingType, param);
        }

        return CreateDefaultConversionExpression(toType, underlyingType, param);
    }

    private static Expression CreateStringToTypeExpression(Type toType, Type underlyingType, ParameterExpression param)
    {
        Expression parseExpr;
        if (underlyingType == typeof(Guid))
        {
            parseExpr = Expression.Call(typeof(Guid), nameof(Guid.Parse), Type.EmptyTypes, param);
        }
        else if (underlyingType == typeof(DateTime))
        {
            parseExpr = Expression.Call(typeof(DateTime), nameof(DateTime.Parse), Type.EmptyTypes, param);
        }
        else if (underlyingType == typeof(DateTimeOffset))
        {
            parseExpr = Expression.Call(typeof(DateTimeOffset), nameof(DateTimeOffset.Parse), Type.EmptyTypes, param);
        }
        else if (underlyingType == typeof(DateOnly))
        {
            parseExpr = Expression.Call(typeof(DateOnly), nameof(DateOnly.Parse), Type.EmptyTypes, param);
        }
        else if (underlyingType == typeof(TimeOnly))
        {
            parseExpr = Expression.Call(typeof(TimeOnly), nameof(TimeOnly.Parse), Type.EmptyTypes, param);
        }
        else if (underlyingType == typeof(byte[]))
        {
            parseExpr = Expression.Call(typeof(Convert), nameof(Convert.FromBase64String), Type.EmptyTypes, param);
        }
        else if (underlyingType.IsEnum)
        {
            parseExpr = Expression.Call(typeof(Enum), nameof(Enum.Parse), [underlyingType], param, Expression.Constant(true));
        }
        else
        {
            return Expression.Convert(
                Expression.Call(
                    typeof(Convert).GetMethod(nameof(System.Convert.ChangeType), [typeof(object), typeof(Type)])!,
                    Expression.Convert(param, typeof(object)),
                    Expression.Constant(underlyingType)),
                toType);
        }

        var throwExpr = Expression.Throw(
            Expression.New(
                typeof(InvalidCastException).GetConstructor([typeof(string)])!,
                Expression.Constant($"Cannot convert string to {toType.Name}")),
            toType);

        var convertExpr = Expression.Convert(parseExpr, toType);

        return Expression.Condition(
            Expression.Equal(param, Expression.Constant(null, typeof(string))),
            Nullable.GetUnderlyingType(toType) != null
                ? Expression.Constant(null, toType)
                : throwExpr,
            convertExpr);
    }

    private static Expression CreateGuidConversionExpression(Type fromType, Type toType, ParameterExpression param)
    {
        if (fromType == typeof(Guid) || fromType == typeof(Guid?))
        {
            return Expression.Convert(param, toType);
        }

        return Expression.Convert(
            Expression.Call(typeof(Guid), nameof(Guid.Parse), Type.EmptyTypes, Expression.Convert(param, typeof(string))),
            toType);
    }

    private static Expression CreateEnumConversionExpression(Type toType, Type underlyingType, ParameterExpression param)
    {
        return Expression.Convert(
            Expression.Call(typeof(Enum), nameof(Enum.Parse), [underlyingType],
                Expression.Convert(param, typeof(string)), Expression.Constant(true)),
            toType);
    }

    private static Expression CreateDefaultConversionExpression(Type toType, Type underlyingType, ParameterExpression param)
    {
        return Expression.Convert(
            Expression.Call(
                typeof(Convert).GetMethod(nameof(System.Convert.ChangeType), [typeof(object), typeof(Type)])!,
                Expression.Convert(param, typeof(object)),
                Expression.Constant(underlyingType)),
            toType);
    }
}