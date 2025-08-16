using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Demos;

public record Todo(string Title, bool Done);

[App(icon: Icons.Calendar, path: ["Demos"])]
public class TodosApp : SampleBase
{
    protected override object? BuildSample()
    {
        var newTitle = UseState("");
        var todos = UseState(ImmutableArray.Create<Todo>());
        var client = UseService<IClientProvider>();

        return Layout.Vertical(
            new Card(
                Layout.Vertical(
                    Layout.Horizontal(
                        newTitle.ToTextInput(placeholder: "New todo...").Width(Size.Grow()),
                        new Button("Add", onClick: _ =>
                            {
                                var title = newTitle.Value;
                                todos.Set(todos.Value.Add(new Todo(title, false)));
                                client.Toast($"New '{title}' todo added.", "Todos");
                                newTitle.Set("");
                            }
                        ).Icon(Icons.Plus).Variant(ButtonVariant.Primary)
                    ).Width(Size.Full()),
                    Layout.Vertical(
                        todos.Value.Select(todo => new TodoItem(todo,
                            () =>
                            {
                                todos.Set(todos.Value.Remove(todo));
                            },
                            () =>
                            {
                                todos.Set(todos.Value.Replace(todo, todo with
                                {
                                    Done = !todo.Done
                                }));
                            }
                        ))
                    )
                )
            ).Title("Todos").Width(1 / 2f)
        );
    }
}

public class TodoItem(Todo todo, Action deleteTodo, Action toggleTodo) : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical(
            Layout.Horizontal(
                new BoolInput<bool>(todo.Done, _ =>
                {
                    toggleTodo();
                }),
                todo.Done ? Text.Muted(todo.Title).StrikeThrough().Width(Size.Grow()) : Text.Literal(todo.Title).Width(Size.Grow()),
                new Button(null, _ =>
                {
                    deleteTodo();
                }).Icon(Icons.Trash).Variant(ButtonVariant.Outline)
            ).Align(Align.Center).Width(Size.Full()),
            new Separator()
        );
    }
}
