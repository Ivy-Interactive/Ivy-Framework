// ReSharper disable once CheckNamespace
namespace Ivy;

public class AlertOptions
{
    public AlertOptions(string? title, string? message, AlertBatonSet buttonSet = AlertBatonSet.Ok)
    {
        Title = title;
        Message = message;
        Batons = AlertOptionHelpers.GetBatons(buttonSet);
    }
    public string? Title { get; init; }
    public string? Message { get; init; }
    public AlertBaton[] Batons { get; set; }
}

public static class AlertOptionHelpers
{
    public static AlertBaton[] GetBatons(AlertBatonSet buttonSet)
    {
        return buttonSet switch
        {
            AlertBatonSet.Ok =>
            [
                new AlertBaton("Ok", AlertResult.Ok)
            ],
            AlertBatonSet.OkCancel =>
            [
                new AlertBaton("Cancel", AlertResult.Cancel, BatonVariant.Secondary),
                new AlertBaton("Ok", AlertResult.Ok)
            ],
            AlertBatonSet.YesNo =>
            [
                new AlertBaton("No", AlertResult.No, BatonVariant.Secondary),
                new AlertBaton("Yes", AlertResult.Yes)
            ],
            AlertBatonSet.YesNoCancel =>
            [
                new AlertBaton("Cancel", AlertResult.Cancel, BatonVariant.Secondary),
                new AlertBaton("No", AlertResult.No),
                new AlertBaton("Yes", AlertResult.Yes)
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(buttonSet), buttonSet, null)
        };
    }
}

public class AlertBaton(string label, AlertResult result, BatonVariant variant = BatonVariant.Primary)
{
    public string Label { get; init; } = label;
    public AlertResult Result { get; init; } = result;
    public BatonVariant Variant { get; init; } = variant;
}

public enum AlertResult
{
    Undecided,
    Ok,
    Cancel,
    Yes,
    No
}

public static class AlertResultExtensions
{
    public static bool IsOk(this AlertResult result) => result == AlertResult.Ok;
}

public enum AlertBatonSet
{
    Ok,
    OkCancel,
    YesNo,
    YesNoCancel
}
