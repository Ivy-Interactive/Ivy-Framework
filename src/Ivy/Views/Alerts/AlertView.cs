using Ivy.Core;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class AlertView(IState<AlertResult> alertResult, IState<bool> isOpen, AlertOptions options) : ViewBase
{
    public override object? Build()
    {
        Baton CreateBaton(AlertBaton button)
        {
            return new Baton(button.Label, _ =>
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
                Layout.Horizontal(options.Batons.Select(CreateBaton)).Align(Align.Right)
            )
        );
    }
}