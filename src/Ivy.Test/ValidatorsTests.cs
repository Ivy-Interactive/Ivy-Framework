using Ivy;
using Ivy.Views.Forms;

namespace Ivy.Test;

public class ValidatorsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateEmailValidator_EmptyOrNull_ReturnsValid(object? value)
    {
        var validator = Validators.CreateEmailValidator("Email");
        var (valid, _) = validator(value);
        Assert.True(valid);
    }

    [Theory]
    [InlineData("a@b.co")]
    [InlineData("user@example.com")]
    [InlineData("user.name+tag@example.co.uk")]
    public void CreateEmailValidator_ValidEmail_ReturnsValid(string email)
    {
        var validator = Validators.CreateEmailValidator("Email");
        var (valid, _) = validator(email);
        Assert.True(valid);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user@nodot")]
    public void CreateEmailValidator_InvalidEmail_ReturnsInvalid(string email)
    {
        var validator = Validators.CreateEmailValidator("Email");
        var (valid, message) = validator(email);
        Assert.False(valid);
        Assert.False(string.IsNullOrEmpty(message));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateTelValidator_EmptyOrNull_ReturnsValid(object? value)
    {
        var validator = Validators.CreateTelValidator("Phone");
        var (valid, _) = validator(value);
        Assert.True(valid);
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("+1 234 567 8901")]
    [InlineData("(123) 456-7890")]
    [InlineData("+44 20 7946 0958")]
    public void CreateTelValidator_ValidPhone_ReturnsValid(string phone)
    {
        var validator = Validators.CreateTelValidator("Phone");
        var (valid, _) = validator(phone);
        Assert.True(valid);
    }

    [Theory]
    [InlineData("123")]      // too few digits
    [InlineData("123456")]   // 6 digits
    [InlineData("12345678901234567")] // too many digits
    [InlineData("abc-def-ghij")] // letters
    public void CreateTelValidator_InvalidPhone_ReturnsInvalid(string phone)
    {
        var validator = Validators.CreateTelValidator("Phone");
        var (valid, message) = validator(phone);
        Assert.False(valid);
        Assert.False(string.IsNullOrEmpty(message));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateUrlValidator_EmptyOrNull_ReturnsValid(object? value)
    {
        var validator = Validators.CreateUrlValidator("Url");
        var (valid, _) = validator(value);
        Assert.True(valid);
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://example.com/path")]
    [InlineData("https://sub.example.co.uk/page?q=1")]
    public void CreateUrlValidator_ValidUrl_ReturnsValid(string url)
    {
        var validator = Validators.CreateUrlValidator("Url");
        var (valid, _) = validator(url);
        Assert.True(valid);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com")]
    [InlineData("example.com")] // no scheme
    public void CreateUrlValidator_InvalidUrl_ReturnsInvalid(string url)
    {
        var validator = Validators.CreateUrlValidator("Url");
        var (valid, message) = validator(url);
        Assert.False(valid);
        Assert.False(string.IsNullOrEmpty(message));
    }

    [Theory]
    [InlineData(TextInputs.Email)]
    [InlineData(TextInputs.Tel)]
    [InlineData(TextInputs.Url)]
    public void ForVariant_EmailTelUrl_ReturnsValidator(TextInputs variant)
    {
        var validator = Validators.ForVariant(variant, "Field");
        Assert.NotNull(validator);
    }

    [Theory]
    [InlineData(TextInputs.Text)]
    [InlineData(TextInputs.Textarea)]
    [InlineData(TextInputs.Password)]
    [InlineData(TextInputs.Search)]
    public void ForVariant_TextPasswordSearch_ReturnsNull(TextInputs variant)
    {
        var validator = Validators.ForVariant(variant, "Field");
        Assert.Null(validator);
    }
}
