using Ivy;
using Ivy.Core;
using Xunit;

namespace Ivy.Test;

/// <summary>
/// Tests that Baton constructor overloads correctly resolve method groups
/// for all supported delegate types, including the Func&lt;ValueTask&gt; overload.
/// </summary>
public class BatonConstructorOverloadTests
{
    #region Constructor Tests

    [Fact]
    public void Baton_Constructor_ParameterlessAction_ResolvesCorrectly()
    {
        // Arrange
        void HandleClick() { }

        // Act
        var button = new Baton("Test", HandleClick);

        // Assert
        Assert.NotNull(button);
        Assert.Equal("Test", button.Title);
        Assert.NotNull(button.OnClick);
    }

    [Fact]
    public void Baton_Constructor_ParameterlessAsyncFunc_ResolvesCorrectly()
    {
        // Arrange
        async ValueTask HandleClickAsync() { await ValueTask.CompletedTask; }

        // Act
        var button = new Baton("Test", HandleClickAsync);

        // Assert
        Assert.NotNull(button);
        Assert.Equal("Test", button.Title);
        Assert.NotNull(button.OnClick);
    }

    [Fact]
    public void Baton_Constructor_EventParameterAsyncFunc_ResolvesCorrectly()
    {
        // Arrange
        async ValueTask HandleClickEvent(Event<Baton> e) { await ValueTask.CompletedTask; }

        // Act
        var button = new Baton("Test", HandleClickEvent);

        // Assert
        Assert.NotNull(button);
        Assert.Equal("Test", button.Title);
        Assert.NotNull(button.OnClick);
    }

    [Fact]
    public void Baton_Constructor_EventParameterAction_ResolvesCorrectly()
    {
        // Arrange
        void HandleClickSync(Event<Baton> e) { }

        // Act
        var button = new Baton("Test", HandleClickSync);

        // Assert
        Assert.NotNull(button);
        Assert.Equal("Test", button.Title);
        Assert.NotNull(button.OnClick);
    }

    [Fact]
    public void Baton_Constructor_FuncValueTask_NullOnClick_SetsOnClickToNull()
    {
        // Act
        var button = new Baton("Test", (Func<ValueTask>?)null);

        // Assert
        Assert.NotNull(button);
        Assert.Equal("Test", button.Title);
        Assert.Null(button.OnClick);
    }

    [Fact]
    public async Task Baton_Constructor_FuncValueTask_InvokesHandler()
    {
        // Arrange
        var invoked = false;
        async ValueTask HandleClick() { invoked = true; await ValueTask.CompletedTask; }

        // Act
        var button = new Baton("Test", HandleClick);
        await button.OnClick!.Invoke(new Event<Baton>("OnClick", button));

        // Assert
        Assert.True(invoked);
    }

    #endregion

    #region ToBaton Extension Tests

    [Fact]
    public void ToBaton_FuncValueTask_ResolvesCorrectly()
    {
        // Arrange
        async ValueTask HandleClickAsync() { await ValueTask.CompletedTask; }

        // Act
        var button = Icons.Plus.ToBaton(HandleClickAsync);

        // Assert
        Assert.NotNull(button);
        Assert.NotNull(button.OnClick);
        Assert.Equal(Icons.Plus, button.Icon);
    }

    [Fact]
    public void ToBaton_FuncValueTask_WithVariant_SetsVariant()
    {
        // Arrange
        async ValueTask HandleClickAsync() { await ValueTask.CompletedTask; }

        // Act
        var button = Icons.Plus.ToBaton(HandleClickAsync, BatonVariant.Destructive);

        // Assert
        Assert.Equal(BatonVariant.Destructive, button.Variant);
    }

    #endregion
}
