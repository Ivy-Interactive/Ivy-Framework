using System.ComponentModel.DataAnnotations;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class AlertExtensions
{
    public static IView WithConfirm(this Button button, string message, string? title = null, string? confirmLabel = null, bool destructive = false)
    {
        return new WithConfirmView(button, message, title, confirmLabel, destructive);
    }

    public static IView WithPrompt<T>(this Button button, Action<T> handleResult, T? defaultValue = default(T), string? title = null, string? message = null)
    {
        return new WithPromptView<T>(button, handleResult, defaultValue, message, title);
    }
}

public record PromptValueTypeWrapper<T>([Required] T? Value);

public class WithPromptView<T>(Button button, Action<T> handleResult, T? defaultValue = default(T), string? message = null, string? title = null) : ViewBase
{
    public override object? Build()
    {
        if (
            typeof(T) != typeof(FileUpload)
            && !TypeHelper.IsSimpleType(typeof(T)))
        {
            throw new NotSupportedException();
        }

        var record = UseState(() => new PromptValueTypeWrapper<T>(defaultValue));

        UseEffect(() =>
        {
            if (record.Value.Value != null)
            {
                handleResult(record.Value.Value);
            }
        }, record);

        return button.ToTrigger((isOpen) => record.ToForm()
            .Label(e => e.Value!, title ?? "Value")
            .ToDialog(isOpen, title: "", description: message));
    }
}

public class WithConfirmView(Button button, string message, string? title = null, string? confirmLabel = null, bool destructive = false) : ViewBase
{
    public override object? Build()
    {
        var isOpen = UseState(false);

        var clonedButton = button with
        {
            OnClick = new(_ =>
            {
                isOpen.Value = true;
                return ValueTask.CompletedTask;
            })
        };

        return new Fragment(
            clonedButton,
            isOpen.Value ? new Dialog(
                _ => isOpen.Set(false),
                new DialogHeader(title ?? ""),
                new DialogBody(message),
                new DialogFooter(
                    new Button("Cancel", _ => isOpen.Value = false, variant: ButtonVariant.Outline),
                    new Button(confirmLabel ?? "Ok", async @event =>
                    {
                        isOpen.Set(false);
                        if (button.OnClick != null) await button.OnClick.Invoke(@event);
                    }, variant: destructive ? ButtonVariant.Destructive : ButtonVariant.Primary)
                )
            ) : null
        );
    }
}