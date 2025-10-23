using Ivy.Shared;
using Ivy.Views.Forms;

namespace Ivy.Samples.Shared.Apps.Concepts;

public enum UserRole
{
    Admin,
    User,
    Guest
}

public record TestModel(
    string Name,
    string Email,
    string Password,
    string Description,
    bool IsActive,
    int Age,
    double Salary,
    DateTime BirthDate,
    UserRole Role,
    string? PhoneNumber,
    string? Website,
    string? Color
);

[App(icon: Icons.Clipboard, searchHints: ["form", "size", "spacing", "layout", "small", "medium", "large"])]
public class FormSizeTestApp : SampleBase
{
    protected override object? BuildSample()
    {
        var smallModel = UseState(() => new TestModel(
            "John Doe",
            "john@example.com",
            "password123",
            "A small form example with all input types",
            true,
            25,
            75000.50,
            DateTime.Parse("1999-01-01"),
            UserRole.User,
            "+1-555-0123",
            "https://johndoe.com",
            "#3B82F6"
        ));

        var mediumModel = UseState(() => new TestModel(
            "Jane Smith",
            "jane@example.com",
            "password456",
            "A medium form example with all input types",
            false,
            30,
            85000.75,
            DateTime.Parse("1994-06-15"),
            UserRole.Admin,
            "+1-555-0456",
            "https://janesmith.com",
            "#10B981"
        ));

        var largeModel = UseState(() => new TestModel(
            "Bob Johnson",
            "bob@example.com",
            "password789",
            "A large form example with all input types",
            true,
            35,
            95000.25,
            DateTime.Parse("1989-12-25"),
            UserRole.Guest,
            "+1-555-0789",
            "https://bobjohnson.com",
            "#F59E0B"
        ));

        return Layout.Vertical()
               | Text.H2("Form Size Demonstration")
               | Text.P("This demonstrates how form sizes affect spacing between fields.")
               | (Layout.Horizontal()
                | new Card(
                    smallModel.ToForm()
                        .Size(Sizes.Small)
                        .Builder(m => m.Description, s => s.ToTextAreaInput())
                        .Builder(m => m.Password, s => s.ToPasswordInput())
                        .Builder(m => m.PhoneNumber, s => s.ToTelInput())
                        .Builder(m => m.Website, s => s.ToUrlInput())
                        .Builder(m => m.Color, s => s.ToColorInput())
                )
                .Width(1 / 3f)
                .Title("Small Form")
                | new Card(
                    mediumModel.ToForm()
                        .Size(Sizes.Medium)
                        .Builder(m => m.Description, s => s.ToTextAreaInput())
                        .Builder(m => m.Password, s => s.ToPasswordInput())
                        .Builder(m => m.PhoneNumber, s => s.ToTelInput())
                        .Builder(m => m.Website, s => s.ToUrlInput())
                        .Builder(m => m.Color, s => s.ToColorInput())
                )
                .Width(1 / 3f)
                .Title("Medium Form (Default)")
                | new Card(
                    largeModel.ToForm()
                        .Size(Sizes.Large)
                        .Builder(m => m.Description, s => s.ToTextAreaInput())
                        .Builder(m => m.Password, s => s.ToPasswordInput())
                        .Builder(m => m.PhoneNumber, s => s.ToTelInput())
                        .Builder(m => m.Website, s => s.ToUrlInput())
                        .Builder(m => m.Color, s => s.ToColorInput())
                )
                .Width(1 / 3f)
                .Title("Large Form"))
               | Text.P("Notice how the spacing between fields, the size of input elements, field labels, AND submit buttons all change based on the form size setting.")
               | Text.P("This example shows all available input types: Text, Email, Password, TextArea, Boolean, Number, DateTime, Select (Enum), Tel, URL, and Color inputs.")
               | Text.P("The FormBuilder.Size() method controls the gap between form fields, the size of all input elements, field labels, and submit buttons within the form.")
            ;
    }
}
