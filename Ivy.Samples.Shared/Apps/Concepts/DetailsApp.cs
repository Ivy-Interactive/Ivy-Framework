using Ivy.Shared;
using Ivy.Views.Builders;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Settings, searchHints: ["properties", "fields", "display", "information", "view", "data"])]
public class DetailsApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Vertical().Gap(2)
               | Text.H1("Details")
               | Layout.Tabs(
                   new Tab("Basic", new DetailsBasicExample()),
                   new Tab("Nested", new DetailsNestedExample()),
                   new Tab("Multiline", new DetailsMultilineExample()),
                   new Tab("Scale", new DetailsScaleExample())
               ).Variant(TabsVariant.Content);
    }
}

public class DetailsBasicExample : ViewBase
{
    public override object? Build()
    {
        var record = new
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30,
            IsMarried = true,
            BirthDate = new DateTime(1990, 1, 1),
            Email = "john.doe@example.com",
            Phone = "+1 (555) 123-4567",
            EmptyField1 = "",
            EmptyField2 = false,
            EmptyField3 = (string)null!,
        };

        return Layout.Vertical().Gap(2)
               | Text.H2("Basic Details")
               | Text.P("Simple example of ToDetails() that displays properties of an object. Empty fields are removed using RemoveEmpty().")
               | new Card(record.ToDetails().RemoveEmpty());
    }
}

public class DetailsNestedExample : ViewBase
{
    public override object? Build()
    {
        var record = new
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30,
            IsMarried = true,
            BirthDate = new DateTime(1990, 1, 1),
            Address = new
            {
                Street = "123 Elm St",
                City = "Springfield",
                State = "IL",
                Zip = "62701"
            }.ToDetails(),
            Employment = new
            {
                Company = "Company Name Here",
                Position = "Position Name Here",
                Department = "Department Name Here"
            }.ToDetails(),
            EmptyField1 = "",
            EmptyField2 = false,
            EmptyField3 = (string)null!,
        };

        return Layout.Vertical().Gap(2)
               | Text.H2("Nested Details")
               | Text.P("Example of nested details where some properties are themselves Details objects (for example, Address and Employment).")
               | new Card(record.ToDetails().RemoveEmpty());
    }
}

public class DetailsMultilineExample : ViewBase
{
    public override object? Build()
    {
        var record = new
        {
            Title = "Very Long Description Example",
            ShortSummary = "This item has a long description that is better displayed as multiline text.",
            Description =
                "This is a long description field that contains multiple sentences and paragraphs of explanatory text. " +
                "Details.MultiLine() allows this content to wrap across multiple lines instead of being truncated, " +
                "so users can comfortably read everything without hovering or opening a separate dialog. " +
                "You can use this pattern for notes, comments, troubleshooting instructions, or domain-specific explanations " +
                "that would otherwise break the layout if rendered as a single-line value. " +
                "In real applications this might describe how a record was created, why certain values were chosen, " +
                "or provide a short change history that helps other team members quickly understand the context.",
            Notes =
                "Additional notes about this record can also be displayed as multiline content " +
                "to keep the layout readable and user-friendly.",
        };

        return Layout.Vertical().Gap(2)
               | Text.H2("Multiline Fields")
               | Text.P("Compare the difference: without MultiLine() text is truncated, with MultiLine() it wraps across multiple lines.")
               | new Spacer().Height(10)
               | (Layout.Grid().Columns(2).Gap(4)
                   | (Layout.Vertical().Height(Size.Full())
                       | Text.Label("With MultiLine()").Bold()
                       | new Card(
                           record.ToDetails()
                                 .MultiLine(x => x.Description, x => x.Notes)
                       ))
                   | (Layout.Vertical().Height(Size.Full())
                       | Text.Label("Without MultiLine()").Bold()
                       | new Card(record.ToDetails()))
                    )
               ;
    }
}

public class DetailsScaleExample : ViewBase
{
    public override object? Build()
    {
        var record_small = new
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30,
            IsMarried = true,
            BirthDate = new DateTime(1990, 1, 1),
            Address = new
            {
                Street = "123 Elm St",
                City = "Springfield",
                State = "IL",
                Zip = "62701"
            }.ToDetails().Small(),
            EmptyField1 = "",
            EmptyField2 = false,
            EmptyField3 = (string)null!,
        };

        var record_medium = new
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30,
            IsMarried = true,
            BirthDate = new DateTime(1990, 1, 1),
            Address = new
            {
                Street = "123 Elm St",
                City = "Springfield",
                State = "IL",
                Zip = "62701"
            }.ToDetails(),
            EmptyField1 = "",
            EmptyField2 = false,
            EmptyField3 = (string)null!,
        };
        var record_large = new
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 30,
            IsMarried = true,
            BirthDate = new DateTime(1990, 1, 1),
            Address = new
            {
                Street = "123 Elm St",
                City = "Springfield",
                State = "IL",
                Zip = "62701"
            }.ToDetails().Large(),
            EmptyField1 = "",
            EmptyField2 = false,
            EmptyField3 = (string)null!,
        };

        var record = new
        {
            FirstName = "Hubert Blaine Wolfeschlegelsteinhausenbergerdorff Sr.",
            LastName = "Leone Sextus Denys Oswolf Fraudatifilius Tollemache-Tollemache de Orellana-Plantagenet-Tollemache-Tollemache",
            Age = 42,
            IsMarried = false,
            BirthDate = new DateTime(1982, 7, 14),
            EmptyField1 = "",
            EmptyField2 = false,
            EmptyField3 = (string)null!,
        };

        return Layout.Vertical().Gap(2)
                | Text.H1("Details Scale")
                | (Layout.Horizontal().Gap(2)
                    | (Layout.Vertical()
                        | Text.Label("Small Scale").Bold()
                        | new Card(record_small.ToDetails().RemoveEmpty().Small())
                        | new Card(record.ToDetails().MultiLine(x => x.LastName).RemoveEmpty().Small()))
                    | (Layout.Vertical()
                        | Text.Label("Medium Scale").Bold()
                        | new Card(record_medium.ToDetails().RemoveEmpty())
                        | new Card(record.ToDetails().MultiLine(x => x.LastName).RemoveEmpty()))
                    | (Layout.Vertical()
                        | Text.Label("Large Scale").Bold()
                        | new Card(record_large.ToDetails().RemoveEmpty().Large())
                        | new Card(record.ToDetails().MultiLine(x => x.LastName).RemoveEmpty().Large())))
                ;
    }
}