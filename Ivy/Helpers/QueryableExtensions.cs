using System.Linq.Expressions;
using System.Reflection;

namespace Ivy.Helpers;

public static class QueryableExtensions
{
    public static IQueryable RemoveFields<TModel>(this IQueryable<TModel> queryable, string[] fields)
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
                return Expression.MakeMemberAccess(parameter, prop!);
            }).ToArray();

            newExpression = Expression.New(bestConstructor, ctorArgs);
        }
        else
        {
            // Fallback to default constructor with member initialization
            var bindings = availableProperties.Select(prop =>
                Expression.Bind(prop, Expression.MakeMemberAccess(parameter, prop))
            ).ToList();

            newExpression = Expression.MemberInit(Expression.New(type), bindings);
        }

        var lambda = Expression.Lambda<Func<TModel, TModel>>(newExpression, parameter);
        return queryable.Select(lambda);
    }
}