using System.Text;
using Isopoh.Cryptography.Argon2;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Setup;

public class SecuritySetupView : ViewBase
{
    public override object Build()
    {
        var config = UseService<IConfigService>();
        var client = UseService<IClientProvider>();

        var isEnabled = UseState(config.Settings.Auth != null);
        var currentPassword = UseState("");
        var newPassword = UseState("");
        var confirmPassword = UseState("");
        var errorMessage = UseState<string?>(null);

        var hasAuthConfigured = config.Settings.Auth != null;
        var passwordsMatch = newPassword.Value == confirmPassword.Value;
        var canSave = isEnabled.Value
            && !string.IsNullOrWhiteSpace(newPassword.Value)
            && passwordsMatch;

        var form = Layout.Vertical().Gap(4).Padding(4).Width(Size.Auto().Max(Size.Units(120)))
                   | Text.Block("Session Protection").Bold()
                   | Text.Block("Require a password to access the Tendril interface")
                   | isEnabled.ToBoolInput("Enable Password Protection")
                   | (isEnabled.Value
                       ? Layout.Vertical().Gap(3)
                         | (hasAuthConfigured
                             ? currentPassword.ToPasswordInput("Current password...")
                                 .WithField().Label("Current Password")
                             : null)
                         | newPassword.ToPasswordInput("New password...")
                             .WithField().Label("New Password")
                         | confirmPassword.ToPasswordInput("Confirm password...")
                             .WithField().Label("Confirm Password")
                         | (!passwordsMatch && !string.IsNullOrWhiteSpace(confirmPassword.Value)
                             ? Text.Block("Passwords do not match").Color(Colors.Destructive)
                             : null)
                       : null)
                   | (errorMessage.Value != null
                       ? Text.Block(errorMessage.Value!).Color(Colors.Destructive)
                       : null)
                   | new Button("Save").Primary()
                       .Disabled(!canSave && isEnabled.Value || (!isEnabled.Value && !hasAuthConfigured))
                       .OnClick(() =>
                       {
                           errorMessage.Set(null);

                           if (!isEnabled.Value)
                           {
                               if (hasAuthConfigured && !VerifyCurrentPassword(config, currentPassword.Value))
                               {
                                   errorMessage.Set("Current password is incorrect");
                                   return;
                               }

                               config.Settings.Auth = null;
                               config.SaveSettings();
                               client.Toast("Password protection disabled", "Saved");
                               currentPassword.Set("");
                               newPassword.Set("");
                               confirmPassword.Set("");
                               return;
                           }

                           if (hasAuthConfigured && !VerifyCurrentPassword(config, currentPassword.Value))
                           {
                               errorMessage.Set("Current password is incorrect");
                               return;
                           }

                           var secret = config.Settings.Auth?.HashSecret
                               ?? GenerateSecret();
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
                               Password = Encoding.UTF8.GetBytes(newPassword.Value),
                               Salt = salt,
                               Secret = secretBytes,
                               HashLength = 32
                           });

                           config.Settings.Auth = new AuthConfig
                           {
                               Password = hash,
                               HashSecret = secret
                           };

                           config.SaveSettings();
                           client.Toast("Password protection enabled", "Saved");

                           currentPassword.Set("");
                           newPassword.Set("");
                           confirmPassword.Set("");
                       });

        return form;
    }

    internal static bool VerifyCurrentPassword(IConfigService config, string password)
    {
        if (config.Settings.Auth == null)
            return true;

        if (string.IsNullOrWhiteSpace(password))
            return false;

        try
        {
            var secretBytes = Convert.FromBase64String(config.Settings.Auth.HashSecret);
            return Argon2.Verify(config.Settings.Auth.Password, new Argon2Config
            {
                Password = Encoding.UTF8.GetBytes(password),
                Secret = secretBytes,
            });
        }
        catch
        {
            return false;
        }
    }

    internal static string GenerateSecret()
    {
        var bytes = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
