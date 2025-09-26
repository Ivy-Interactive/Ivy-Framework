# Tasks and Observables

<Ingress>
Handle asynchronous operations and reactive data streams with Tasks and Observables for responsive application behavior.
</Ingress>

Ivy provides powerful abstractions for working with asynchronous operations and reactive data streams. **Tasks** handle one-time asynchronous operations, while **Observables** manage continuous data streams that automatically update the UI when data changes.

## Basic Task Usage

Tasks represent asynchronous operations that complete once and return a result. Ivy provides `TaskView<T>` to automatically handle loading states and display results.

```csharp demo-below
public class TaskExample : ViewBase
{
    public override object? Build()
    {
        var task = Task.Run(async () =>
        {
            await Task.Delay(2000); 
            return "Task completed successfully!";
        });

        return new TaskView<string>(task);
    }
}
```

## Basic Observable Usage

Ivy's `ObservableView<T>` automatically subscribes and updates the UI as new values arrive. This example shows how to create a simple observable that emits the current time every second.

```csharp demo-below
public class TimeBasedObservableExample : ViewBase
{
    public override object? Build()
    {
        var isActive = this.UseState<bool>(false);

        var timeObservable = this.UseStatic(() =>
            Observable.Interval(TimeSpan.FromMilliseconds(500))
                .Where(_ => isActive.Value)
                .Select(_ => DateTime.Now.ToString("HH:mm:ss.fff"))
        );

        return Layout.Vertical(
            Layout.Horizontal(
                new Button("Start", _ => isActive.Value = true),
                new Button("Stop", _ => isActive.Value = false)
            ),
            Text.Block("Time Updates:"),
            new ObservableView<string>(timeObservable)
        );
    }
}
```

### Observable with State Management

This example demonstrates how to properly manage state with observables by controlling when subscriptions are active. It shows a timer-based counter that only increments when a state flag is active, with proper subscription cleanup.

```csharp demo-tabs
public class StateManagementExample : ViewBase
{
    public override object? Build()
    {
        var counter = this.UseState<int>(0);
        var isRunning = this.UseState<bool>(false);
        
        var timerObservable = this.UseStatic(() => 
            Observable.Interval(TimeSpan.FromSeconds(1))
        );

        this.UseEffect(() =>
        {
            var subscription = timerObservable.Subscribe(_ =>
            {
                if (isRunning.Value)
                {
                    counter.Set(prev => prev + 1);
                }
            });
            return subscription;
        }); 

        return Layout.Vertical(
            Layout.Horizontal(
                new Button("Start", _ => isRunning.Value = true),
                new Button("Stop", _ => isRunning.Value = false),
                new Button("Reset", _ => counter.Value = 0)
            ),
            Text.Block($"Counter: {counter.Value}"),
            Text.Block($"Status: {(isRunning.Value ? "Running" : "Stopped")}")
        );
    }
}
```

### Observable with Throttling

This example demonstrates how to use observables for search functionality with performance optimizations. It shows throttled updates to prevent excessive filtering while typing, and proper state management to avoid duplicate data.

```csharp demo-tabs
public class ObservableSearchExample : ViewBase
{
    public override object? Build()
    {
        var inputText = this.UseState<string>("");
        var originalItems = this.UseState<string[]>(new[] { "Apple", "Banana", "Cherry", "Date", "Elderberry", "Fig", "Grape", "Honeydew" });
        var filteredItems = this.UseState<string[]>(Array.Empty<string>());
        
        this.UseEffect(() => {
            filteredItems.Set(originalItems.Value);
        }, []); 
        
        var searchObservable = this.UseStatic(() => 
            Observable.Interval(TimeSpan.FromMilliseconds(300))
                .Select(_ => inputText.Value)
                .DistinctUntilChanged()
        );

        this.UseEffect(() =>
        {
            return searchObservable.Subscribe(searchTerm =>
            {   
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    filteredItems.Set(originalItems.Value);
                }
                else
                {
                    var filtered = originalItems.Value
                        .Where(item => item.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                    filteredItems.Set(filtered);
                }
            });
        });

        return Layout.Vertical(
            Text.Block("Observable Search: "),
            Layout.Horizontal(
                new TextInput(inputText, placeholder: "Type to filter (throttled)..."),
                new Button("Clear", _ => inputText.Set(""))
            ),
            Text.Block($"Found {filteredItems.Value.Length} of {originalItems.Value.Length} items"),
            Layout.Vertical(
                filteredItems.Value.Select(item => 
                    Text.Block(item)
                )
            )
        );
    }
}
```

### Observable Transformations

This example demonstrates interactive data transformation with immediate feedback. It demonstrates filtering, projection, and limiting operations to create processed results.

```csharp demo-tabs
public class TransformationExample : ViewBase
{
    public override object? Build()
    {
        var generatedData = this.UseState<int[]>(Array.Empty<int>());
        var lastTransformed = this.UseState<int[]>(Array.Empty<int>());

        void GenerateNewData(Event<Button> _)
        {
            var random = new Random();
            var newData = Enumerable.Range(1, 10)
                .Select(_ => random.Next(1, 21))
                .ToArray();
            
            generatedData.Set(newData);
            
            var immediateResult = newData
                .Where(num => num % 2 == 0)
                .Select(num => num * 2)
                .Take(5)
                .ToArray();
            lastTransformed.Set(immediateResult);
        }

        return Layout.Vertical(
            new Button("Generate New Data", GenerateNewData),
            Text.Block("Generated data: " + string.Join(", ", generatedData.Value)),
            Text.Block("Last Generated Result: " + string.Join(", ", lastTransformed.Value))
        );
    }
}
```
