using Ivy.Core.Exceptions;
using Ivy.Core.Hooks;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Test;

public class UseMemoTests
{
    private static ViewContext CreateViewContext()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IExceptionHandler>(new StubExceptionHandler());
        var provider = services.BuildServiceProvider();
        return new ViewContext(() => { }, null, provider);
    }

    [Fact]
    public void UseMemo_RecomputesWhenIStateDependencyValueChanges()
    {
        var ctx = CreateViewContext();

        // First render: create state and memo
        var state = ctx.UseState(1);
        var computeCount = 0;
        var result = ctx.UseMemo(() =>
        {
            computeCount++;
            return state.Value * 10;
        }, state); // Passing IState directly, not .Value

        Assert.Equal(10, result);
        Assert.Equal(1, computeCount);

        // Change state value
        state.Set(2);

        // Second render
        ctx.Reset();
        _ = ctx.UseState(1); // re-call to advance hook index
        result = ctx.UseMemo(() =>
        {
            computeCount++;
            return state.Value * 10;
        }, state); // Still passing IState directly

        Assert.Equal(20, result);
        Assert.Equal(2, computeCount);
    }

    [Fact]
    public void UseMemo_DoesNotRecomputeWhenIStateValueUnchanged()
    {
        var ctx = CreateViewContext();

        var state = ctx.UseState(5);
        var computeCount = 0;
        var result = ctx.UseMemo(() =>
        {
            computeCount++;
            return state.Value * 10;
        }, state);

        Assert.Equal(50, result);
        Assert.Equal(1, computeCount);

        // Do not change state, just re-render
        ctx.Reset();
        _ = ctx.UseState(5);
        result = ctx.UseMemo(() =>
        {
            computeCount++;
            return state.Value * 10;
        }, state);

        Assert.Equal(50, result);
        Assert.Equal(1, computeCount); // Should NOT have recomputed
    }

    [Fact]
    public void UseMemo_DotValueAndIStateBothWork()
    {
        // Verify that passing .Value and passing the IState object both trigger recomputation correctly
        var ctx1 = CreateViewContext();
        var ctx2 = CreateViewContext();

        var state1 = ctx1.UseState(1);
        var state2 = ctx2.UseState(1);

        // Using .Value
        var result1 = ctx1.UseMemo(() => state1.Value * 10, state1.Value);
        // Using IState directly
        var result2 = ctx2.UseMemo(() => state2.Value * 10, state2);

        Assert.Equal(10, result1);
        Assert.Equal(10, result2);

        state1.Set(2);
        state2.Set(2);

        ctx1.Reset();
        _ = ctx1.UseState(1);
        result1 = ctx1.UseMemo(() => state1.Value * 10, state1.Value);

        ctx2.Reset();
        _ = ctx2.UseState(1);
        result2 = ctx2.UseMemo(() => state2.Value * 10, state2);

        Assert.Equal(20, result1);
        Assert.Equal(20, result2);
    }

    [Fact]
    public void UseCallback_ReturnsStableReferenceWhenDepsUnchanged()
    {
        var ctx = CreateViewContext();

        var dep = ctx.UseState(1);
        var callback1 = ctx.UseCallback((string s) => { }, dep.Value);

        ctx.Reset();
        _ = ctx.UseState(1);
        var callback2 = ctx.UseCallback((string s) => { }, dep.Value);

        Assert.Same(callback1, callback2);
    }

    [Fact]
    public void UseCallback_ReturnsNewReferenceWhenDepsChange()
    {
        var ctx = CreateViewContext();

        var dep = ctx.UseState(1);
        var callback1 = ctx.UseCallback((string s) => { }, dep.Value);

        dep.Set(2);
        ctx.Reset();
        _ = ctx.UseState(1);
        var callback2 = ctx.UseCallback((string s) => { }, dep.Value);

        Assert.NotSame(callback1, callback2);
    }

    [Fact]
    public void UseCallback_ActionOverloadWorks()
    {
        var ctx = CreateViewContext();
        var invoked = false;

        _ = ctx.UseState(0); // advance index
        var callback = ctx.UseCallback(() => invoked = true, 42);
        callback();

        Assert.True(invoked);
    }

    [Fact]
    public void UseCallback_FuncOverloadWorks()
    {
        var ctx = CreateViewContext();

        _ = ctx.UseState(0);
        var callback = ctx.UseCallback((int x) => x * 2, 42);

        Assert.Equal(10, callback(5));
    }

    private class StubExceptionHandler : IExceptionHandler
    {
        public bool HandleException(Exception exception) => false;
    }
}
