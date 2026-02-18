using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

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
    string TextField,
    string EmailField,
    string PasswordField,
    string SearchField,
    string TelField,
    string UrlField,
    string TextAreaField,
    int IntegerField,
    double DecimalField,
    bool CheckboxField,
    bool SwitchField,
    bool ToggleField,
    DateTime DateField,
    DateTime DateTimeField,
    DateTime TimeField,
    UserRole SelectField,
    List<Fruits> MultiSelectField,
    string? AsyncSelectField,
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
    string Name,
    string Password,
    bool IsAwesome,
    DateTime BirthDate,
    int Height,
    int UserId = 123,
    Gender Gender = Gender.Male,
    string Json = "{}",
    List<Fruits> FavoriteFruits = null!
);

public class DisplayExample
{
    [Display(
        Name = "Custom Name",
        Description = "This is a custom description.",
        Order = 2,
        Prompt = "Enter value here")]
    public string CustomDisplayString { get; set; } = "Foo";

    [Display(GroupName = "Extras")]
    public string Foo { get; set; } = "Foo Value";

    [Display(GroupName = "Extras")]
    public string Bar { get; set; } = "Bar Value";
}

public class FormatsExample
{
    [EmailAddress(ErrorMessage = "Custom: Invalid email address")]
    public string? Field1 { get; set; }

    [CreditCard(ErrorMessage = "Custom: Invalid credit card number")]
    public string? Field2 { get; set; }

    [Url(ErrorMessage = "Custom: Invalid URL")]
    public string? Field3 { get; set; }

    [Phone(ErrorMessage = "Custom: Invalid phone number")]
    public string? Field4 { get; set; }

    [RegularExpression(@"\d{4,5}", ErrorMessage = "Custom: Invalid format")]
    public string? Field5 { get; set; }
}

public class NumbersExample
{
    [Range(0, 100, ErrorMessage = "Custom error: Value must be between 0 and 100")]
    public int Range { get; set; }
}

public class StringsExample
{
    [ScaffoldColumn(false)]
    public string IgnoredString1 { get; set; } = "";

    public string NormalString { get; set; } = "Hello";

    public string? NullableString { get; set; } = null;

    [Required]
    public string RequiredString1 { get; set; } = "";

    [Required]
    public string? RequiredString2 { get; set; }

    [MinLength(5, ErrorMessage = "Custom error: Minimum length is 5")]
    [Display(Description = "Must be at least 5 characters long")]
    public string MinLengthString { get; set; } = "";

    [MaxLength(5, ErrorMessage = "Custom error: Maximum length is 5")]
    [Display(Description = "Must be at most 5 characters long")]
    public string MaxLengthString { get; set; } = "";

    [StringLength(10, MinimumLength = 5, ErrorMessage = "Custom error: Must be between 5 and 10 characters long")]
    [Display(Description = "Must be between 5 and 10 characters long")]
    public string StringLengthString { get; set; } = "";

    [Length(5, 10, ErrorMessage = "Custom error: Must be between 5 and 10 characters long")]
    [Display(Description = "Must be between 5 and 10 characters long")]
    public string LengthString { get; set; } = "";
}

public record FormValidationExamples
{
    [Display(Name = "User Name", Description = "Enter your full name", Prompt = "John Doe", Order = 1)]
    [Required(ErrorMessage = "Name is required")]
    [Length(2, 50, ErrorMessage = "Name must be between 2 and 50 characters")]
    public string Name { get; init; } = string.Empty;

    [Display(Name = "Email Address", Description = "Your primary contact email", Order = 2)]
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; init; } = string.Empty;

    [Display(Name = "Phone", Description = "Mobile or landline", Prompt = "+1-234-567-8900", Order = 3)]
    [Phone]
    [StringLength(20, MinimumLength = 10)]
    public string? PhoneNumber { get; init; }

    [Display(Name = "Age", Description = "Must be 18-120", Order = 4)]
    [Range(18, 120)]
    public int Age { get; init; } = 18;

    [Display(Name = "Website", Order = 5)]
    [Url]
    public string? Website { get; init; }

    [Display(Name = "Bio", Description = "Tell us about yourself", Order = 6)]
    [MinLength(10)]
    [MaxLength(500)]
    public string Bio { get; init; } = string.Empty;

    [Display(Name = "Country", Description = "Select your country", Order = 7)]
    [AllowedValues("USA", "Canada", "UK", "Germany", "France")]
    public string Country { get; init; } = "USA";

    [Display(Name = "Interests", Description = "Pick multiple", Order = 8)]
    [AllowedValues("Technology", "Sports", "Music", "Art", "Travel")]
    [MinLength(1)]
    public string[] Interests { get; init; } = Array.Empty<string>();

    [Display(Name = "Reference Code", Description = "Format: YYYY-MM-DD", Prompt = "2024-01-15", Order = 9)]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Must match format YYYY-MM-DD")]
    public string? ReferenceCode { get; init; }

    [Display(GroupName = "File Upload", Name = "Profile Picture", Order = 10)]
    [Range(1, 10485760)]
    public long MaxImageSize { get; init; } = 2 * 1024 * 1024;

    [Display(GroupName = "File Upload", Name = "Allowed Types", Order = 11)]
    [AllowedValues("image/png", "image/jpeg", "image/webp")]
    public string AcceptedImageTypes { get; init; } = "image/jpeg";

    [Display(GroupName = "Preferences", Name = "Newsletter", Description = "Receive weekly updates", Order = 12)]
    public bool SubscribeNewsletter { get; init; } = false;

    [Display(GroupName = "Preferences", Name = "Theme", Order = 13)]
    [AllowedValues("Light", "Dark", "Auto")]
    public string Theme { get; init; } = "Auto";

    [Display(Name = "Credit Card", Order = 14)]
    [CreditCard]
    [StringLength(19, MinimumLength = 13)]
    public string? CreditCardNumber { get; init; }

    [Display(Name = "ZIP Code", Order = 15)]
    [RegularExpression(@"^\d{5}(-\d{4})?$")]
    public string? ZipCode { get; init; }

    [Display(Name = "Password", Order = 16)]
    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; init; } = string.Empty;

    [Display(Name = "Rating", Description = "Rate from 1-5 stars", Order = 17)]
    [Range(1.0, 5.0)]
    public decimal Rating { get; init; } = 3.0m;

    [Display(Name = "Birthdate", Order = 18)]
    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; init; }

    [Display(Name = "Appointment Time", Order = 19)]
    [DataType(DataType.DateTime)]
    public DateTime? AppointmentDateTime { get; init; }

    [Display(Name = "Comments", Description = "Optional feedback", Order = 20)]
    [DataType(DataType.MultilineText)]
    [MaxLength(1000)]
    public string? Comments { get; init; }
}

[App(icon: Icons.Clipboard, path: ["Concepts"], searchHints: ["inputs", "fields", "validation", "submission", "data-entry", "controls", "scaffolding", "forms"])]
public class FormApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Tabs(
            new Tab("Form", new FormExample()),
            new Tab("Scaffolding", new FormScaffoldingExample()),
            new Tab("Validation", new FormValidationExample())
        ).Variant(TabsVariant.Content);
    }
}

public class FormExample : ViewBase
{
    public override object? Build()
    {
        var model = UseState(() => new UserModel("Niels Bosma", "1234156", true, DateTime.Parse("1982-07-17"), 183));

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
               | BuildFormSizeDemo(smallModel, mediumModel, largeModel);
    }

    private object BuildFormSizeDemo(IState<ComprehensiveInputModel> smallModel, IState<ComprehensiveInputModel> mediumModel, IState<ComprehensiveInputModel> largeModel)
    {
        var sampleOptions = new[] { "Option 1", "Option 2", "Option 3", "Option 4", "Option 5", "Another Option", "Yet Another Option", "Final Option" };

        QueryResult<Option<string?>[]> QueryOptions(IViewContext context, string query)
        {
            var lowerQuery = query.ToLowerInvariant();
            return context.UseQuery<Option<string?>[], (string, string)>(
                key: ("FormApp.QueryOptions", query),
                fetcher: _ => Task.FromResult(sampleOptions
                    .Where(o => o.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase))
                    .Select(o => new Option<string?>(o, o))
                    .ToArray()));
        }

        QueryResult<Option<string?>?> LookupOption(IViewContext context, string? value)
        {
            return context.UseQuery<Option<string?>?, (string, string?)>(
                key: ("FormApp.LookupOption", value),
                fetcher: _ =>
                {
                    if (value == null) return Task.FromResult<Option<string?>?>(null);
                    return Task.FromResult<Option<string?>?>(new Option<string?>(value, value));
                });
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
                        .Description(m => m.TelField, "Enter your phone number.")
                        .Builder(m => m.UrlField, s => s.ToUrlInput())
                        .Description(m => m.UrlField, "Enter your website URL.")
                        .Builder(m => m.TextAreaField, s => s.ToTextareaInput())
                        .Description(m => m.TextAreaField, "Enter a detailed description.")
                        .Builder(m => m.CheckboxField, s => s.ToBoolInput().Variant(BoolInputVariants.Checkbox))
                        .Builder(m => m.SwitchField, s => s.ToBoolInput().Variant(BoolInputVariants.Switch))
                        .Builder(m => m.ToggleField, s => s.ToBoolInput().Variant(BoolInputVariants.Toggle))
                        .Builder(m => m.DateField, s => s.ToDateTimeInput().Variant(DateTimeInputVariants.Date))
                        .Builder(m => m.DateTimeField, s => s.ToDateTimeInput().Variant(DateTimeInputVariants.DateTime))
                        .Builder(m => m.TimeField, s => s.ToDateTimeInput().Variant(DateTimeInputVariants.Time))
                        .Builder(m => m.SelectField, s => s.ToSelectInput())
                        .Builder(m => m.MultiSelectField, s => s.ToSelectInput().List())
                        .Builder(m => m.AsyncSelectField, s => s.ToAsyncSelectInput(QueryOptions, LookupOption, "Search options..."))
                        .Builder(m => m.ColorField, s => s.ToColorInput())
                        .Description(m => m.ColorField, "Pick a color that suits you.")
                        .Builder(m => m.CodeField, s => s.ToCodeInput().Language(Languages.Json))
                        .Builder(m => m.RatingField, s => s.ToFeedbackInput().Variant(FeedbackInputVariants.Stars))
                        .Builder(m => m.ThumbsField, s => s.ToFeedbackInput().Variant(FeedbackInputVariants.Thumbs))
                        .Builder(m => m.EmojiField, s => s.ToFeedbackInput().Variant(FeedbackInputVariants.Emojis))
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
                        .Description(m => m.TelField, "Enter your phone number.")
                        .Builder(m => m.UrlField, s => s.ToUrlInput())
                        .Description(m => m.UrlField, "Enter your website URL.")
                        .Builder(m => m.TextAreaField, s => s.ToTextareaInput())
                        .Description(m => m.TextAreaField, "Enter a detailed description.")
                        .Builder(m => m.CheckboxField, s => s.ToBoolInput().Variant(BoolInputVariants.Checkbox))
                        .Builder(m => m.SwitchField, s => s.ToBoolInput().Variant(BoolInputVariants.Switch))
                        .Builder(m => m.ToggleField, s => s.ToBoolInput().Variant(BoolInputVariants.Toggle))
                        .Builder(m => m.DateField, s => s.ToDateTimeInput().Variant(DateTimeInputVariants.Date))
                        .Builder(m => m.DateTimeField, s => s.ToDateTimeInput().Variant(DateTimeInputVariants.DateTime))
                        .Builder(m => m.TimeField, s => s.ToDateTimeInput().Variant(DateTimeInputVariants.Time))
                        .Builder(m => m.SelectField, s => s.ToSelectInput())
                        .Builder(m => m.MultiSelectField, s => s.ToSelectInput().List())
                        .Builder(m => m.AsyncSelectField, s => s.ToAsyncSelectInput(QueryOptions, LookupOption, "Search options..."))
                        .Builder(m => m.ColorField, s => s.ToColorInput())
                        .Description(m => m.ColorField, "Pick a color that suits you.")
                        .Builder(m => m.CodeField, s => s.ToCodeInput().Language(Languages.Javascript))
                        .Builder(m => m.RatingField, s => s.ToFeedbackInput().Variant(FeedbackInputVariants.Stars))
                        .Builder(m => m.ThumbsField, s => s.ToFeedbackInput().Variant(FeedbackInputVariants.Thumbs))
                        .Builder(m => m.EmojiField, s => s.ToFeedbackInput().Variant(FeedbackInputVariants.Emojis))
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
                        .Description(m => m.TelField, "Enter your phone number.")
                        .Builder(m => m.UrlField, s => s.ToUrlInput())
                        .Description(m => m.UrlField, "Enter your website URL.")
                        .Builder(m => m.TextAreaField, s => s.ToTextareaInput())
                        .Description(m => m.TextAreaField, "Enter a detailed description.")
                        .Builder(m => m.CheckboxField, s => s.ToBoolInput().Variant(BoolInputVariants.Checkbox))
                        .Builder(m => m.SwitchField, s => s.ToBoolInput().Variant(BoolInputVariants.Switch))
                        .Builder(m => m.ToggleField, s => s.ToBoolInput().Variant(BoolInputVariants.Toggle))
                        .Builder(m => m.DateField, s => s.ToDateTimeInput().Variant(DateTimeInputVariants.Date))
                        .Builder(m => m.DateTimeField, s => s.ToDateTimeInput().Variant(DateTimeInputVariants.DateTime))
                        .Builder(m => m.TimeField, s => s.ToDateTimeInput().Variant(DateTimeInputVariants.Time))
                        .Builder(m => m.SelectField, s => s.ToSelectInput())
                        .Builder(m => m.MultiSelectField, s => s.ToSelectInput().List())
                        .Builder(m => m.AsyncSelectField, s => s.ToAsyncSelectInput(QueryOptions, LookupOption, "Search options..."))
                        .Builder(m => m.ColorField, s => s.ToColorInput())
                        .Description(m => m.ColorField, "Pick a color that suits you.")
                        .Builder(m => m.CodeField, s => s.ToCodeInput().Language(Languages.Sql))
                        .Builder(m => m.RatingField, s => s.ToFeedbackInput().Variant(FeedbackInputVariants.Stars))
                        .Builder(m => m.ThumbsField, s => s.ToFeedbackInput().Variant(FeedbackInputVariants.Thumbs))
                        .Builder(m => m.EmojiField, s => s.ToFeedbackInput().Variant(FeedbackInputVariants.Emojis))
                )
                .Width(1 / 3f)
                .Title("Large Scale - All Inputs");
    }
}

public class FormScaffoldingExample : ViewBase
{
    public override object? Build()
    {
        var formatsExample = UseState(() => new FormatsExample());
        var displayExample = UseState(() => new DisplayExample());
        var stringsExample = UseState(() => new StringsExample());
        var numbersExample = UseState(() => new NumbersExample());

        var formatsGrid = Layout.Grid().Columns(3).Gap(10)
                          | formatsExample.ToForm()
                          | formatsExample.ToDetails().Builder(NullBuilder);

        var displayGrid = Layout.Grid().Columns(3).Gap(10)
                          | displayExample.ToForm()
                          | displayExample.ToDetails().Builder(NullBuilder);

        var stringsGrid = Layout.Grid().Columns(3).Gap(10)
                          | stringsExample.ToForm()
                          | stringsExample.ToDetails().Builder(NullBuilder);

        var numbersGrid = Layout.Grid().Columns(3).Gap(10)
                          | numbersExample.ToForm()
                          | numbersExample.ToDetails();

        return Layout.Vertical()
               | Text.H2("Display")
               | displayGrid
               | Text.H2("Strings")
               | stringsGrid
               | Text.H2("Formats")
               | formatsGrid
               | Text.H2("Numbers")
               | numbersGrid;
    }

    private static object? NullBuilder(object? e)
    {
        if (e == null) return Text.Muted("(null)");
        if (e is string s && string.IsNullOrEmpty(s)) return Text.Muted("(empty string)");
        return e;
    }
}

public class FormValidationExample : ViewBase
{
    public override object? Build()
    {
        var model = UseState(() => new FormValidationExamples());
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            if (!string.IsNullOrEmpty(model.Value.Name))
            {
                client.Toast($"Form submitted successfully for {model.Value.Name}!");
            }
        }, model);

        var countryOptions = new[] { "USA", "Canada", "UK", "Germany", "France" }.ToOptions();
        var interestOptions = new[] { "Technology", "Sports", "Music", "Art", "Travel" }.ToOptions();
        var themeOptions = new[] { "Light", "Dark", "Auto" }.ToOptions();
        var imageTypeOptions = new[] { "image/png", "image/jpeg", "image/webp" }.ToOptions();

        var form = model.ToForm("Submit Registration")
            .Builder(m => m.Bio, s => s.ToTextareaInput())
            .Builder(m => m.Country, s => s.ToSelectInput(countryOptions))
            .Builder(m => m.Interests, s => s.ToSelectInput(interestOptions).List())
            .Builder(m => m.Theme, s => s.ToSelectInput(themeOptions))
            .Builder(m => m.AcceptedImageTypes, s => s.ToSelectInput(imageTypeOptions))
            .Builder(m => m.Comments, s => s.ToTextareaInput())
            .Builder(m => m.BirthDate, s => s.ToDateTimeInput())
            .Builder(m => m.AppointmentDateTime, s => s.ToDateTimeInput())
            .Builder(m => m.Password, s => s.ToPasswordInput())
            .Builder(m => m.Website, s => s.ToUrlInput())
            .Builder(m => m.PhoneNumber, s => s.ToTelInput())
            .Validate<DateTime?>(m => m.BirthDate, birthDate =>
                (birthDate == null || birthDate <= DateTime.Now, "Birth date cannot be in the future"))
            .Validate<string>(m => m.Bio, bio =>
                (string.IsNullOrEmpty(bio) || !bio.Contains("spam"), "Bio cannot contain spam content"));

        return Layout.Vertical()
            | (Layout.Horizontal()
                | new Card(form)
                    .Width(1 / 2f)
                    .Title("Enhanced Form Validation")
                | new Card(
                    Layout.Vertical()
                        | Text.H4("Current Form Data")
                        | model.ToDetails()
                ).Width(1 / 2f)
                    .Title("Form State"));
    }
}
