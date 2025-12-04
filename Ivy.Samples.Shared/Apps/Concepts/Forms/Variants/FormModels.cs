using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace Ivy.Samples.Shared.Apps.Concepts.Forms.Variants;

#region Enums

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

#endregion

#region Records

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

public record FormValidationModel
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

#endregion
