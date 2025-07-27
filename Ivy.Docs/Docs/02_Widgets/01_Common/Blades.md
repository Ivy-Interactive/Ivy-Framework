# Blades

Blades provide a stacked navigation pattern where new views slide in from the right. Use the `UseBlades` extension to create a root blade and manage a stack of blades through `IBladeController`.

## Basic Usage

```csharp
this.UseBlades(() => new RootView("A"), "Blade 0");
```

Pushing a new blade can be done through the controller:

```csharp
action<Event<Button>> onClick = e =>
{
    var blades = this.UseContext<IBladeController>();
    blades.Push(this, new RootView(e.Sender.Tag?.ToString() ?? "?"), "Next Blade");
};
```

<WidgetDocs Type="Ivy.Blade" ExtensionTypes="Ivy.Views.Blades.UseBladesExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Blades/UseBlades.cs"/>
