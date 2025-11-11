---
prepare: |
    var client = this.UseService<IClientProvider>();
searchHints:
  - fundamentals
  - views
  - widgets
  - state
  - beginner
  - basics
---

# Basics

<Ingress>
Learn the essential concepts of Ivy development including Views, Widgets, state management, and how to build your first interactive application.
</Ingress>

First, make sure you have [installed Ivy](./02_Installation.md) on your development machine.

## Create a new project

Using the CLI we can easily create a new project.

```terminal
>ivy init --namespace YourProjectNamespace
```

## Views and Widgets

Now let's add our first Ivy app. In the folder `Apps` create a new file `CounterApp.cs` that inherits from `ViewBase`.

```csharp
[App(icon: Icons.Box)]
public class CounterApp : ViewBase
{
    public override object? Build()
    {
        return "HelloWorld";
    }
}
```

Ivy is heavily inspired by React. A view is similar to a component in React and needs to implement a `Build` function that can return any `object` and Ivy will figure out how to render it (see [ContentBuilders](../02_Concepts/ContentBuilders.md)).

The result from `Build` is usually another view or a widget. Widgets are the smallest building blocks in Ivy and are rendered on the client as a React component.

Now let's make it a little more interesting by returning a button widget that shows a toast when clicked.

```csharp demo-below
public class CounterApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        return new Button("Click me", onClick: _ => client.Toast("Hello!"));
    }
}
```

<Callout Icon="Info">
These pages are implemented in Ivy so try to click on the button above. You should get a toast with the text "Hello!"
</Callout>

In this example, you can see how to use an Ivy Button. For more details, check out the [Button implementation](../../02_Widgets/01_Common/Button.md).

## State Management

Ivy has a built-in state management system through the `UseState` hook (similar to React).

```csharp
[App(icon: Icons.Box)]
public class CounterApp : ViewBase
{
    public override object? Build()
    {
        var counter = UseState(0);
        return Layout.Horizontal().Align(Align.Left)
          | new Button("+1", onClick: _ => counter.Set(counter.Value+1))
          | new Button("-1", onClick: _ => counter.Set(counter.Value-1))
          | counter;
    }
}
```

```csharp demo
public class CounterApp : ViewBase
{
    public override object? Build()
    {
        var counter = UseState(0);
        return Layout.Horizontal().Align(Align.Left)
          | new Button("+1", onClick: _ => counter.Set(counter.Value+1))
          | new Button("-1", onClick: _ => counter.Set(counter.Value-1))
          | counter;
    }
}
```

You can learn more about layouts [here](../../02_Widgets/04_Layouts/SidebarLayout.md). For more information about widgets, see [Widgets](../02_Concepts/Widgets.md).
