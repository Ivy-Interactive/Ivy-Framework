using System.Collections;
using System.Linq.Expressions;

namespace Ivy;

public static class TypeHelper
{
    public static Type? GetCollectionTypeParameter(this Type type)
    {
        // Handle arrays
        if (type.IsArray)
        {
            return type.GetElementType();
        }

        // Handle Dictionary separately if you want both key and value types
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            return typeof(KeyValuePair<,>).MakeGenericType(type.GetGenericArguments());
        }

        // Handle generic collections
        if (type.IsGenericType)
        {
            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Length == 1)
            {
                return genericArgs[0];
            }
        }

        // Try to infer from IEnumerable<T>
        var enumerableInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return enumerableInterface?.GetGenericArguments()[0];
    }

    public static bool IsCollectionType(this Type? type)
    {
        if (type == null) return false;
        if (type == typeof(string)) return false;

        // Handle arrays
        if (type.IsArray) return true;

        // Handle common generic collection types
        if (type.IsGenericType)
        {
            var genericTypeDef = type.GetGenericTypeDefinition();
            if (genericTypeDef == typeof(List<>) ||
                genericTypeDef == typeof(IList<>) ||
                genericTypeDef == typeof(IEnumerable<>) ||
                genericTypeDef == typeof(ICollection<>))
            {
                return true;
            }
        }

        // Handle non-generic collections like ArrayList, Hashtable, etc.
        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            return true;
        }

        return false;
    }

    public static bool IsNullable(this Type type)
    {
        if (type == null) { return false; }
        return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
    }

    public static bool IsNullableType(this Type type)
    {
        return Nullable.GetUnderlyingType(type) != null;
    }

    public static int SuggestPrecision(this Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GetGenericArguments()[0];
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return 4;
            default:
                return 0;
        }
    }

    public static double SuggestStep(this Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GetGenericArguments()[0];
        }

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Decimal => 0.01, // 2 decimal places for currency/decimal
            TypeCode.Double => 0.01,  // 2 decimal places for double
            TypeCode.Single => 0.01f, // 2 decimal places for float
            TypeCode.Int16 => 1.0,
            TypeCode.Int32 => 1.0,
            TypeCode.Int64 => 1.0,
            TypeCode.Byte => 1.0,
            _ => 1.0,
        };
    }

    public static double SuggestMin(this Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GetGenericArguments()[0];
        }

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Decimal => -999999999999.99, // Practical limit for currency/decimal values
            TypeCode.Double => -999999999999.99,  // Practical limit for double values
            TypeCode.Single => -999999999999.99f, // Practical limit for float values
            TypeCode.Int16 => short.MinValue,
            TypeCode.Int32 => int.MinValue,
            TypeCode.Int64 => long.MinValue,
            TypeCode.Byte => byte.MinValue,
            _ => 0.0,
        };
    }

    public static double SuggestMax(this Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GetGenericArguments()[0];
        }

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Decimal => 999999999999.99, // Practical limit for currency/decimal values
            TypeCode.Double => 999999999999.99,  // Practical limit for double values
            TypeCode.Single => 999999999999.99f, // Practical limit for float values
            TypeCode.Int16 => short.MaxValue,
            TypeCode.Int32 => int.MaxValue,
            TypeCode.Int64 => long.MaxValue,
            TypeCode.Byte => byte.MaxValue,
            _ => int.MaxValue,
        };
    }

    public static bool IsDate(this Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GetGenericArguments()[0];
        }

        return type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(DateOnly);
    }

    public static bool IsNumeric(this Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GetGenericArguments()[0];
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }

    public static bool IsSimpleType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsPrimitive
               || type.IsEnum
               || type == typeof(string)
               || IsNumeric(type)
               || type == typeof(DateTime)
               || type == typeof(DateTimeOffset)
               || type == typeof(TimeSpan)
               || type == typeof(Guid);
    }

    public static bool IsObservable(object observable)
    {
        return observable
            .GetType()
            .GetInterfaces()
            .Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IObservable<>)
            );
    }

    public static string GetNameFromMemberExpression(Expression expression)
    {
        while (true)
        {
            switch (expression)
            {
                case MemberExpression memberExpression:
                    return memberExpression.Member.Name;
                case UnaryExpression unaryExpression:
                    expression = unaryExpression.Operand;
                    continue;
                case MethodCallExpression methodCall when methodCall.Method.Name == "get_Item"
                    && methodCall.Arguments.Count == 1
                    && methodCall.Arguments[0] is ConstantExpression constant:
                    return constant.Value?.ToString() ?? throw new ArgumentException("Invalid expression.");
            }

            throw new ArgumentException("Invalid expression.");
        }
    }
}
