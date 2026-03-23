using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Ivy.Core.Helpers;

public static class QueryableExtensions
{
    public static IQueryable RemoveFields<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TModel>(this IQueryable<TModel> queryable, string[] fields)
    {
        // If no fields to remove, return as-is
        if (fields == null || fields.Length == 0)
        {
            return queryable;
        }

        var type = typeof(TModel);
        var parameter = Expression.Parameter(type, "x");

        // Get all properties that should be kept
        var availableProperties = type.GetProperties()
            .Where(p => !fields.Contains(p.Name))
            .ToList();

        // Try to find a constructor that matches the available properties
        var constructors = type.GetConstructors();
        ConstructorInfo? bestConstructor = null;

        // Look for a constructor that has parameters matching our properties
        foreach (var ctor in constructors)
        {
            var ctorParams = ctor.GetParameters();
            if (ctorParams.Length == availableProperties.Count)
            {
                // Check if all parameter names match property names (case-insensitive for records)
                var paramNames = ctorParams.Select(p => p.Name?.ToLowerInvariant()).ToList();
                var propNames = availableProperties.Select(p => p.Name.ToLowerInvariant()).ToList();

                if (paramNames.All(pn => propNames.Contains(pn ?? "")))
                {
                    bestConstructor = ctor;
                    break;
                }
            }
        }

        // If no exact match, try the primary constructor and fill removed params with defaults
        if (bestConstructor == null)
        {
            var primaryCtor = constructors
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault(c =>
                {
                    var ctorParams = c.GetParameters();
                    var propNames = availableProperties.Select(p => p.Name.ToLowerInvariant()).ToHashSet();
                    return ctorParams.Length > 0 && ctorParams.All(p => propNames.Contains(p.Name?.ToLowerInvariant() ?? "") || fields.Contains(p.Name!, StringComparer.OrdinalIgnoreCase));
                });

            if (primaryCtor != null)
            {
                bestConstructor = primaryCtor;
            }
        }

        Expression newExpression;

        if (bestConstructor != null)
        {
            // Use constructor with parameters (for records)
            var ctorParams = bestConstructor.GetParameters();
            var ctorArgs = ctorParams.Select(p =>
            {
                var prop = availableProperties.FirstOrDefault(
                    ap => ap.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase)
                );
                return prop != null
                    ? (Expression)Expression.MakeMemberAccess(parameter, prop)
                    : Expression.Default(p.ParameterType);
            }).ToArray();

            newExpression = Expression.New(bestConstructor, ctorArgs);
        }
        else
        {
            // Try to find the primary constructor (e.g., for positional records without a parameterless constructor)
            var primaryCtor = constructors
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault(c => c.GetParameters().Length > 0);

            if (primaryCtor != null && type.GetConstructor(Type.EmptyTypes) == null)
            {
                var ctorParams = primaryCtor.GetParameters();
                var ctorArgs = ctorParams.Select(p =>
                {
                    var prop = availableProperties.FirstOrDefault(
                        ap => ap.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase)
                    );
                    return prop != null
                        ? (Expression)Expression.MakeMemberAccess(parameter, prop)
                        : Expression.Default(p.ParameterType);
                }).ToArray();

                newExpression = Expression.New(primaryCtor, ctorArgs);
            }
            else
            {
                // Fallback to default constructor with member initialization
                var bindings = availableProperties.Select(prop =>
                    Expression.Bind(prop, Expression.MakeMemberAccess(parameter, prop))
                ).ToList();

                newExpression = Expression.MemberInit(Expression.New(type), bindings);
            }
        }

        var lambda = Expression.Lambda<Func<TModel, TModel>>(newExpression, parameter);
        return queryable.Select(lambda);
    }
}
