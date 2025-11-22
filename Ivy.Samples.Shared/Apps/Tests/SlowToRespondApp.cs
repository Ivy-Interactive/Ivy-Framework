namespace Ivy.Samples.Shared.Apps.Tests;

[App]
public class SlowToRespondApp : ViewBase
{
    public override object? Build()
    {
        var loading = this.UseState(false);

        async ValueTask OnClick()
        {
            loading.Set(true);
            await Task.Delay(10000);
            loading.Set(false);
        }

        return new Button("Press Me").HandleClick(OnClick).Loading(loading);

    }
}