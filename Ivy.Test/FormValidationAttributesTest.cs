using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Ivy.Views.Forms;

namespace Ivy.Test;

#region Test Models

/// <summary>
/// Model to test all supported DataAnnotation validation attributes.
/// </summary>
public class ValidationAttributesTestModel
{
    // Required validation
    [Required(ErrorMessage = "Name is required")]
    public string RequiredField { get; set; } = string.Empty;

    // MinLength validation
    [MinLength(5, ErrorMessage = "Minimum 5 characters")]
    public string MinLengthField { get; set; } = string.Empty;

    // MaxLength validation
    [MaxLength(10, ErrorMessage = "Maximum 10 characters")]
    public string MaxLengthField { get; set; } = string.Empty;

    // StringLength validation (with both min and max)
    [StringLength(20, MinimumLength = 5, ErrorMessage = "Must be 5-20 characters")]
    public string StringLengthField { get; set; } = string.Empty;

    // Length validation (.NET 8+)
    [Length(3, 15, ErrorMessage = "Must be 3-15 characters")]
    public string LengthField { get; set; } = string.Empty;

    // Range validation (int)
    [Range(18, 120, ErrorMessage = "Must be between 18 and 120")]
    public int RangeIntField { get; set; }

    // Range validation (double)
    [Range(0.0, 100.0, ErrorMessage = "Must be between 0 and 100")]
    public double RangeDoubleField { get; set; }

    // EmailAddress validation
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? EmailField { get; set; }

    // Phone validation
    [Phone(ErrorMessage = "Invalid phone format")]
    public string? PhoneField { get; set; }

    // Url validation
    [Url(ErrorMessage = "Invalid URL format")]
    public string? UrlField { get; set; }

    // CreditCard validation
    [CreditCard(ErrorMessage = "Invalid credit card number")]
    public string? CreditCardField { get; set; }

    // RegularExpression validation
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Must match YYYY-MM-DD format")]
    public string? RegexField { get; set; }

    // AllowedValues validation
    [AllowedValues("Red", "Green", "Blue", ErrorMessage = "Must be Red, Green, or Blue")]
    public string? AllowedValuesField { get; set; }
}

/// <summary>
/// Model to test Display attribute properties.
/// </summary>
public class FormDisplayAttributeTestModel
{
    [Display(Name = "Custom Name")]
    public string NameField { get; set; } = string.Empty;

    [Display(Description = "This is a description")]
    public string DescriptionField { get; set; } = string.Empty;

    [Display(GroupName = "Group A")]
    public string GroupNameField { get; set; } = string.Empty;

    [Display(Prompt = "Enter placeholder")]
    public string PromptField { get; set; } = string.Empty;

    [Display(Order = 5)]
    public string OrderField { get; set; } = string.Empty;

    [Display(Name = "Full Display", Description = "All properties", GroupName = "Complete", Prompt = "Type here", Order = 10)]
    public string AllPropertiesField { get; set; } = string.Empty;

    public string NoDisplayField { get; set; } = string.Empty;
}

/// <summary>
/// Model to test DataType attribute.
/// </summary>
public class DataTypeTestModel
{
    [DataType(DataType.Password)]
    public string PasswordField { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime DateField { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime DateTimeField { get; set; }

    [DataType(DataType.MultilineText)]
    public string MultilineField { get; set; } = string.Empty;

    [DataType(DataType.EmailAddress)]
    public string EmailDataTypeField { get; set; } = string.Empty;

    [DataType(DataType.PhoneNumber)]
    public string PhoneDataTypeField { get; set; } = string.Empty;

    [DataType(DataType.Url)]
    public string UrlDataTypeField { get; set; } = string.Empty;
}

#endregion

public class FormValidationAttributesTest
{
    #region Required Attribute Tests

    [Fact]
    public void Required_ShouldFail_WhenValueIsNull()
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.RequiredField));
        var (isValid, errorMessage) = validators[0](null);

        Assert.False(isValid);
        Assert.Contains("required", errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Required_ShouldFail_WhenValueIsEmpty()
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.RequiredField));
        var (isValid, errorMessage) = validators[0](string.Empty);

        Assert.False(isValid);
        Assert.Contains("required", errorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Required_ShouldPass_WhenValueIsProvided()
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.RequiredField));
        var (isValid, _) = validators[0]("valid value");

        Assert.True(isValid);
    }

    #endregion

    #region MinLength Attribute Tests

    [Fact]
    public void MinLength_ShouldFail_WhenValueIsTooShort()
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.MinLengthField));
        var (isValid, errorMessage) = validators[0]("abc");

        Assert.False(isValid);
        Assert.Contains("5", errorMessage);
    }

    [Fact]
    public void MinLength_ShouldPass_WhenValueMeetsMinimum()
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.MinLengthField));
        var (isValid, _) = validators[0]("abcde");

        Assert.True(isValid);
    }

    #endregion

    #region MaxLength Attribute Tests

    [Fact]
    public void MaxLength_ShouldFail_WhenValueIsTooLong()
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.MaxLengthField));
        var (isValid, errorMessage) = validators[0]("12345678901");

        Assert.False(isValid);
        Assert.Contains("10", errorMessage);
    }

    [Fact]
    public void MaxLength_ShouldPass_WhenValueIsWithinLimit()
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.MaxLengthField));
        var (isValid, _) = validators[0]("1234567890");

        Assert.True(isValid);
    }

    #endregion

    #region StringLength Attribute Tests

    [Fact]
    public void StringLength_ShouldFail_WhenValueIsTooShort()
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.StringLengthField));
        var (isValid, _) = validators[0]("abc");

        Assert.False(isValid);
    }

    [Fact]
    public void StringLength_ShouldFail_WhenValueIsTooLong()
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.StringLengthField));
        var (isValid, _) = validators[0]("123456789012345678901");

        Assert.False(isValid);
    }

    [Fact]
    public void StringLength_ShouldPass_WhenValueIsWithinRange()
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.StringLengthField));
        var (isValid, _) = validators[0]("validstring");

        Assert.True(isValid);
    }

    #endregion

    #region Length Attribute Tests

    [Fact]
    public void Length_ShouldFail_WhenValueIsTooShort()
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.LengthField));
        var (isValid, _) = validators[0]("ab");

        Assert.False(isValid);
    }

    [Fact]
    public void Length_ShouldFail_WhenValueIsTooLong()
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.LengthField));
        var (isValid, _) = validators[0]("1234567890123456");

        Assert.False(isValid);
    }

    [Fact]
    public void Length_ShouldPass_WhenValueIsWithinRange()
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.LengthField));
        var (isValid, _) = validators[0]("validtext");

        Assert.True(isValid);
    }

    #endregion

    #region Range Attribute Tests

    [Fact]
    public void Range_Int_ShouldFail_WhenValueIsBelowMinimum()
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.RangeIntField));
        var (isValid, _) = validators[0](17);

        Assert.False(isValid);
    }

    [Fact]
    public void Range_Int_ShouldFail_WhenValueIsAboveMaximum()
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.RangeIntField));
        var (isValid, _) = validators[0](121);

        Assert.False(isValid);
    }

    [Fact]
    public void Range_Int_ShouldPass_WhenValueIsWithinRange()
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.RangeIntField));
        var (isValid, _) = validators[0](25);

        Assert.True(isValid);
    }

    [Fact]
    public void Range_Double_ShouldFail_WhenValueIsOutOfRange()
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.RangeDoubleField));
        var (isValid, _) = validators[0](150.0);

        Assert.False(isValid);
    }

    [Fact]
    public void Range_Double_ShouldPass_WhenValueIsWithinRange()
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.RangeDoubleField));
        var (isValid, _) = validators[0](50.5);

        Assert.True(isValid);
    }

    #endregion

    #region EmailAddress Attribute Tests

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    public void EmailAddress_ShouldFail_WhenFormatIsInvalid(string invalidEmail)
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.EmailField));
        var (isValid, _) = validators[0](invalidEmail);

        Assert.False(isValid);
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.org")]
    [InlineData("user+tag@example.co.uk")]
    public void EmailAddress_ShouldPass_WhenFormatIsValid(string validEmail)
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.EmailField));
        var (isValid, _) = validators[0](validEmail);

        Assert.True(isValid);
    }

    #endregion

    #region Phone Attribute Tests

    [Theory]
    [InlineData("+1-234-567-8900")]
    [InlineData("(555) 123-4567")]
    [InlineData("555.123.4567")]
    public void Phone_ShouldPass_WhenFormatIsValid(string validPhone)
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.PhoneField));
        var (isValid, _) = validators[0](validPhone);

        Assert.True(isValid);
    }

    #endregion

    #region Url Attribute Tests

    [Theory]
    [InlineData("not a url")]
    [InlineData("missing-protocol.com")]
    [InlineData("://noprotocol.com")]
    public void Url_ShouldFail_WhenFormatIsInvalid(string invalidUrl)
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.UrlField));
        var (isValid, _) = validators[0](invalidUrl);

        Assert.False(isValid);
    }

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://www.example.org/path")]
    [InlineData("https://sub.domain.com/path?query=value")]
    public void Url_ShouldPass_WhenFormatIsValid(string validUrl)
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.UrlField));
        var (isValid, _) = validators[0](validUrl);

        Assert.True(isValid);
    }

    #endregion

    #region CreditCard Attribute Tests

    [Theory]
    [InlineData("1234")]
    [InlineData("not-a-card")]
    [InlineData("1234567890123456789")]
    public void CreditCard_ShouldFail_WhenFormatIsInvalid(string invalidCard)
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.CreditCardField));
        var (isValid, _) = validators[0](invalidCard);

        Assert.False(isValid);
    }

    [Theory]
    [InlineData("4111111111111111")] // Valid Visa test number
    [InlineData("5500000000000004")] // Valid Mastercard test number
    public void CreditCard_ShouldPass_WhenFormatIsValid(string validCard)
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.CreditCardField));
        var (isValid, _) = validators[0](validCard);

        Assert.True(isValid);
    }

    #endregion

    #region RegularExpression Attribute Tests

    [Theory]
    [InlineData("2024-1-1")]
    [InlineData("24-01-15")]
    [InlineData("not-a-date")]
    public void RegularExpression_ShouldFail_WhenPatternDoesNotMatch(string invalidValue)
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.RegexField));
        var (isValid, _) = validators[0](invalidValue);

        Assert.False(isValid);
    }

    [Theory]
    [InlineData("2024-01-15")]
    [InlineData("1999-12-31")]
    [InlineData("2030-06-01")]
    public void RegularExpression_ShouldPass_WhenPatternMatches(string validValue)
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.RegexField));
        var (isValid, _) = validators[0](validValue);

        Assert.True(isValid);
    }

    #endregion

    #region AllowedValues Attribute Tests

    [Theory]
    [InlineData("Yellow")]
    [InlineData("Purple")]
    [InlineData("red")] // Case sensitive
    public void AllowedValues_ShouldFail_WhenValueIsNotAllowed(string invalidValue)
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.AllowedValuesField));
        var (isValid, _) = validators[0](invalidValue);

        Assert.False(isValid);
    }

    [Theory]
    [InlineData("Red")]
    [InlineData("Green")]
    [InlineData("Blue")]
    public void AllowedValues_ShouldPass_WhenValueIsAllowed(string validValue)
    {
        var validators = GetValidators<ValidationAttributesTestModel>(nameof(ValidationAttributesTestModel.AllowedValuesField));
        var (isValid, _) = validators[0](validValue);

        Assert.True(isValid);
    }

    #endregion

    #region Display Attribute Tests

    [Fact]
    public void Display_ShouldExtractName()
    {
        var displayInfo = GetDisplayInfo<FormDisplayAttributeTestModel>(nameof(FormDisplayAttributeTestModel.NameField));

        Assert.Equal("Custom Name", displayInfo.Name);
    }

    [Fact]
    public void Display_ShouldExtractDescription()
    {
        var displayInfo = GetDisplayInfo<FormDisplayAttributeTestModel>(nameof(FormDisplayAttributeTestModel.DescriptionField));

        Assert.Equal("This is a description", displayInfo.Description);
    }

    [Fact]
    public void Display_ShouldExtractGroupName()
    {
        var displayInfo = GetDisplayInfo<FormDisplayAttributeTestModel>(nameof(FormDisplayAttributeTestModel.GroupNameField));

        Assert.Equal("Group A", displayInfo.GroupName);
    }

    [Fact]
    public void Display_ShouldExtractPrompt()
    {
        var displayInfo = GetDisplayInfo<FormDisplayAttributeTestModel>(nameof(FormDisplayAttributeTestModel.PromptField));

        Assert.Equal("Enter placeholder", displayInfo.Prompt);
    }

    [Fact]
    public void Display_ShouldExtractOrder()
    {
        var displayInfo = GetDisplayInfo<FormDisplayAttributeTestModel>(nameof(FormDisplayAttributeTestModel.OrderField));

        Assert.Equal(5, displayInfo.Order);
    }

    [Fact]
    public void Display_ShouldExtractAllProperties()
    {
        var displayInfo = GetDisplayInfo<FormDisplayAttributeTestModel>(nameof(FormDisplayAttributeTestModel.AllPropertiesField));

        Assert.Equal("Full Display", displayInfo.Name);
        Assert.Equal("All properties", displayInfo.Description);
        Assert.Equal("Complete", displayInfo.GroupName);
        Assert.Equal("Type here", displayInfo.Prompt);
        Assert.Equal(10, displayInfo.Order);
    }

    [Fact]
    public void Display_ShouldReturnEmptyWhenNoAttribute()
    {
        var displayInfo = GetDisplayInfo<FormDisplayAttributeTestModel>(nameof(FormDisplayAttributeTestModel.NoDisplayField));

        Assert.Null(displayInfo.Name);
        Assert.Null(displayInfo.Description);
        Assert.Null(displayInfo.GroupName);
        Assert.Null(displayInfo.Prompt);
        Assert.Null(displayInfo.Order);
    }

    #endregion

    #region Range Info Tests

    [Fact]
    public void GetRangeInfo_ShouldExtractMinAndMax()
    {
        var propertyInfo = typeof(ValidationAttributesTestModel).GetProperty(nameof(ValidationAttributesTestModel.RangeIntField))!;
        var rangeInfo = FormHelpers.GetRangeInfo(propertyInfo);

        Assert.Equal(18, rangeInfo.Min);
        Assert.Equal(120, rangeInfo.Max);
    }

    [Fact]
    public void GetRangeInfo_ShouldReturnNullWhenNoAttribute()
    {
        var propertyInfo = typeof(ValidationAttributesTestModel).GetProperty(nameof(ValidationAttributesTestModel.RequiredField))!;
        var rangeInfo = FormHelpers.GetRangeInfo(propertyInfo);

        Assert.Null(rangeInfo.Min);
        Assert.Null(rangeInfo.Max);
    }

    #endregion

    #region MaxLength Extraction Tests

    [Fact]
    public void GetMaxLength_ShouldExtractFromMaxLengthAttribute()
    {
        var propertyInfo = typeof(ValidationAttributesTestModel).GetProperty(nameof(ValidationAttributesTestModel.MaxLengthField))!;
        var maxLength = FormHelpers.GetMaxLength(propertyInfo);

        Assert.Equal(10, maxLength);
    }

    [Fact]
    public void GetMaxLength_ShouldExtractFromStringLengthAttribute()
    {
        var propertyInfo = typeof(ValidationAttributesTestModel).GetProperty(nameof(ValidationAttributesTestModel.StringLengthField))!;
        var maxLength = FormHelpers.GetMaxLength(propertyInfo);

        Assert.Equal(20, maxLength);
    }

    [Fact]
    public void GetMaxLength_ShouldExtractFromLengthAttribute()
    {
        var propertyInfo = typeof(ValidationAttributesTestModel).GetProperty(nameof(ValidationAttributesTestModel.LengthField))!;
        var maxLength = FormHelpers.GetMaxLength(propertyInfo);

        Assert.Equal(15, maxLength);
    }

    [Fact]
    public void GetMaxLength_ShouldReturnNullWhenNoAttribute()
    {
        var propertyInfo = typeof(ValidationAttributesTestModel).GetProperty(nameof(ValidationAttributesTestModel.RequiredField))!;
        var maxLength = FormHelpers.GetMaxLength(propertyInfo);

        Assert.Null(maxLength);
    }

    #endregion

    #region DataType Attribute Tests

    [Fact]
    public void DataType_Password_ShouldBeDetectable()
    {
        var propertyInfo = typeof(DataTypeTestModel).GetProperty(nameof(DataTypeTestModel.PasswordField))!;
        var dataTypeAttr = propertyInfo.GetCustomAttribute<DataTypeAttribute>();

        Assert.NotNull(dataTypeAttr);
        Assert.Equal(DataType.Password, dataTypeAttr.DataType);
    }

    [Fact]
    public void DataType_Date_ShouldBeDetectable()
    {
        var propertyInfo = typeof(DataTypeTestModel).GetProperty(nameof(DataTypeTestModel.DateField))!;
        var dataTypeAttr = propertyInfo.GetCustomAttribute<DataTypeAttribute>();

        Assert.NotNull(dataTypeAttr);
        Assert.Equal(DataType.Date, dataTypeAttr.DataType);
    }

    [Fact]
    public void DataType_DateTime_ShouldBeDetectable()
    {
        var propertyInfo = typeof(DataTypeTestModel).GetProperty(nameof(DataTypeTestModel.DateTimeField))!;
        var dataTypeAttr = propertyInfo.GetCustomAttribute<DataTypeAttribute>();

        Assert.NotNull(dataTypeAttr);
        Assert.Equal(DataType.DateTime, dataTypeAttr.DataType);
    }

    [Fact]
    public void DataType_MultilineText_ShouldBeDetectable()
    {
        var propertyInfo = typeof(DataTypeTestModel).GetProperty(nameof(DataTypeTestModel.MultilineField))!;
        var dataTypeAttr = propertyInfo.GetCustomAttribute<DataTypeAttribute>();

        Assert.NotNull(dataTypeAttr);
        Assert.Equal(DataType.MultilineText, dataTypeAttr.DataType);
    }

    #endregion

    #region Helper Methods

    private static List<Func<object?, (bool, string)>> GetValidators<T>(string propertyName)
    {
        var propertyInfo = typeof(T).GetProperty(propertyName)!;
        return FormHelpers.GetValidators(propertyInfo);
    }

    private static FormHelpers.DisplayInfo GetDisplayInfo<T>(string propertyName)
    {
        var propertyInfo = typeof(T).GetProperty(propertyName)!;
        return FormHelpers.GetDisplayInfo(propertyInfo);
    }

    #endregion
}
