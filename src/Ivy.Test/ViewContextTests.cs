using Ivy.Core.Exceptions;
using Ivy.Core.Hooks;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Test;

public class ViewContextTests
{
    private static ViewContext CreateViewContext(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IExceptionHandler>(new StubExceptionHandler());
        configure?.Invoke(services);
        var provider = services.BuildServiceProvider();
        return new ViewContext(() => { }, null, provider);
    }

    [Fact]
    public void UseService_ThrowsWhenServiceNotRegistered()
    {
        var ctx = CreateViewContext();
        var ex = Assert.Throws<InvalidOperationException>(() => ctx.UseService<IDisposable>());
        Assert.Contains(nameof(IDisposable), ex.Message);
    }

    [Fact]
    public void TryUseService_ReturnsFalseWhenServiceNotRegistered()
    {
        var ctx = CreateViewContext();
        var result = ctx.TryUseService<IDisposable>(out var service);
        Assert.False(result);
        Assert.Null(service);
    }

    [Fact]
    public void TryUseService_ReturnsTrueWhenServiceRegistered()
    {
        var expected = new StubExceptionHandler();
        var ctx = CreateViewContext(s => s.AddSingleton<ITestService>(new TestServiceImpl()));

        var result = ctx.TryUseService<ITestService>(out var service);

        Assert.True(result);
        Assert.NotNull(service);
        Assert.IsType<TestServiceImpl>(service);
    }

    private interface ITestService;

    private class TestServiceImpl : ITestService;

    private class StubExceptionHandler : IExceptionHandler
    {
        public bool HandleException(Exception exception) => false;
    }
}
