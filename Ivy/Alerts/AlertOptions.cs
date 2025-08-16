namespace Ivy.Alerts;

public class AlertOptions
{
    public AlertOptions(string? title, string? message, AlertButtonSet buttonSet = AlertButtonSet.Ok)
    {
        Title = title;
        Message = message;
        Buttons = AlertOptionHelpers.GetButtons(buttonSet);
    }
    public string? Title { get; init; }
    public string? Message { get; init; }
    public AlertButton[] Buttons { get; set; }
}

public static class AlertOptionHelpers
{
    public static AlertButton[] GetButtons(AlertButtonSet buttonSet)
    {
        return buttonSet switch
        {
            AlertButtonSet.Ok =>
            [
                new AlertButton("Ok", AlertResult.Ok)
            ],
            AlertButtonSet.OkCancel =>
            [
                new AlertButton("Ok", AlertResult.Ok),
                new AlertButton("Cancel", AlertResult.Cancel, ButtonVariant.Secondary)
            ],
            AlertButtonSet.YesNo =>
            [
                new AlertButton("Yes", AlertResult.Yes),
                new AlertButton("No", AlertResult.No, ButtonVariant.Secondary)
            ],
            AlertButtonSet.YesNoCancel =>
            [
                new AlertButton("Yes", AlertResult.Yes),
                new AlertButton("No", AlertResult.No),
                new AlertButton("Cancel", AlertResult.Cancel, ButtonVariant.Secondary)
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(buttonSet), buttonSet, null)
        };
    }
}

public class AlertButton(string label, AlertResult result, ButtonVariant variant = ButtonVariant.Primary)
{
    public string Label { get; init; } = label;
    public AlertResult Result { get; init; } = result;
    public ButtonVariant Variant { get; init; } = variant;
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

public enum AlertButtonSet
{
    Ok,
    OkCancel,
    YesNo,
    YesNoCancel
}
