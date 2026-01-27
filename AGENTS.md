All Ivy documentation pages are listed on: <https://docs.ivy.app/sitemap.xml>.
Add ".md" to the end of any URL to go directly to the Markdown version of the doc.

# Introduction to the Ivy Framework for LLMs

- Ivy is a declarative full-stack UI framework that allows developers to build user interfaces using a component-based approach very similar to React.
- In Ivy you only write one application in pure C# and we don't have a BE and FE distinction.
- UI rendering is handled by Ivy.
- When programming in Ivy you focus on building the logical structure of your application using a large set of pre-built widgets and views - you rarely need to specify any styling - Ivy just makes it look good by default.

Terminology:

- "Views" are the main building blocks of the UI, similar to React components.
- "Hooks" are functions that allow views to manage state and side effects.
- "Widgets" are the UI elements that make up the views. e.g., Button, TextBlock, StackPanel...
- "Apps" are the top-level views that represent entire applications.

A view is defined as a class that inherits from `ViewBase` and implements a `Build` method. The `Build` method returns either another view or a widget.

Widgets can have multiple children, but views can only return a single object (widget or view). To return multiple widgets from a view, you can use a `Fragment` use the Layout helpers. See below.

public class MyView : ViewBase
{
  public override object? Build()
  {
    var count = UseState(0);
    return Layout.Vertical()
        | new Text($"Count: {count.Value}")
        | new Button("Increment", () => count.Set(count.Value + 1));
  }
}

The topmost view in an Ivy application is called an [App](https://docs.ivy.app/onboarding/concepts/apps.md) and is decorated with the `[App]` attribute.

[App()]
public class MyApp : ViewBase

- The convention is to put all apps in the `Apps` folder of your Ivy project.
- An app is built into a tree of widgets. This is what's rendered to the screen.

## Common Widgets

[Button](https://docs.ivy.app/widgets/common/button.md)
[Card](https://docs.ivy.app/widgets/common/card.md)
[Badge](https://docs.ivy.app/widgets/common/badge.md)
[Sheet](https://docs.ivy.app/widgets/advanced/sheet.md)
[Progress](https://docs.ivy.app/widgets/common/progress.md)
[Expandable](https://docs.ivy.app/widgets/common/expandable.md)
[Tooltip](https://docs.ivy.app/widgets/common/tooltip.md)
[DropDownMenu](https://docs.ivy.app/widgets/common/drop-down-menu)
[Table](https://docs.ivy.app/widgets/common/table.md)
[List](https://docs.ivy.app/widgets/common/list.md)
[Details](https://docs.ivy.app/widgets/common/details.md)
[Image](https://docs.ivy.app/widgets/primitives/image.md)
[Avatar](https://docs.ivy.app/widgets/primitives/avatar.md)
[Spacer](https://docs.ivy.app/widgets/primitives/spacer.md)
[Callout](https://docs.ivy.app/widgets/primitives/callout.md)

## Layouts

- Use Layout.Vertical() or Layout.Horizontal() to create stack layouts.
- Layout.Grid()
— Layout.Wrap()
- Add Children: Pipe child elements using the | operator to arrange them top-to-bottom (vertical) or left-to-right (horizontal).
- Layouts can be customized with methods like .Gap(int number) to set spacing between children. Use .Left(), .Center(), or .Right() methods to control alignment.
- The number in Gap(int number) works the same as in Tailwind CSS spacing scale (e.g., 1 = 0.25rem, 2 = 0.5rem, etc.).
Layouts have a default gap of 4 (1rem). In general, you very rarely need to set the gap explicitly.

// Basic Vertical Layout
Layout.Vertical()
    | new Badge("Top")
    | new Badge("Middle")
    | new Badge("Bottom");

// Nested layouts with alignment
Layout.Vertical().Align(Align.Center)
    | Text.Label("Header")
    | (Layout.Horizontal()
        | new Button("Previous")
        | new Button("Next")); //NOTE: Parentheses are used to group the horizontal layout - THIS IS REQUIRED

Grids:

Layout.Grid()
  .Columns(2)
  .Rows(2)
  .Gap(4)
  .Padding(8)
    | Text.Block("Cell 1")
    | Text.Block("Cell 2")
    ...

Align values: TopLeft, TopCenter, TopRight, Left, Center, Right, BottomLeft, BottomCenter, BottomRight, Stretch

[Layouts](https://docs.ivy.app/onboarding/concepts/layout.md)

## Text

The Text helper utility is used to create various semantic text elements.

- Text.H1, Text.H2, : For headings.
- Text.Lead: For prominent introductory text.
- Text.P: For standard paragraphs.
- Text.Block: For block-level content (e.g., list items).
- Text.InlineCode: For displaying inline code snippets.

Styling Modifiers:
.NoWrap():
.Bold()
.Italic()
.StrikeThrough()
.Color(Colors)

Layout.Vertical()
    | Text.H1("Getting Started")
    | Text.P("This is a paragraph of text.").NoWrap()

[Text](https://docs.ivy.app/widgets/primitives/text-block.md)

## Event Handling

new Button("Click Me")
  .Primary()
  .HandleClick(() => {
    count.Set(count.Value + 1);
})

new TextInput().Default()
    .Value(text.Value)
    .OnChange(text.Set)
    .OnBlur(() => Console.WriteLine("Blurred"))

## Hooks

- Most hooks that you know from React are available in Ivy. They follow the same principles as in React.
- Hooks should only be called at the top level of a Build() method, not inside loops or conditions.

### UseState

var nameState = UseState("World");
var iconsState = this.UseState<Icons[]>();

If you don't specify a value, default(T) is used.

UseState hook returns a state object IState<T> that provides:

- .Value property to read the current state.
- .Set(newValue) method to update the state in UseEffect or in an event handler.

### UseEffect

void UseEffect(Action effect, params IEffectTriggers[] triggers)
void UseEffect(Func<Task> asyncEffect, params IEffectTriggers[] triggers)
void UseEffect(Func<IDisposable> effectWithCleanup, params IEffectTriggers[] triggers)
void UseEffect(Func<Task<IDisposable>> asyncEffectWithCleanup, IEffectTriggers object[] triggers)

- EffectTrigger.OnBuild() - runs after every build
- EffectTrigger.OnMount() - runs once when the view is first mounted
- EffectTrigger.OnStateChange(IState<T>) - runs when the specified state changes

- IState<T> is automatically converted to EffectTrigger.OnStateChange
- If no triggers are provided, the effect trigger is assumed to be OnMount.

### Other Hooks

UseMemo
UseCallback
UseRef
UseContext
UseReducer
UseQuery
UseSignal

## Inputs

Ivy has several Input widgets for handling user input. There are rarely used directly - instead we use
extension methods on IState<T> to bind state to inputs.

var userNameState = UseState("");
var input = userNameState.ToTextInput().Placeholder("Enter your name");

ToTextInput()
ToTextAreaInput()
ToPasswordInput()
ToNumberInput()
ToBoolInput()
ToSelectInput(IEnumerable<IAnyOption>)
ToCodeInput(Language)
ToColorInput()
ToDateTimeInput()
ToDateRangeInput()
ToFeedbackInput()

Most inputs have extension methods for common configurations:
userNameState.ToTextInput().Required().MaxLength(50).Placeholder("Enter your name");

## Best Practices

(Basically the same as React best practices)

1. **Keep Views Pure** - Views should be pure functions of their props and state
2. **Use Hooks Correctly** - Call hooks at the top level, never in loops or conditions  
3. **Minimize State** - Derive computed values instead of storing them
4. **Handle Loading States** - Always consider loading and error states
5. **Leverage Type Safety** - Use strongly-typed widgets and state
6. **Component Composition** - Build complex UIs from simple, reusable views

## Further Reading

[Forms](https://docs.ivy.app/onboarding/concepts/forms.md)
[DataTable](https://docs.ivy.app/widgets/advanced/data-table.md)
[Table](https://docs.ivy.app/widgets/common/table.md)
[Details](https://docs.ivy.app/widgets/common/details.md) - Display structured label-value pairs
[Services](https://docs.ivy.app/onboarding/concepts/services.md)
[Program.cs](https://docs.ivy.app/onboarding/concepts/program.md)
[Colors](https://docs.ivy.app/api-reference/ivy-shared/colors.md)
[Size](https://docs.ivy.app/api-reference/ivy-shared/size.md)
[Align](https://docs.ivy.app/api-reference/ivy-shared/align.md)
[UseAlert](https://docs.ivy.app/onboarding/concepts/alerts.md)
[RefreshTokens](https://docs.ivy.app/onboarding/concepts/refresh-tokens.md)
[Downloads](https://docs.ivy.app/onboarding/concepts/downloads.md)
[Uploads](https://docs.ivy.app/widgets/inputs/file.md)
[Icons](https://raw.githubusercontent.com/Ivy-Interactive/Ivy-Framework/refs/heads/main/src/Ivy/Shared/Icons.cs)
