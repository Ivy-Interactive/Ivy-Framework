using System.Reactive.Linq;
using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Views;

public class ObservableView<T>(IObservable<T> observable) : ViewBase
{
    public override object? Build()
    {
        var lastObserved = this.UseState((object?)null);

        this.UseEffect(() =>
        {
            return observable.Subscribe(e => lastObserved.Set(e));
        });

        return lastObserved.Value;
    }
}

public static class ObservableViewFactory
{
    public static ViewBase FromObservable(object observable)
    {
        var observableType = observable
            .GetType()
            .GetInterfaces()
            .FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IObservable<>)
            );

        if (observableType == null) throw new Exception("Not an observable.");

        var resultType = observableType.GetGenericArguments()[0];
        var observableViewType = typeof(ObservableView<>).MakeGenericType(resultType);
        var observableViewInstance = Activator
            .CreateInstance(observableViewType, [observable]);
        return (ViewBase)observableViewInstance!;
    }
}