using Xunit;
using Tailwind.Mail.Commands;

namespace Tailwind.Mail.Tests;

public class LinkClickedCommandTests
{
    [Fact]
    public void LinkClickedCommand_Constructor_SetsKey()
    {
        // Arrange
        var key = "test-link-key";

        // Act
        var command = new LinkClickedCommand(key);

        // Assert
        Assert.Equal(key, command.Key);
    }

    [Fact]
    public void LinkClickedCommand_Execute_ReturnsCorrectMessage()
    {
        // Arrange
        var key = "my-link-123";
        var command = new LinkClickedCommand(key);

        // Act
        var result = command.Execute();

        // Assert
        Assert.Contains("Link clicked:", result);
        Assert.Contains(key, result);
    }

    [Fact]
    public void LinkClickedCommand_Execute_WithDifferentKeys_ReturnsUniqueMessages()
    {
        // Arrange
        var command1 = new LinkClickedCommand("key1");
        var command2 = new LinkClickedCommand("key2");

        // Act
        var result1 = command1.Execute();
        var result2 = command2.Execute();

        // Assert
        Assert.Contains("key1", result1);
        Assert.Contains("key2", result2);
        Assert.NotEqual(result1, result2);
    }
}
