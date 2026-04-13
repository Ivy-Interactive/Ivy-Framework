namespace Ivy.Tendril.Views;

public class NewPlanButton : ViewBase
{
    public override object Build()
    {
        return new CreatePlanDialogLauncher(
            open => new Button("New Plan")
                .Icon(Icons.Plus)
                .Width(Size.Full())
                .Variant(ButtonVariant.Primary)
                .OnClick(open)
                .ShortcutKey("CTRL+ALT+N"));
    }
}
