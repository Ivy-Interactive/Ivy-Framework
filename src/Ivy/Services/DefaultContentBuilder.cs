using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Ivy.Core;
using Ivy.Core.Helpers;

// Resharper disable once CheckNamespace
namespace Ivy;

public class ContentBuilder : IContentBuilder
{
    private readonly List<IContentBuilder> _middlewares = new();

    public ContentBuilder Use(IContentBuilder middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }

    public bool CanHandle(object? content) => true;

    public object? Format(object? content)
    {
        foreach (var mw in _middlewares.Where(mw => mw.CanHandle(content)))
        {
            return mw.Format(content);
        }
        return new DefaultContentBuilder().Format(content);
    }
}

public class DefaultContentBuilder : IContentBuilder
{
    private static readonly ConcurrentDictionary<Type, MethodInfo?> BuildMethodCache = new();

    private string IntegerFormat { get; set; } = "N0";

    private string DecimalFormat { get; set; } = "#,#.##";

    private string DateFormat { get; set; } = "yyyy-MM-dd";

    public bool CanHandle(object? content) => true;

    public object? Format(object? content)
    {
        if (content is null)
        {
            return new Empty();
        }

        if (content is Exception e)
        {
            return new ExceptionErrorView(e);
        }

        if (content is IWidget widget)
        {
            return widget;
        }

        if (content is IView view)
        {
            return view;
        }

        if (content is IAnyState state)
        {
            return Format(state.As<object>().Value);
        }

        if (content is Icons icon)
        {
            return icon.ToIcon();
        }

        if (content is Task task)
        {
            return TaskViewFactory.FromTask(task);
        }

        if (TypeHelper.IsObservable(content))
        {
            return ObservableViewFactory.FromObservable(content);
        }

        if (content is FuncViewBuilder funcBuilder)
        {
            return new FuncView(funcBuilder);
        }

        if (content is Func<object> factory1)
        {
            return factory1();
        }

        if (content is JsonNode jsonNode)
        {
            return new Json(jsonNode);
        }

        if (content is XObject xObject)
        {
            return new Xml(xObject);
        }

        if (content is bool boolContent)
        {
            return boolContent ? new Icon(Icons.Check).Color(Colors.Primary) : new Icon(Icons.None);
        }

        if (content is string stringContent)
        {
            return new TextBlock(stringContent, TextVariant.Block);
        }

        if (content is long longContent)
        {
            return new TextBlock(longContent.ToString(IntegerFormat), TextVariant.Block);
        }

        if (content is int intContent)
        {
            return new TextBlock(intContent.ToString(IntegerFormat), TextVariant.Block);
        }

        if (content is double doubleContent)
        {
            return new TextBlock(doubleContent.ToString(DecimalFormat).TrimEnd(".00"), TextVariant.Block); //todo:
        }

        if (content is decimal decimalContent)
        {
            return new TextBlock(decimalContent.ToString(DecimalFormat).TrimEnd(".00"), TextVariant.Block);
        }

        if (content is DateTime dateContent)
        {
            return new TextBlock(dateContent.ToString(DateFormat), TextVariant.Block);
        }

        if (content is DateTimeOffset dateTimeOffset)
        {
            return new TextBlock(dateTimeOffset.ToString(DateFormat), TextVariant.Block);
        }

        if (content is DateOnly dateOnly)
        {
            return new TextBlock(dateOnly.ToString(DateFormat), TextVariant.Block);
        }

        if (content is object[] array)
        {
            return new Fragment(array);
        }

        if (content is IEnumerable enumerable)
        {
            //todo: zero items?
            //todo: one items?
            return TableBuilderFactory.FromEnumerable(enumerable);
        }

        // Check if type has a Build method or extension that takes IViewContext and returns object?
        var buildMethod = GetBuildMethod(content.GetType());
        if (buildMethod is not null)
        {
            return new FuncView(context => buildMethod.Invoke(
                buildMethod.IsStatic ? null : content,
                buildMethod.IsStatic ? [content, context] : [context]));
        }

        return new TextBlock(content.ToString()!, TextVariant.Block);
    }

    private static MethodInfo? GetBuildMethod(Type type)
    {
        return BuildMethodCache.GetOrAdd(type, t =>
        {
            // Check for instance method: Build(IViewContext) -> object?
            var instanceMethod = t.GetMethod(
                "Build",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                [typeof(IViewContext)],
                null);

            if (instanceMethod is not null && typeof(object).IsAssignableFrom(instanceMethod.ReturnType))
            {
                return instanceMethod;
            }

            // Check for extension method: Build(this T, IViewContext) -> object?
            return FindExtensionMethod(t);
        });
    }

    private static MethodInfo? FindExtensionMethod(Type targetType)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                foreach (var extensionType in assembly.GetLoadableTypes()
                    .Where(t => t is { IsSealed: true, IsGenericType: false, IsNested: false, IsAbstract: true }))
                {
                    foreach (var method in extensionType.GetMethods(BindingFlags.Static | BindingFlags.Public))
                    {
                        if (method.Name != "Build" || !method.IsDefined(typeof(ExtensionAttribute), false))
                        {
                            continue;
                        }

                        var parameters = method.GetParameters();
                        if (parameters.Length != 2)
                        {
                            continue;
                        }

                        var firstParam = parameters[0].ParameterType;
                        var secondParam = parameters[1].ParameterType;

                        // Handle generic extension methods like Build<TValue>(this QueryResult<TValue>, IViewContext)
                        if (method.IsGenericMethodDefinition)
                        {
                            if (targetType.IsGenericType &&
                                firstParam.IsGenericType &&
                                firstParam.GetGenericTypeDefinition() == targetType.GetGenericTypeDefinition() &&
                                secondParam == typeof(IViewContext))
                            {
                                try
                                {
                                    var genericArgs = targetType.GetGenericArguments();
                                    return method.MakeGenericMethod(genericArgs);
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                        }
                        else if (firstParam.IsAssignableFrom(targetType) && secondParam == typeof(IViewContext))
                        {
                            return method;
                        }
                    }
                }
            }
            catch
            {
                // Some assemblies may not be accessible
            }
        }

        return null;
    }
}

