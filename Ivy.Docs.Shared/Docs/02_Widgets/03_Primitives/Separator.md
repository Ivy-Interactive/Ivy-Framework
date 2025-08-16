# Separator

<Ingress>
Create visual dividers between content sections to organize information and improve interface readability with clear content demarcation.
</Ingress>

The `Separator` widget creates a visual divider between content sections. It helps organize information and improve readability by clearly demarcating different parts of your interface.

```csharp demo-tabs 
public class ProfileDetailView : ViewBase
{
    public override object? Build()
    {
        // Sample user data - in a real app, this would come from a data source
        var user = new
        {
            Name = "Alex Johnson",
            Email = "alex.johnson@example.com",
            Role = "Senior Developer",
            Department = "Engineering",
            JoinDate = new DateTime(2020, 3, 15),
            Skills = new[] { "C#", "React", "Azure", "SQL", "TypeScript" },
            Projects = new[] 
            { 
                "Customer Portal Redesign", 
                "Inventory Management System", 
                "API Gateway Implementation" 
            }
        };
        
        return Layout.Vertical().Gap(4).Width(Size.Units(600))
            | Text.H1("User Profile")
            
            | Layout.Vertical().Gap(2)
                | Text.H2("Personal Information")
                | Layout.Horizontal().Gap(2)
                    | Text.Strong("Name:")
                    | Text.Inline(user.Name)
                | Layout.Horizontal().Gap(2)
                    | Text.Strong("Email:")
                    | Text.Inline(user.Email)
            
            | new Separator()
            
            | Layout.Vertical().Gap(2)
                | Text.H2("Work Information")
                | Layout.Horizontal().Gap(2)
                    | Text.Strong("Role:")
                    | Text.Inline(user.Role)
                | Layout.Horizontal().Gap(2)
                    | Text.Strong("Department:")
                    | Text.Inline(user.Department)
                | Layout.Horizontal().Gap(2)
                    | Text.Strong("Join Date:")
                    | Text.Inline(user.JoinDate.ToShortDateString())
            
            | new Separator()
            
            | Layout.Vertical().Gap(2)
                | Text.H2("Skills")
                | Layout.Horizontal().Gap(2).Wrap(true)
                    | user.Skills.Select(skill => 
                        new Badge(skill).Variant(BadgeVariant.Secondary))
            
            | new Separator()
            
            | Layout.Vertical().Gap(2)
                | Text.H2("Projects")
                | Layout.Vertical().Gap(1)
                    | user.Projects.Select(project => Text.P(project));
    }
}
```

<WidgetDocs Type="Ivy.Separator" ExtensionTypes="Ivy.SeparatorExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Separator.cs"/>
