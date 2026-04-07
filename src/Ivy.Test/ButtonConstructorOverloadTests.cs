namespace Ivy.Test;

/// <summary>
/// Tests that Button constructor overloads correctly resolve method groups
/// for all supported delegate types, including the Func&lt;ValueTask&gt; overload.
/// </summary>
public class ButtonConstructorOverloadTests
{
    #region Constructor Tests

    [Fact]
    public void Button_Constructor_ParameterlessAction_ResolvesCorrectly()
    {
        // Arrange
        void HandleClick() { }

        // Act
        var button = new Button("Test", HandleClick);

        // Assert
        Assert.NotNull(button);
        Assert.Equal("Test", button.Title);
        Assert.NotNull(button.OnClick);
    }

    [Fact]
    public void Button_Constructor_ParameterlessAsyncFunc_ResolvesCorrectly()
    {
        // Arrange
        async ValueTask HandleClickAsync() { await ValueTask.CompletedTask; }

        // Act
        var button = new Button("Test", HandleClickAsync);

        // Assert
        Assert.NotNull(button);
        Assert.Equal("Test", button.Title);
        Assert.NotNull(button.OnClick);
    }

    [Fact]
    public void Button_Constructor_EventParameterAsyncFunc_ResolvesCorrectly()
    {
        // Arrange
        async ValueTask HandleClickEvent(Event<Button> e) { await ValueTask.CompletedTask; }

        // Act
        var button = new Button("Test", HandleClickEvent);

        // Assert
        Assert.NotNull(button);
        Assert.Equal("Test", button.Title);
        Assert.NotNull(button.OnClick);
    }

    [Fact]
    public void Button_Constructor_EventParameterAction_ResolvesCorrectly()
    {
        // Arrange
        void HandleClickSync(Event<Button> e) { }

        // Act
        var button = new Button("Test", HandleClickSync);

        // Assert
        Assert.NotNull(button);
        Assert.Equal("Test", button.Title);
        Assert.NotNull(button.OnClick);
    }

    [Fact]
    public void Button_Constructor_FuncValueTask_NullOnClick_SetsOnClickToNull()
    {
        // Act
        var button = new Button("Test", (Func<ValueTask>?)null);

        // Assert
        Assert.NotNull(button);
        Assert.Equal("Test", button.Title);
        Assert.Null(button.OnClick);
    }

    [Fact]
    public async Task Button_Constructor_FuncValueTask_InvokesHandler()
    {
        // Arrange
        var invoked = false;
        async ValueTask HandleClick() { invoked = true; await ValueTask.CompletedTask; }

        // Act
        var button = new Button("Test", HandleClick);
        await button.OnClick!.Invoke(new Event<Button>("OnClick", button));

        // Assert
        Assert.True(invoked);
    }

    #endregion

    #region ToButton Extension Tests

    [Fact]
    public void ToButton_FuncValueTask_ResolvesCorrectly()
    {
        // Arrange
        async ValueTask HandleClickAsync() { await ValueTask.CompletedTask; }

        // Act
        var button = Icons.Plus.ToButton(HandleClickAsync);

        // Assert
        Assert.NotNull(button);
        Assert.NotNull(button.OnClick);
        Assert.Equal(Icons.Plus, button.Icon);
    }

    [Fact]
    public void ToButton_FuncValueTask_WithVariant_SetsVariant()
    {
        // Arrange
        async ValueTask HandleClickAsync() { await ValueTask.CompletedTask; }

        // Act
        var button = Icons.Plus.ToButton(HandleClickAsync, ButtonVariant.Destructive);

        // Assert
        Assert.Equal(ButtonVariant.Destructive, button.Variant);
    }

    #endregion
}
