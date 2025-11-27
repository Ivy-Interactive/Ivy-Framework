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

public record ComprehensiveInputModel(
    // Text inputs
    string TextField,
    string EmailField,
    string PasswordField,
    string SearchField,
    string TelField,
    string UrlField,
    string TextAreaField,
    // Number inputs
    int IntegerField,
    double DecimalField,
    // Bool inputs
    bool CheckboxField,
    bool SwitchField,
    bool ToggleField,
    // DateTime inputs
    DateTime DateField,
    DateTime DateTimeField,
    DateTime TimeField,
    // Select inputs
    UserRole SelectField,
    List<Fruits> MultiSelectField,
    string? AsyncSelectField,
    // Other inputs
    string ColorField,
    string CodeField,
    int RatingField,
    bool ThumbsField,
    int EmojiField
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
        var smallModel = UseState(() => new ComprehensiveInputModel(
            "John Doe",
            "john@example.com",
            "password123",
            "Search query",
            "+1-555-0123",
            "https://johndoe.com",
            "A small form example with all input types",
            25,
            75000.50,
            true,
            false,
            true,
            DateTime.Parse("1999-01-01"),
            DateTime.Parse("1999-01-01 14:30:00"),
            DateTime.Parse("1999-01-01 14:30:00"),
            UserRole.User,
            new List<Fruits> { Fruits.Apple, Fruits.Banana },
            null,
            "#3B82F6",
            "{\"key\": \"value\"}",
            4,
            true,
            3
        ));

        var mediumModel = UseState(() => new ComprehensiveInputModel(
            "Jane Smith",
            "jane@example.com",
            "password456",
            "Search query",
            "+1-555-0456",
            "https://janesmith.com",
            "A medium form example with all input types",
            30,
            85000.75,
            false,
            true,
            false,
            DateTime.Parse("1994-06-15"),
            DateTime.Parse("1994-06-15 10:20:00"),
            DateTime.Parse("1994-06-15 10:20:00"),
            UserRole.Admin,
            new List<Fruits> { Fruits.Orange },
            null,
            "#10B981",
            "console.log('hello');",
            5,
            false,
            4
        ));

        var largeModel = UseState(() => new ComprehensiveInputModel(
            "Bob Johnson",
            "bob@example.com",
            "password789",
            "Search query",
            "+1-555-0789",
            "https://bobjohnson.com",
            "A large form example with all input types",
            35,
            95000.25,
            true,
            true,
            true,
            DateTime.Parse("1989-12-25"),
            DateTime.Parse("1989-12-25 18:45:00"),
            DateTime.Parse("1989-12-25 18:45:00"),
            UserRole.Guest,
            new List<Fruits> { Fruits.Pear, Fruits.Strawberry },
            null,
            "#F59E0B",
            "SELECT * FROM users;",
            3,
            true,
            5
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
               | Text.P("This demonstrates how form sizes affect spacing between fields. All input types are shown with Small, Medium, and Large scales.")
               | BuildFormSizeDemo(smallModel, mediumModel, largeModel)
            ;
        ;
    }

    private object BuildFormSizeDemo(IState<ComprehensiveInputModel> smallModel, IState<ComprehensiveInputModel> mediumModel, IState<ComprehensiveInputModel> largeModel)
    {
        // Sample data for async select
        var sampleOptions = new[] { "Option 1", "Option 2", "Option 3", "Option 4", "Option 5", "Another Option", "Yet Another Option", "Final Option" };

        Task<Option<string?>[]> QueryOptions(string query)
        {
            var lowerQuery = query.ToLowerInvariant();
            return Task.FromResult(sampleOptions
                .Where(o => o.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase))
                .Select(o => new Option<string?>(o, o))
                .ToArray());
        }

        Task<Option<string?>?> LookupOption(string? value)
        {
            if (value == null) return Task.FromResult<Option<string?>?>(null);
            return Task.FromResult<Option<string?>?>(new Option<string?>(value, value));
        }

        return Layout.Horizontal()
                | new Card(
                    smallModel.ToForm()
                        .Small()
                        .Group("Text Inputs", open: true,
                            m => m.TextField,
                            m => m.EmailField,
                            m => m.PasswordField,
                            m => m.SearchField,
                            m => m.TelField,
                            m => m.UrlField,
                            m => m.TextAreaField)
                        .Group("Number Inputs",
                            m => m.IntegerField,
                            m => m.DecimalField)
                        .Group("Bool Inputs",
                            m => m.CheckboxField,
                            m => m.SwitchField,
                            m => m.ToggleField)
                        .Group("DateTime Inputs",
                            m => m.DateField,
                            m => m.DateTimeField,
                            m => m.TimeField)
                        .Group("Select Inputs",
                            m => m.SelectField,
                            m => m.MultiSelectField,
                            m => m.AsyncSelectField)
                        .Group("Other Inputs",
                            m => m.ColorField,
                            m => m.CodeField,
                            m => m.RatingField,
                            m => m.ThumbsField,
                            m => m.EmojiField)
                        .Builder(m => m.TextField, s => s.ToTextInput())
                        .Builder(m => m.EmailField, s => s.ToEmailInput())
                        .Builder(m => m.PasswordField, s => s.ToPasswordInput())
                        .Builder(m => m.SearchField, s => s.ToSearchInput())
                        .Builder(m => m.TelField, s => s.ToTelInput())
                        .Builder(m => m.UrlField, s => s.ToUrlInput())
                        .Builder(m => m.TextAreaField, s => s.ToTextAreaInput())
                        .Builder(m => m.CheckboxField, s => s.ToBoolInput().Variant(BoolInputs.Checkbox))
                        .Builder(m => m.SwitchField, s => s.ToBoolInput().Variant(BoolInputs.Switch))
                        .Builder(m => m.ToggleField, s => s.ToBoolInput().Variant(BoolInputs.Toggle))
                        .Builder(m => m.DateField, s => s.ToDateTimeInput().Variant(DateTimeInputs.Date))
                        .Builder(m => m.DateTimeField, s => s.ToDateTimeInput().Variant(DateTimeInputs.DateTime))
                        .Builder(m => m.TimeField, s => s.ToDateTimeInput().Variant(DateTimeInputs.Time))
                        .Builder(m => m.SelectField, s => s.ToSelectInput())
                        .Builder(m => m.MultiSelectField, s => s.ToSelectInput().List())
                        .Builder(m => m.AsyncSelectField, s => s.ToAsyncSelectInput(QueryOptions, LookupOption, "Search options..."))
                        .Builder(m => m.ColorField, s => s.ToColorInput())
                        .Builder(m => m.CodeField, s => s.ToCodeInput().Language(Languages.Json))
                        .Builder(m => m.RatingField, s => s.ToFeedbackInput().Variant(FeedbackInputs.Stars))
                        .Builder(m => m.ThumbsField, s => s.ToFeedbackInput().Variant(FeedbackInputs.Thumbs))
                        .Builder(m => m.EmojiField, s => s.ToFeedbackInput().Variant(FeedbackInputs.Emojis))
                )
                .Width(1 / 3f)
                .Title("Small Scale - All Inputs")
                | new Card(
                    mediumModel.ToForm()
                        .Medium()
                        .Group("Text Inputs", open: true,
                            m => m.TextField,
                            m => m.EmailField,
                            m => m.PasswordField,
                            m => m.SearchField,
                            m => m.TelField,
                            m => m.UrlField,
                            m => m.TextAreaField)
                        .Group("Number Inputs",
                            m => m.IntegerField,
                            m => m.DecimalField)
                        .Group("Bool Inputs",
                            m => m.CheckboxField,
                            m => m.SwitchField,
                            m => m.ToggleField)
                        .Group("DateTime Inputs",
                            m => m.DateField,
                            m => m.DateTimeField,
                            m => m.TimeField)
                        .Group("Select Inputs",
                            m => m.SelectField,
                            m => m.MultiSelectField,
                            m => m.AsyncSelectField)
                        .Group("Other Inputs",
                            m => m.ColorField,
                            m => m.CodeField,
                            m => m.RatingField,
                            m => m.ThumbsField,
                            m => m.EmojiField)
                        .Builder(m => m.TextField, s => s.ToTextInput())
                        .Builder(m => m.EmailField, s => s.ToEmailInput())
                        .Builder(m => m.PasswordField, s => s.ToPasswordInput())
                        .Builder(m => m.SearchField, s => s.ToSearchInput())
                        .Builder(m => m.TelField, s => s.ToTelInput())
                        .Builder(m => m.UrlField, s => s.ToUrlInput())
                        .Builder(m => m.TextAreaField, s => s.ToTextAreaInput())
                        .Builder(m => m.CheckboxField, s => s.ToBoolInput().Variant(BoolInputs.Checkbox))
                        .Builder(m => m.SwitchField, s => s.ToBoolInput().Variant(BoolInputs.Switch))
                        .Builder(m => m.ToggleField, s => s.ToBoolInput().Variant(BoolInputs.Toggle))
                        .Builder(m => m.DateField, s => s.ToDateTimeInput().Variant(DateTimeInputs.Date))
                        .Builder(m => m.DateTimeField, s => s.ToDateTimeInput().Variant(DateTimeInputs.DateTime))
                        .Builder(m => m.TimeField, s => s.ToDateTimeInput().Variant(DateTimeInputs.Time))
                        .Builder(m => m.SelectField, s => s.ToSelectInput())
                        .Builder(m => m.MultiSelectField, s => s.ToSelectInput().List())
                        .Builder(m => m.AsyncSelectField, s => s.ToAsyncSelectInput(QueryOptions, LookupOption, "Search options..."))
                        .Builder(m => m.ColorField, s => s.ToColorInput())
                        .Builder(m => m.CodeField, s => s.ToCodeInput().Language(Languages.Javascript))
                        .Builder(m => m.RatingField, s => s.ToFeedbackInput().Variant(FeedbackInputs.Stars))
                        .Builder(m => m.ThumbsField, s => s.ToFeedbackInput().Variant(FeedbackInputs.Thumbs))
                        .Builder(m => m.EmojiField, s => s.ToFeedbackInput().Variant(FeedbackInputs.Emojis))
                )
                .Width(1 / 3f)
                .Title("Medium Scale - All Inputs")
                | new Card(
                    largeModel.ToForm()
                        .Large()
                        .Group("Text Inputs", open: true,
                            m => m.TextField,
                            m => m.EmailField,
                            m => m.PasswordField,
                            m => m.SearchField,
                            m => m.TelField,
                            m => m.UrlField,
                            m => m.TextAreaField)
                        .Group("Number Inputs",
                            m => m.IntegerField,
                            m => m.DecimalField)
                        .Group("Bool Inputs",
                            m => m.CheckboxField,
                            m => m.SwitchField,
                            m => m.ToggleField)
                        .Group("DateTime Inputs",
                            m => m.DateField,
                            m => m.DateTimeField,
                            m => m.TimeField)
                        .Group("Select Inputs",
                            m => m.SelectField,
                            m => m.MultiSelectField,
                            m => m.AsyncSelectField)
                        .Group("Other Inputs",
                            m => m.ColorField,
                            m => m.CodeField,
                            m => m.RatingField,
                            m => m.ThumbsField,
                            m => m.EmojiField)
                        .Builder(m => m.TextField, s => s.ToTextInput())
                        .Builder(m => m.EmailField, s => s.ToEmailInput())
                        .Builder(m => m.PasswordField, s => s.ToPasswordInput())
                        .Builder(m => m.SearchField, s => s.ToSearchInput())
                        .Builder(m => m.TelField, s => s.ToTelInput())
                        .Builder(m => m.UrlField, s => s.ToUrlInput())
                        .Builder(m => m.TextAreaField, s => s.ToTextAreaInput())
                        .Builder(m => m.CheckboxField, s => s.ToBoolInput().Variant(BoolInputs.Checkbox))
                        .Builder(m => m.SwitchField, s => s.ToBoolInput().Variant(BoolInputs.Switch))
                        .Builder(m => m.ToggleField, s => s.ToBoolInput().Variant(BoolInputs.Toggle))
                        .Builder(m => m.DateField, s => s.ToDateTimeInput().Variant(DateTimeInputs.Date))
                        .Builder(m => m.DateTimeField, s => s.ToDateTimeInput().Variant(DateTimeInputs.DateTime))
                        .Builder(m => m.TimeField, s => s.ToDateTimeInput().Variant(DateTimeInputs.Time))
                        .Builder(m => m.SelectField, s => s.ToSelectInput())
                        .Builder(m => m.MultiSelectField, s => s.ToSelectInput().List())
                        .Builder(m => m.AsyncSelectField, s => s.ToAsyncSelectInput(QueryOptions, LookupOption, "Search options..."))
                        .Builder(m => m.ColorField, s => s.ToColorInput())
                        .Builder(m => m.CodeField, s => s.ToCodeInput().Language(Languages.Sql))
                        .Builder(m => m.RatingField, s => s.ToFeedbackInput().Variant(FeedbackInputs.Stars))
                        .Builder(m => m.ThumbsField, s => s.ToFeedbackInput().Variant(FeedbackInputs.Thumbs))
                        .Builder(m => m.EmojiField, s => s.ToFeedbackInput().Variant(FeedbackInputs.Emojis))
                )
                .Width(1 / 3f)
                .Title("Large Scale - All Inputs");
    }
}