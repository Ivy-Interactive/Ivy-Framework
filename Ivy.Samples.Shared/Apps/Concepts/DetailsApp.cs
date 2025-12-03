using Ivy.Shared;
using Ivy.Views.Builders;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Settings, searchHints: ["properties", "fields", "display", "information", "view", "data"])]
public class DetailsApp : SampleBase
{
    protected override object? BuildSample()
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
            EmptyField1 = "",
            EmptyField2 = false,
            EmptyField3 = (string)null!,
        };

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

        var record_2 = new
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
                | Text.H1("Details Size")
                | (Layout.Horizontal().Gap(2)
                    | (Layout.Vertical()
                        | Text.Label("Small Size").Bold()
                        | new Card(record_small.ToDetails().RemoveEmpty().Small())
                        | new Card(record_2.ToDetails().MultiLine(x => x.LastName).RemoveEmpty().Small()))
                    | (Layout.Vertical()
                        | Text.Label("Medium Size").Bold()
                        | new Card(record.ToDetails().RemoveEmpty())
                        | new Card(record_2.ToDetails().MultiLine(x => x.LastName).RemoveEmpty()))
                    | (Layout.Vertical()
                        | Text.Label("Large Size").Bold()
                        | new Card(record_large.ToDetails().RemoveEmpty().Large())
                        | new Card(record_2.ToDetails().MultiLine(x => x.LastName).RemoveEmpty().Large())))
                ;
    }
}