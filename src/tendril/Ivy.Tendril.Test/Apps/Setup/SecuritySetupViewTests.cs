using System.Text;
using Isopoh.Cryptography.Argon2;
using Ivy.Tendril.Apps.Setup;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test.Apps.Setup;

public class SecuritySetupViewTests
{
    private static AuthConfig CreateAuthConfig(string password, string secret)
    {
        var secretBytes = Convert.FromBase64String(secret);
        var salt = new byte[16];
        System.Security.Cryptography.RandomNumberGenerator.Fill(salt);
        var hash = Argon2.Hash(new Argon2Config
        {
            Type = Argon2Type.DataIndependentAddressing,
            Version = Argon2Version.Nineteen,
            TimeCost = 3,
            MemoryCost = 65536,
            Lanes = 1,
            Threads = 1,
            Password = Encoding.UTF8.GetBytes(password),
            Salt = salt,
            Secret = secretBytes,
            HashLength = 32
        });

        return new AuthConfig { Password = hash, HashSecret = secret };
    }

    [Fact]
    public void GenerateSecret_ProducesValidBase64_32Bytes()
    {
        var secret = SecuritySetupView.GenerateSecret();
        var bytes = Convert.FromBase64String(secret);
        Assert.Equal(32, bytes.Length);
    }

    [Fact]
    public void GenerateSecret_ProducesUniqueValues()
    {
        var secret1 = SecuritySetupView.GenerateSecret();
        var secret2 = SecuritySetupView.GenerateSecret();
        Assert.NotEqual(secret1, secret2);
    }

    [Fact]
    public void VerifyCurrentPassword_NullAuth_ReturnsTrue()
    {
        var settings = new TendrilSettings { Auth = null };
        var config = new ConfigService(settings);

        Assert.True(SecuritySetupView.VerifyCurrentPassword(config, "anything"));
    }

    [Fact]
    public void VerifyCurrentPassword_EmptyPassword_ReturnsFalse()
    {
        var secret = SecuritySetupView.GenerateSecret();
        var auth = CreateAuthConfig("test", secret);
        var settings = new TendrilSettings { Auth = auth };
        var config = new ConfigService(settings);

        Assert.False(SecuritySetupView.VerifyCurrentPassword(config, ""));
    }

    [Fact]
    public void VerifyCurrentPassword_WhitespacePassword_ReturnsFalse()
    {
        var secret = SecuritySetupView.GenerateSecret();
        var auth = CreateAuthConfig("test", secret);
        var settings = new TendrilSettings { Auth = auth };
        var config = new ConfigService(settings);

        Assert.False(SecuritySetupView.VerifyCurrentPassword(config, "   "));
    }

    [Fact]
    public void VerifyCurrentPassword_CorrectPassword_ReturnsTrue()
    {
        var secret = SecuritySetupView.GenerateSecret();
        var auth = CreateAuthConfig("correct-pass", secret);
        var settings = new TendrilSettings { Auth = auth };
        var config = new ConfigService(settings);

        Assert.True(SecuritySetupView.VerifyCurrentPassword(config, "correct-pass"));
    }

    [Fact]
    public void VerifyCurrentPassword_WrongPassword_ReturnsFalse()
    {
        var secret = SecuritySetupView.GenerateSecret();
        var auth = CreateAuthConfig("correct-pass", secret);
        var settings = new TendrilSettings { Auth = auth };
        var config = new ConfigService(settings);

        Assert.False(SecuritySetupView.VerifyCurrentPassword(config, "wrong-pass"));
    }

    [Fact]
    public void PasswordHashing_RoundTrip()
    {
        var password = "round-trip-test";
        var secret = SecuritySetupView.GenerateSecret();
        var secretBytes = Convert.FromBase64String(secret);

        var salt = new byte[16];
        System.Security.Cryptography.RandomNumberGenerator.Fill(salt);
        var hash = Argon2.Hash(new Argon2Config
        {
            Type = Argon2Type.DataIndependentAddressing,
            Version = Argon2Version.Nineteen,
            TimeCost = 3,
            MemoryCost = 65536,
            Lanes = 1,
            Threads = 1,
            Password = Encoding.UTF8.GetBytes(password),
            Salt = salt,
            Secret = secretBytes,
            HashLength = 32
        });

        var verified = Argon2.Verify(hash, new Argon2Config
        {
            Password = Encoding.UTF8.GetBytes(password),
            Secret = secretBytes,
        });

        Assert.True(verified);
    }
}
