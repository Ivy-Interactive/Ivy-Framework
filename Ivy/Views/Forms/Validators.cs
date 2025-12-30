using System.Net.Mail;

namespace Ivy.Views.Forms;

public static class Validators
{
    public static Func<object?, (bool, string)> CreateEmailValidator(string fieldName)
    {
        return email =>
        {
            if (email is not string emailStr || string.IsNullOrWhiteSpace(emailStr))
                return (true, ""); // Empty is handled by Required validator

            try
            {
                var addr = new MailAddress(emailStr);

                if (!addr.Host.Contains('.'))
                    return (false, "Please enter a valid email address");

                return (true, "");
            }
            catch (FormatException)
            {
                return (false, "Please enter a valid email address");
            }
        };
    }
}

