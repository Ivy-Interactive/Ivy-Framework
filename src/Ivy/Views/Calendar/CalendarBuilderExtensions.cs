using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class CalendarBuilderExtensions
{
    public static CalendarBuilder<TModel> ToCalendar<TModel>(
        this IEnumerable<TModel> records,
        Expression<Func<TModel, DateTime>> startSelector,
        Expression<Func<TModel, DateTime>> endSelector,
        Expression<Func<TModel, object?>>? eventIdSelector = null)
    {
        return new CalendarBuilder<TModel>(
            records,
            startSelector,
            endSelector,
            eventIdSelector);
    }
}
