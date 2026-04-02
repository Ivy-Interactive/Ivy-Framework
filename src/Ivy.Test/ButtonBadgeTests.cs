using Ivy;
using Xunit;

namespace Ivy.Test;

public class ButtonBadgeTests
{
    [Fact]
    public void Badge_SetsProperty()
    {
        var button = new Button("Inbox").Badge("3");

        Assert.Equal("3", button.Badge);
    }

    [Fact]
    public void Badge_AndShortcutKey_CoexistWithoutConflict()
    {
        var button = new Button("Save").Badge("New").ShortcutKey("Ctrl+S");

        Assert.Equal("New", button.Badge);
        Assert.Equal("Ctrl+S", button.ShortcutKey);
    }

    [Fact]
    public void Badge_Null_ClearsProperty()
    {
        var button = new Button("Test").Badge("5").Badge(null);

        Assert.Null(button.Badge);
    }
}
