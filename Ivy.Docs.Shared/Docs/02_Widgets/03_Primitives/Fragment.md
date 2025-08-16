# Fragment

<Ingress>
Group multiple elements without adding extra DOM markup, similar to React Fragments, for clean component composition.
</Ingress>

The `Fragment` widget is a container component that doesn't produce any HTML elements itself. It's useful for grouping multiple elements without adding extra markup to the DOM, similar to React Fragments.

```csharp demo-tabs 
public class ConditionalRenderingView : ViewBase
{
    public override object? Build()
    {
        var showAdminControls = UseState(false);
        
        return Layout.Vertical().Gap(4)
            | Text.H2("User Dashboard")
            | new Fragment(
                showAdminControls.Value
                    ? new Fragment(
                        Text.H3("Admin Controls"),
                        new Button("Reset System", variant: ButtonVariant.Destructive),
                        new Button("View Logs"),
                        new Button("Manage Users")
                      )
                    : Text.Muted("Admin controls are hidden")
              );
    }
}
```

<WidgetDocs Type="Ivy.Fragment" ExtensionTypes="Ivy.FragmentExtensions"  SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Fragment.cs"/>
