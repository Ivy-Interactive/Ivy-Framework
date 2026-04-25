using System.Xml;
using Ivy.Core;

namespace Ivy.IvyML;

public record IvyMLValidationResult(bool IsValid, AbstractWidget? Widget, string? ErrorMessage)
{
    public static IvyMLValidationResult Success(AbstractWidget widget) => new(true, widget, null);
    public static IvyMLValidationResult Failure(string error) => new(false, null, error);
}

public static class IvyMLValidator
{
    public static IvyMLValidationResult Validate(string ivyml)
    {
        try
        {
            var builder = new XamlBuilder();
            var widget = builder.Build(ivyml);
            return IvyMLValidationResult.Success(widget);
        }
        catch (XmlException ex)
        {
            return IvyMLValidationResult.Failure($"Malformed markup: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return IvyMLValidationResult.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return IvyMLValidationResult.Failure($"Unexpected error: {ex.Message}");
        }
    }
}
