using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Concepts;

[App(icon: Icons.Settings)]
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

        return new Card(record.ToDetails().RemoveEmpty());
    }
}