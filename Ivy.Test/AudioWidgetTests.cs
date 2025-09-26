using Ivy.Shared;

namespace Ivy.Test;

public class AudioPlayerWidgetTests
{
    [Fact]
    public void Audio_Constructor_SetsDefaultProperties()
    {
        // Arrange & Act
        var audio = new Audio("test.mp3");

        // Assert
        Assert.Equal("test.mp3", audio.Src);
        Assert.Equal(Size.Full(), audio.Width);
        Assert.Equal(Size.Units(10), audio.Height);
        Assert.False(audio.Autoplay);
        Assert.False(audio.Loop);
        Assert.False(audio.Muted);
        Assert.Equal(AudioPreload.Metadata, audio.Preload);
        Assert.True(audio.Controls);
    }

    [Fact]
    public void Audio_FluentAPI_ConfiguresProperties()
    {
        // Arrange & Act
        var audio = new Audio("test.mp3")
            .Autoplay(true)
            .Loop(true)
            .Muted(true)
            .Preload(AudioPreload.Auto)
            .Controls(false);

        // Assert
        Assert.Equal("test.mp3", audio.Src);
        Assert.True(audio.Autoplay);
        Assert.True(audio.Loop);
        Assert.True(audio.Muted);
        Assert.Equal(AudioPreload.Auto, audio.Preload);
        Assert.False(audio.Controls);
    }

    [Fact]
    public void Audio_Src_Extension_UpdatesSource()
    {
        // Arrange
        var audio = new Audio("original.mp3");

        // Act
        var updatedAudio = audio.Src("updated.mp3");

        // Assert
        Assert.Equal("updated.mp3", updatedAudio.Src);
        Assert.Equal("original.mp3", audio.Src); // Original should be unchanged
    }

    [Fact]
    public void Audio_Preload_Extension_UpdatesPreloadStrategy()
    {
        // Arrange
        var audio = new Audio("test.mp3");

        // Act
        var updatedAudio = audio.Preload(AudioPreload.None);

        // Assert
        Assert.Equal(AudioPreload.None, updatedAudio.Preload);
        Assert.Equal(AudioPreload.Metadata, audio.Preload); // Original should be unchanged
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Audio_Autoplay_Extension_UpdatesAutoplayState(bool autoplayValue)
    {
        // Arrange
        var audio = new Audio("test.mp3");

        // Act
        var updatedAudio = audio.Autoplay(autoplayValue);

        // Assert
        Assert.Equal(autoplayValue, updatedAudio.Autoplay);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Audio_Loop_Extension_UpdatesLoopState(bool loopValue)
    {
        // Arrange
        var audio = new Audio("test.mp3");

        // Act
        var updatedAudio = audio.Loop(loopValue);

        // Assert
        Assert.Equal(loopValue, updatedAudio.Loop);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Audio_Muted_Extension_UpdatesMutedState(bool mutedValue)
    {
        // Arrange
        var audio = new Audio("test.mp3");

        // Act
        var updatedAudio = audio.Muted(mutedValue);

        // Assert
        Assert.Equal(mutedValue, updatedAudio.Muted);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Audio_Controls_Extension_UpdatesControlsState(bool controlsValue)
    {
        // Arrange
        var audio = new Audio("test.mp3");

        // Act
        var updatedAudio = audio.Controls(controlsValue);

        // Assert
        Assert.Equal(controlsValue, updatedAudio.Controls);
    }

}
