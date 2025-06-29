using System.Linq.Expressions;
using System.Reflection;
using Force.DeepCloner;
using Ivy.Core.Hooks;
#pragma warning disable CS8620

namespace Ivy.Core.Helpers;

public static class StateHelpers
{
    public static IState<TTo> Convert<TFrom, TTo>(this IState<TFrom?> state, Func<TFrom?, TTo> forward, Func<TTo, TFrom> backward)
    {
        return new ConvertedState<TFrom?, TTo>(state, forward, backward);
    }

    public static T DeepClone<T>(T source)
    {
        return source.DeepClone();
    }

    public static IState<T> As<T>(this IAnyState state)
    {
        if (state is IState<T> s) return s;

        var iStateInterface = state.GetType().GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IState<>));

        if (iStateInterface == null)
            throw new ArgumentException($"Unable to convert IAnyState to IState<{typeof(T).Name}>.");

        var sourceType = iStateInterface.GetGenericArguments()[0];
        var convertMethod = typeof(StateHelpers)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m is { Name: nameof(Convert), IsGenericMethodDefinition: true }
                        && m.GetParameters().Length == 3);

        var closedMethod = convertMethod.MakeGenericMethod(sourceType, typeof(T));
        var forward = ConversionDelegateFactory.CreateConversionDelegate(sourceType, typeof(T));
        var backward = ConversionDelegateFactory.CreateConversionDelegate(typeof(T), sourceType);

        var result = closedMethod.Invoke(null, [state, forward, backward])!;
        return (IState<T>)result;
    }

    private static void SetValue<TModel, TValue>(this IState<TModel> model, Expression<Func<TModel, TValue>> selector, TValue value)
    {
        var memberInfo = GetMemberInfo(selector.Body);
        switch (memberInfo)
        {
            case PropertyInfo property:
                property.SetValue(model.Value, Utils.BestGuessConvert(value, property.PropertyType));
                break;
            case FieldInfo field:
                field.SetValue(model.Value, Utils.BestGuessConvert(value, field.FieldType));
                break;
            default:
                throw new ArgumentException("Invalid expression.");
        }
    }

    private static TValue? GetValue<TModel, TValue>(this IState<TModel> model, Expression<Func<TModel, TValue>> selector)
    {
        var memberInfo = GetMemberInfo(selector.Body);
        if (memberInfo is PropertyInfo property)
        {
            return (TValue)property.GetValue(model.Value)!;
        }
        if (memberInfo is FieldInfo field)
        {
            return (TValue)field.GetValue(model.Value)!;
        }
        throw new ArgumentException("Invalid expression.");
    }

    private static MemberInfo GetMemberInfo(Expression expression)
    {
        while (true)
        {
            switch (expression)
            {
                case MemberExpression memberExpression:
                    return memberExpression.Member;
                case UnaryExpression unaryExpression:
                    expression = unaryExpression.Operand;
                    continue;
            }
            throw new ArgumentException("Invalid expression.");
        }
    }

    private static Type GetSelectorType<TModel>(Expression<Func<TModel, object>> selector)
    {
        var memberInfo = GetMemberInfo(selector.Body);
        var type = memberInfo switch
        {
            PropertyInfo property => property.PropertyType,
            FieldInfo field => field.FieldType,
            _ => throw new ArgumentException("Invalid expression.")
        };
        return type;
    }

    public static (IAnyState, IDisposable) MemberState<TModel>(IState<TModel> model, Expression<Func<TModel, object>> selector)
    {
        var selectorType = GetSelectorType(selector);
        var value = model.GetValue(selector)!;
        var stateType = typeof(State<>).MakeGenericType(selectorType);
        var derivedState = (IAnyState)Activator.CreateInstance(stateType, value)!;
        var disposable = derivedState.SubscribeAny(e => model.SetValue(selector, e));
        return (derivedState, disposable);
    }
}
