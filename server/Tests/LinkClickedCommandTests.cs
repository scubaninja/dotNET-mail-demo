using System;
using Xunit;
using Tailwind.Mail.Commands;

namespace Tailwind.Mail.Tests;

public class LinkClickedCommandTests
{
    [Fact]
    public void Execute_ReturnsLinkClickedString()
    {
        var key = "abc123";
        var cmd = new LinkClickedCommand(key);

        var result = cmd.Execute();

        Assert.Equal("Link clicked: abc123", result);
    }

    [Fact]
    public void Execute_WithDifferentKey_ContainsKey()
    {
        var key = "unique-key-456";
        var cmd = new LinkClickedCommand(key);

        var result = cmd.Execute();

        Assert.Contains(key, result);
    }

    [Fact]
    public void Constructor_SetsKey()
    {
        var key = "test-key";

        var cmd = new LinkClickedCommand(key);

        Assert.Equal(key, cmd.Key);
    }

    [Fact]
    public void Execute_WithEmptyKey_ReturnsExpectedString()
    {
        var cmd = new LinkClickedCommand(string.Empty);

        var result = cmd.Execute();

        Assert.Equal("Link clicked: ", result);
    }
}
