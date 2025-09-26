using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Views;

/// <summary>
/// A reactive view that displays task execution results with automatic loading
/// state management.
/// </summary>
/// <typeparam name="T">The type of result returned by the task.</typeparam>
public class TaskView<T>(Task<T> task) : ViewBase
{
    /// <summary>
    /// Builds the reactive view by managing task execution state and
    /// displaying the result when the task completes.
    /// </summary>
    /// <returns>The task result when completed, or "Loading..." while the task is running.</returns>
    public override object? Build()
    {
        var taskResult = UseState((object?)"Loading...");

        UseEffect(async () =>
        {
            await task;
            taskResult.Set(task.Result!);
        });

        return taskResult.Value;
    }
}

/// <summary>
/// Factory class for creating TaskView instances from untyped Task objects.
/// </summary>
public static class TaskViewFactory
{
    /// <summary>
    /// Creates a TaskView instance from an untyped Task object.
    /// </summary>
    /// <param name="task">The Task object to create a view from.</param>
    /// <returns>A ViewBase instance that wraps the task and provides reactive updates.</returns>
    public static ViewBase FromTask(Task task)
    {
        var taskType = task.GetType();
        if (!taskType.IsGenericType) return new TaskView<object>((Task<object>)task);
        var resultType = taskType.GetGenericArguments()[0];
        var taskViewType = typeof(TaskView<>).MakeGenericType(resultType);
        var taskViewInstance = Activator
            .CreateInstance(taskViewType, [task]);
        return (ViewBase)taskViewInstance!;
    }
}