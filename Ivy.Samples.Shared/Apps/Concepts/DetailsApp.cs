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

        return Layout.Vertical()
                | new Card(record.ToDetails().RemoveEmpty())
                | new Card(record_2.ToDetails().MultiLine(x => x.LastName).RemoveEmpty())
                ;
    }
}