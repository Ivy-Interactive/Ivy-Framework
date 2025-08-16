# Callout

`Callout`s are attention-grabbing components used to highlight important information, warnings, or tips. They come in different variants including info, warning, error, and success.

```csharp demo-tabs 
public class UserGuideView : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(4)
            | Callout.Info(
                "This feature requires admin privileges.",
                "Access Note")
            | Callout.Warning(
                "Changes made here cannot be automatically undone.",
                "Caution")
            | Callout.Error(
                "API connection failed. Check your network settings.",
                "Connection Error")
                .Icon(Icons.Bug)
            | Callout.Success(
                "Your settings have been saved successfully!",
                "Success");
    }
}
```

<WidgetDocs Type="Ivy.Callout" ExtensionTypes="Ivy.CalloutExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Callout.cs"/>
