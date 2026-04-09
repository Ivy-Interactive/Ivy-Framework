using System.Collections.Immutable;

namespace Ivy.Demo.Todo;

public record Todo(Guid Id, string Title, bool Done);

public enum TodoFilter { All, Active, Completed }

[App(icon: Icons.ListChecks, title: "Todos")]
public class TodoApp : ViewBase
{
    public override object? Build()
    {
        var todos = UseState(ImmutableArray.Create<Todo>());
        var newTitle = UseState("");
        var filter = UseState(TodoFilter.All);

        var filtered = filter.Value switch
        {
            TodoFilter.Active    => todos.Value.Where(t => !t.Done).ToArray(),
            TodoFilter.Completed => todos.Value.Where(t => t.Done).ToArray(),
            _                    => todos.Value.ToArray()
        };

        var activeCount = todos.Value.Count(t => !t.Done);
        var hasCompleted = todos.Value.Any(t => t.Done);

        void AddTodo()
        {
            var title = newTitle.Value.Trim();
            if (string.IsNullOrEmpty(title)) return;
            todos.Set(todos.Value.Add(new Todo(Guid.NewGuid(), title, false)));
            newTitle.Set("");
        }

        return Layout.Vertical(
            new Card(
                Layout.Vertical(
                    // Input row
                    Layout.Horizontal(
                        newTitle.ToTextInput(placeholder: "What needs to be done?")
                            .Width(Size.Grow()),
                        new Button("Add", onClick: _ => AddTodo())
                            .Icon(Icons.Plus)
                            .Variant(ButtonVariant.Primary)
                    ).Width(Size.Full()),

                    // Filter tabs
                    Layout.Horizontal(
                        new Button("All", _ => filter.Set(TodoFilter.All))
                            .Variant(filter.Value == TodoFilter.All ? ButtonVariant.Primary : ButtonVariant.Outline),
                        new Button("Active", _ => filter.Set(TodoFilter.Active))
                            .Variant(filter.Value == TodoFilter.Active ? ButtonVariant.Primary : ButtonVariant.Outline),
                        new Button("Completed", _ => filter.Set(TodoFilter.Completed))
                            .Variant(filter.Value == TodoFilter.Completed ? ButtonVariant.Primary : ButtonVariant.Outline)
                    ),

                    // Todo list
                    filtered.Length == 0
                        ? (object)Text.Muted(
                            filter.Value == TodoFilter.Completed
                                ? "No completed todos yet."
                                : filter.Value == TodoFilter.Active
                                    ? "Nothing left to do!"
                                    : "Add your first todo above."
                          ).Italic()
                        : Layout.Vertical(
                            filtered.Select(todo => new TodoItemView(
                                todo,
                                onDelete: () => todos.Set(todos.Value.RemoveAll(t => t.Id == todo.Id)),
                                onToggle: () => todos.Set(todos.Value.Replace(
                                    todos.Value.First(t => t.Id == todo.Id),
                                    todo with { Done = !todo.Done }
                                ))
                            ))
                          ),

                    // Footer
                    todos.Value.Length > 0
                        ? (object)Layout.Horizontal(
                            Text.Muted($"{activeCount} item{(activeCount == 1 ? "" : "s")} left").Width(Size.Grow()),
                            hasCompleted
                                ? new Button("Clear completed", _ =>
                                    todos.Set(todos.Value.RemoveAll(t => t.Done)))
                                    .Variant(ButtonVariant.Ghost)
                                : null
                          ).AlignContent(Align.Center).Width(Size.Full())
                        : null
                ).Gap(2)
            ).Title("Todo App").Width(Size.Fraction(1 / 2f))
        ).AlignContent(Align.Center).Padding(8);
    }
}

public class TodoItemView(Todo todo, Action onDelete, Action onToggle) : ViewBase
{
    public override object? Build()
    {
        var done = UseState(todo.Done);

        UseEffect((Action)(() =>
        {
            if (done.Value != todo.Done) onToggle();
        }), done);

        return Layout.Vertical(
            Layout.Horizontal(
                done.ToBoolInput(),
                todo.Done
                    ? Text.Muted(todo.Title).StrikeThrough().Width(Size.Grow())
                    : Text.Literal(todo.Title).Width(Size.Grow()),
                new Button(null, _ => onDelete())
                    .Icon(Icons.Trash)
                    .Variant(ButtonVariant.Ghost)
            ).AlignContent(Align.Center).Width(Size.Full()),
            new Separator()
        );
    }
}
