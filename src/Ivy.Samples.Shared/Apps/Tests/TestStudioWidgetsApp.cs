using Ivy.Shared;
using Ivy.Views;
using Ivy.Widgets.Inputs;

namespace Ivy.Samples.Shared.Apps.Tests;

public class SingleAnswerQuestion(string question, Action<bool>? onAnswer = null, string yesText = "Yes", string noText = "No") : ViewBase
{
    public override object? Build()
    {
        var buttons = Layout.Horizontal().Gap(2)
            | new Button(yesText, () => onAnswer?.Invoke(true)).Small().Icon(Icons.Waypoints)
            | new Button(noText, () => onAnswer?.Invoke(false)).Outline().Small();

        return new ChatBox(question, buttons);
    }
}

public class MultipleAnswerQuestion<T>(string question, IEnumerable<IAnyOption> options, Action<T>? onAnswer = null) : ViewBase
{
    public override object? Build()
    {
        var selected = UseState<T>();

        UseEffect(() =>
        {
            if (selected.Value != null)
                onAnswer?.Invoke(selected.Value);
        }, selected);

        var radioList = selected.ToSelectInput(options).List();

        return new ChatBox(question, radioList);
    }
}

[App(icon: Icons.MessageSquare, path: ["Tests"])]
public class TestStudioWidgetsApp : SampleBase
{
    protected override object? BuildSample()
    {
        var name = UseState("");
        var answer = UseState((bool?)null);
        var selectedDb = UseState<string>();

        var dbOptions = new[]
        {
            new Option<string>("PostgreSQL", "postgres"),
            new Option<string>("MySQL", "mysql"),
            new Option<string>("SQLite", "sqlite"),
            new Option<string>("SQL Server", "mssql"),
        };

        return Layout.Vertical().Gap(6)
            | Text.H1("Studio Widgets")

            | Text.H2("FloatingBox")
            | Text.P("A subtle dark-tinted container with transparent border.")
            | new FloatingBox(
                Layout.Horizontal().Gap(1).Align(Align.Center)
                | (Layout.Horizontal().Gap(1).Grow()
                    | Icons.Citrus.ToButton().BorderRadius(BorderRadius.Full)
                    | Icons.Waypoints.ToButton().BorderRadius(BorderRadius.Full).Ghost()
                    | Icons.Code.ToButton().BorderRadius(BorderRadius.Full).Ghost())
                | (Layout.Horizontal().Gap(2).Align(Align.Right)
                    | new Button("Action")
                    | new Avatar("JD")))

            | Text.H2("ChatBox")
            | Text.P("ChatBox is a reusable container with a title and custom content.")

            | Text.H3("With Text Input")
            | new ChatBox(
                "What is your name?",
                name.ToTextInput().Placeholder("Enter your name..."))

            | Text.H3("With Custom Buttons")
            | new ChatBox(
                "Choose an action:",
                Layout.Horizontal().Gap(2)
                | new Button("Save", () => { }).Primary().Small()
                | new Button("Export", () => { }).Secondary().Small()
                | new Button("Delete", () => { }).Destructive().Small())

            | Text.H3("Without Title")
            | new ChatBox(
                content: Layout.Vertical().Gap(2)
                | Text.P("This ChatBox has no title, only content.")
                | new Button("Got it!", () => { }).Outline().Small())

            | Text.H3("SingleAnswerQuestion")
            | new SingleAnswerQuestion(
                "Create Database Connection?",
                value => answer.Set(value),
                yesText: "Connect",
                noText: "Cancel")
            | (answer.Value != null
                ? Text.Label($"You answered: {(answer.Value == true ? "Connect" : "Cancel")}")
                : null)

            | Text.H3("MultipleAnswerQuestion")
            | new MultipleAnswerQuestion<string>(
                "Which database do you want to use?",
                dbOptions,
                value => selectedDb.Set(value))
            | (selectedDb.Value != null
                ? Text.Label($"Selected: {selectedDb.Value}")
                : null);
    }
}
