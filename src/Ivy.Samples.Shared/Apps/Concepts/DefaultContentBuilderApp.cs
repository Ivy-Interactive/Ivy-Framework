using Ivy.Views;
using Ivy.Core.Hooks;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Paintbrush, searchHints: ["rendering", "display", "types", "conversion", "formatting", "output"])]
public class DefaultContentBuilderApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical().Gap(8)
            | Text.H3("Primitive Types")
            | Layout.Vertical(
                null!,
                "HelloWorld",
                123_456.78,
                false,
                true,
                DateTime.Now,
                new int[] { 1, 2, 3, 4 },
                new List<int> { 1, 2, 3, 4 },
                new string[] { "a", "b", "c" },
                new List<string> { "a", "b", "c" }
            )
            | Text.H3("Extension Method Build")
            | Text.Muted("Types with a Build(this T, IViewContext) extension method:")
            | new Person("Alice", 30, "alice@example.com")
            | new Person("Bob", 25, "bob@example.com")
            | Text.H3("Instance Method Build")
            | Text.Muted("Types with a Build(IViewContext) instance method:")
            | new StatusInfo("Connected", true)
            | new StatusInfo("Disconnected", false)
            | Text.H3("Generic Type Build")
            | Text.Muted("Generic types with Build<T>(this Container<T>, IViewContext) extension:")
            | new Wrapper<string>("Hello World")
            | new Wrapper<int>(42);
    }
}

// Extension method example
public record Person(string Name, int Age, string Email);

public static class PersonExtensions
{
    public static object? Build(this Person person, IViewContext context)
    {
        return new Card(
            Layout.Vertical().Gap(2)
            | Text.Label(person.Name)
            | Layout.Horizontal().Gap(4)
            | new Badge($"Age: {person.Age}")
            | Text.P(person.Email).Small()
        );
    }
}

// Instance method example
public class StatusInfo(string label, bool isActive)
{
    public object? Build(IViewContext context)
    {
        return Layout.Horizontal().Gap(2)
            | new Icon(isActive ? Icons.CircleCheck : Icons.CircleX)
                .Color(isActive ? Colors.Success : Colors.Destructive)
            | Text.P(label);
    }
}

// Generic type example
public record Wrapper<T>(T Value);

public static class WrapperExtensions
{
    public static object? Build<T>(this Wrapper<T> wrapper, IViewContext context)
    {
        return Layout.Horizontal().Gap(2)
            | Text.Muted($"Wrapper<{typeof(T).Name}>:")
            | new Badge(wrapper.Value?.ToString() ?? "null");
    }
}