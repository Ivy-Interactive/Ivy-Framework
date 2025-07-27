using Ivy.Core;
using Ivy.Core.Hooks;
using Ivy.Helpers;
using Ivy.Views;

namespace Ivy.Alerts;

public class AlertView(IState<AlertResult> alertResult, IState<bool> isOpen, AlertOptions options) : ViewBase
{
    public override object? Build()
    {
        Button CreateButton(AlertButton button)
        {
            return new Button(button.Label, _ =>
            {
                alertResult.Set(button.Result);
                isOpen.Set(false);
            }, variant: button.Variant);
        }

        void OnCancel(Event<Dialog> _)
        {
            alertResult.Set(AlertResult.Cancel);
            isOpen.Set(false);
        }

        return new Dialog(
            OnCancel,
            new DialogHeader(options.Title ?? ""),
            new DialogBody(options.Message ?? ""),
            new DialogFooter(
                Layout.Horizontal(options.Buttons.Select(CreateButton))
            )
        );
    }
}