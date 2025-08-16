using Ivy.Shared;
using Ivy.Views.Forms;

namespace Ivy.Samples.Shared.Apps.Concepts;

public enum Gender
{
    Male,
    Female,
    Other
}

public enum Fruits
{
    Banana,
    Apple,
    Orange,
    Pear,
    Strawberry
}

public record UserModel(
    string Name, string Password, bool IsAwesome, DateTime BirthDate, int Height, int UserId = 123, Gender Gender = Gender.Male, string Json = "{}", List<Fruits> FavoriteFruits = null!);

[App(icon: Icons.Clipboard)]
public class FormApp : SampleBase
{
    protected override object? BuildSample()
    {
        var model = UseState(() => new UserModel("Niels Bosma", "1234156", true, DateTime.Parse("1982-07-17"), 183));

        FormBuilder<UserModel> BuildForm(IState<UserModel> x) =>
            x.ToForm()
                .Label(m => m.Name, "Full Name")
                .Builder(m => m.IsAwesome, s => s.ToBoolInput().Description("Is this user awesome?"))
                .Builder(m => m.Gender, s => s.ToSelectInput())
                .Builder(m => m.Json, s => s.ToCodeInput().Language(Languages.Json))
                .Description(m => m.Name, "Make sure you enter your full name.");

        var form0 = Layout.Horizontal(
            new Card(
                    BuildForm(model)
                )
                .Width(1 / 2f)
                .Title("User Information"),
            new Card(
                model.ToDetails()
            ).Width(1 / 2f)
        );

        return Layout.Vertical()
               | (Layout.Horizontal()
                  | new Button("Open in Sheet").ToTrigger((isOpen) => BuildForm(model).ToSheet(isOpen, "User Information", "Please fill in the form."))
                  | new Button("Open in Dialog").ToTrigger((isOpen) => BuildForm(model).ToDialog(isOpen, "User Information", "Please fill in the form.", width: Size.Units(200)))
               )
               | form0
            ;
    }
}