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
public enum UserRole
{
    Admin,
    User,
    Guest
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

[App(icon: Icons.Clipboard, searchHints: ["inputs", "fields", "validation", "submission", "data-entry", "controls"])]
public class FormApp : SampleBase
{
    protected override object? BuildSample()
    {
        var model = UseState(() => new UserModel("Niels Bosma", "1234156", true, DateTime.Parse("1982-07-17"), 183));

        FormBuilder<UserModel> BuildForm(IState<UserModel> x) =>
            x.ToForm()
                .Label(m => m.Name, "Full Name")
                .Description(m => m.Name, "Make sure you enter your full name.")
                .Help(m => m.Name, "Use your full legal name as it appears on official documents")
                .Builder(m => m.IsAwesome, s => s.ToBoolInput().Description("Is this user awesome?"))
                .Builder(m => m.Gender, s => s.ToSelectInput())
                .Builder(m => m.Json, s => s.ToCodeInput().Language(Languages.Json))
                .Help(m => m.Json, "Enter JSON data in valid format. Use curly braces for objects and square brackets for arrays.");

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
               | (Layout.Horizontal()
                  | new Button("Open in Sheet").ToTrigger((isOpen) => BuildForm(model).ToSheet(isOpen, "User Information", "Please fill in the form."))
                  | new Button("Open in Dialog").ToTrigger((isOpen) => BuildForm(model).ToDialog(isOpen, "User Information", "Please fill in the form.", width: Size.Units(200)))
               )
               | form0
               | new Separator()
               | Text.H3("Database Generator Form Test")
               | databaseForm
               | Text.H2("Form Size Demonstration")
               | Text.P("This demonstrates how form sizes affect spacing between fields.")
               | (Layout.Horizontal()
                | new Card(
                    smallModel.ToForm()
                        .Small()
                        .Group("Personal Information", open: true, m => m.Name, m => m.Email, m => m.Age)
                        .Group("Contact Details", m => m.PhoneNumber, m => m.Website)
                        .Builder(m => m.Description, s => s.ToTextAreaInput())
                        .Builder(m => m.Password, s => s.ToPasswordInput())
                        .Builder(m => m.PhoneNumber, s => s.ToTelInput())
                        .Builder(m => m.Website, s => s.ToUrlInput())
                        .Builder(m => m.Color, s => s.ToColorInput())
                )
                .Width(1 / 3f)
                .Title("Small Form (with Groups)")
                | new Card(
                    mediumModel.ToForm()
                        .Medium()
                        .Group("Account", open: true, m => m.Name, m => m.Email, m => m.Password)
                        .Group("Profile", m => m.Age, m => m.BirthDate, m => m.Role)
                        .Builder(m => m.Description, s => s.ToTextAreaInput())
                        .Builder(m => m.Password, s => s.ToPasswordInput())
                        .Builder(m => m.PhoneNumber, s => s.ToTelInput())
                        .Builder(m => m.Website, s => s.ToUrlInput())
                        .Builder(m => m.Color, s => s.ToColorInput())
                )
                .Width(1 / 3f)
                .Title("Medium Form (with Groups)")
                | new Card(
                    largeModel.ToForm()
                        .Large()
                        .Group("Basic Info", open: true, m => m.Name, m => m.Email)
                        .Group("Details", m => m.Description, m => m.Color)
                        .Builder(m => m.Description, s => s.ToTextAreaInput())
                        .Builder(m => m.Password, s => s.ToPasswordInput())
                        .Builder(m => m.PhoneNumber, s => s.ToTelInput())
                        .Builder(m => m.Website, s => s.ToUrlInput())
                        .Builder(m => m.Color, s => s.ToColorInput())
                )
                .Width(1 / 3f)
                .Title("Large Form (with Groups)"))
            ;
        ;
    }
}