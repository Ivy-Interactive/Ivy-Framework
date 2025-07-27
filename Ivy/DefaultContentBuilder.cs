using System.Collections;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Ivy.Apps;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Helpers;
using Ivy.Shared;
using Ivy.Views;
using Ivy.Views.Tables;

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
            return new ErrorView(e);
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

        if (Utils.IsObservable(content))
        {
            return ObservableViewFactory.FromObservable(content);
        }

        if (content is FuncBuilder funcBuilder)
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

        if (content is IEnumerable enumerable)
        {
            //todo: zero items? 
            //todo: one items?
            return TableBuilderFactory.FromEnumerable(enumerable);
        }

        return new TextBlock(content.ToString()!, TextVariant.Block);
    }
}

