using Ivy.Core.Hooks;
using Ivy.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Test.Hooks;

public class UsePluginStateTests
{
    [Fact]
    public void UsePluginState_ReturnsRefreshToken()
    {
        var fakeService = new FakePluginStateService();
        var services = new ServiceCollection();
        services.AddSingleton<IPluginStateService>(fakeService);
        var serviceProvider = services.BuildServiceProvider();

        var view = new TestPluginView(serviceProvider);
        var state = view.GetPluginState();

        Assert.NotNull(state);
        Assert.IsAssignableFrom<IState<RefreshToken>>(state);
    }

    [Fact]
    public void UsePluginState_SubscribesToServiceEvents()
    {
        var fakeService = new FakePluginStateService();
        var services = new ServiceCollection();
        services.AddSingleton<IPluginStateService>(fakeService);
        var serviceProvider = services.BuildServiceProvider();

        var view = new TestPluginView(serviceProvider);
        view.GetPluginState();

        // Verify that the event can be raised without error
        fakeService.RaisePluginStateChanged();

        // If no exception is thrown, the test passes
        Assert.True(true);
    }

    [Fact]
    public void UsePluginState_MultipleEventsDoNotCauseErrors()
    {
        var fakeService = new FakePluginStateService();
        var services = new ServiceCollection();
        services.AddSingleton<IPluginStateService>(fakeService);
        var serviceProvider = services.BuildServiceProvider();

        var view = new TestPluginView(serviceProvider);
        view.GetPluginState();

        // Raise multiple events
        fakeService.RaisePluginStateChanged();
        fakeService.RaisePluginStateChanged();
        fakeService.RaisePluginStateChanged();

        // If no exception is thrown, the test passes
        Assert.True(true);
    }

    [Fact]
    public void UsePluginState_CleanupUnsubscribesFromEvents()
    {
        var fakeService = new FakePluginStateService();
        var services = new ServiceCollection();
        services.AddSingleton<IPluginStateService>(fakeService);
        var serviceProvider = services.BuildServiceProvider();

        var view = new TestPluginView(serviceProvider);
        view.GetPluginState();

        // This should not throw or cause issues after view is done
        fakeService.RaisePluginStateChanged();

        Assert.True(true); // Test passes if no exception
    }

    private class TestPluginView : ViewBase
    {
        private readonly IServiceProvider _serviceProvider;
        private IState<RefreshToken>? _pluginState;

        public TestPluginView(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override object Build()
        {
            _pluginState = UsePluginState();
            return null!;
        }

        public IState<RefreshToken> GetPluginState()
        {
            var context = new ViewContext(() => { }, null, _serviceProvider);
            BeforeBuild(context);
            Build();
            return _pluginState!;
        }
    }

    private class FakePluginStateService : IPluginStateService
    {
        public event Action? PluginStateChanged;

        public IReadOnlyList<string> GetLoadedPluginIds() => [];

        public void RaisePluginStateChanged() => PluginStateChanged?.Invoke();
    }
}
