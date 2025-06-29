using Ivy.Core;
using Ivy.Core.Hooks;

namespace Ivy.Views;

public class TaskView<T>(Task<T> task) : ViewBase
{
    public override object? Build()
    {
        var taskResult = UseState((object?)"Loading...");

        UseEffect(async () =>
        {
            await task;
            taskResult.Set(task.Result);
        });

        return taskResult.Value;
    }
}

public static class TaskViewFactory
{
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