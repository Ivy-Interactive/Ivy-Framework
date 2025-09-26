using System.Collections.Immutable;
using Ivy.Shared;
using Ivy.Views.Builders;
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

public enum DatabaseProvider
{
    Sqlite,
    SqlServer,
    Postgres,
    MySql,
    MariaDb
}

public enum DatabaseNamingConvention
{
    PascalCase,
    CamelCase,
    SnakeCase,
    KebabCase
}

public enum ViewState
{
    Idle,
    Loading,
    Success,
    Error
}

public record AppSpec(string Name, string Description);

public record DatabaseGeneratorModel(
    ViewState ViewState,
    string Prompt,
    string? Dbml,
    string Namespace,
    string ProjectDirectory,
    string GeneratorDirectory,
    DatabaseProvider DatabaseProvider,
    DatabaseNamingConvention DatabaseNamingConvention,
    bool RunGenerator,
    bool DeleteDatabase,
    bool SeedDatabase,
    string ConnectionString,
    string DataContextClassName,
    string DataSeederClassName,
    ImmutableArray<AppSpec> Apps,
    Guid SessionId,
    bool SkipDebug = false
);

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

        // Database Generator Form Test - demonstrates proper boolean field labeling
        var settingsForm = UseState(() => new DatabaseGeneratorModel(
            ViewState.Idle,
            "Generate a simple blog database",
            null,
            "MyApp.Data",
            "/src/MyApp",
            "/tools/generator",
            DatabaseProvider.Sqlite,
            DatabaseNamingConvention.PascalCase,
            true,
            false,
            true,
            "Data Source=blog.db",
            "BlogContext",
            "BlogSeeder",
            ImmutableArray<AppSpec>.Empty,
            Guid.NewGuid()
        ));

        FormBuilder<DatabaseGeneratorModel> BuildDatabaseForm(IState<DatabaseGeneratorModel> x) =>
            x.ToForm()
                .Label(m => m.DatabaseProvider, "Database:")
                .Label(m => m.ConnectionString, "Connection String:")
                .Label(m => m.DeleteDatabase, "Delete Existing Database (Dangerous)")
                .Label(m => m.SeedDatabase, "Fill Database with Seed Data")
                .Builder(m => m.ConnectionString, s => s.ToCodeInput())
                .Visible(m => m.DatabaseProvider, m => m.RunGenerator)
                .Visible(m => m.ConnectionString, m => m.RunGenerator)
                .Visible(m => m.DeleteDatabase, m => m.RunGenerator)
                .Visible(m => m.SeedDatabase, m => m.RunGenerator)
                .Remove(m => m.ProjectDirectory)
                .Remove(m => m.GeneratorDirectory)
                .Remove(m => m.RunGenerator);

        var databaseForm = Layout.Horizontal(
            new Card(
                    BuildDatabaseForm(settingsForm)
                )
                .Width(1 / 2f)
                .Title("Database Generator Settings"),
            new Card(
                settingsForm.ToDetails()
            ).Width(1 / 2f)
        );

        return Layout.Vertical()
               | (Layout.Horizontal()
                  | new Button("Open in Sheet").ToTrigger((isOpen) => BuildForm(model).ToSheet(isOpen, "User Information", "Please fill in the form."))
                  | new Button("Open in Dialog").ToTrigger((isOpen) => BuildForm(model).ToDialog(isOpen, "User Information", "Please fill in the form.", width: Size.Units(200)))
               )
               | form0
               | new Separator()
               | Text.H3("Database Generator Form Test")
               | databaseForm
            ;
    }
}