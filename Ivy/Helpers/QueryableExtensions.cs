using System.Linq.Expressions;
using System.Reflection;

namespace Ivy.Helpers;

public static class QueryableExtensions
{
    public static IQueryable RemoveFields<TModel>(this IQueryable<TModel> queryable, string[] fields)
    {
        var type = typeof(TModel);
        var parameter = Expression.Parameter(type, "x");
    
        var availableMembers = type.GetProperties()
            .Cast<MemberInfo>()
            .Union(type.GetFields().Cast<MemberInfo>())
            .Where(m => !fields.Contains(m.Name))
            .ToList();

        var bindings = availableMembers.Select(member =>
            Expression.Bind(member, Expression.MakeMemberAccess(parameter, member))
        ).ToList();

        var memberInit = Expression.MemberInit(Expression.New(type), bindings);
        var lambda = Expression.Lambda<Func<TModel, TModel>>(memberInit, parameter);

        return queryable.Select(lambda);
    }
}