using Tailwind.Mail.Commands;
using Xunit;

namespace Tailwind.Tests;

/// <summary>Tests for the <see cref="LinkClickedCommand"/> which tracks link-click events.</summary>
public class LinkClickedCommandTests
{
    [Fact]
    public void Constructor_StoresKey()
    {
        var cmd = new LinkClickedCommand("abc-123");
        Assert.Equal("abc-123", cmd.Key);
    }

    [Fact]
    public void Execute_ReturnsFormattedMessage()
    {
        var cmd = new LinkClickedCommand("abc-123");
        Assert.Equal("Link clicked: abc-123", cmd.Execute());
    }

    [Theory]
    [InlineData("key1", "Link clicked: key1")]
    [InlineData("abc-xyz", "Link clicked: abc-xyz")]
    [InlineData("guid-style-key-here", "Link clicked: guid-style-key-here")]
    [InlineData("", "Link clicked: ")]
    public void Execute_VariousKeys_ReturnsExpectedMessage(string key, string expected)
    {
        var cmd = new LinkClickedCommand(key);
        Assert.Equal(expected, cmd.Execute());
    }

    [Fact]
    public void Execute_IsIdempotent_ReturnsSameResultOnRepeatedCalls()
    {
        var cmd = new LinkClickedCommand("my-key");
        var first = cmd.Execute();
        var second = cmd.Execute();
        Assert.Equal(first, second);
    }
}
