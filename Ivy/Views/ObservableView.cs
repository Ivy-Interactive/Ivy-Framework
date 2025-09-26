using System.Reactive.Linq;
using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Views;

/// <summary>
/// A reactive view that automatically updates its content based on an observable
/// stream.
/// </summary>
/// <typeparam name="T">The type of data emitted by the observable stream.</typeparam>
public class ObservableView<T>(IObservable<T> observable) : ViewBase
{
    /// <summary>
    /// Builds the reactive view by subscribing to the observable stream and
    /// returning the most recently observed value.
    /// </summary>
    /// <returns>The most recently observed value from the observable stream, or null if no value has been emitted yet.</returns>
    public override object? Build()
    {
        var lastObserved = this.UseState((object?)null);

        this.UseEffect(() =>
        {
            return observable.Subscribe(e => lastObserved.Set(e!));
        });

        return lastObserved.Value;
    }
}

/// <summary>
/// Factory class for creating ObservableView instances from untyped observable objects.
/// </summary>
public static class ObservableViewFactory
{
    /// <summary>
    /// Creates an ObservableView instance from an untyped observable object.
    /// </summary>
    /// <param name="observable">The observable object to create a view from. Must implement IObservable&lt;T&gt;.</param>
    /// <returns>A ViewBase instance that wraps the observable and provides reactive updates.</returns>
    /// <exception cref="Exception">Thrown when the provided object does not implement IObservable&lt;T&gt;.</exception>
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