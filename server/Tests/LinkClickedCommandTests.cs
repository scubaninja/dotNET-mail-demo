using Tailwind.Mail.Commands;
using Xunit;

namespace Tailwind.Mail.Tests;

public class LinkClickedCommandTests
{
    [Fact]
    public void Execute_WithKey_ReturnsExpectedMessage()
    {
        // Arrange
        var key = "abc123";
        var command = new LinkClickedCommand(key);

        // Act
        var result = command.Execute();

        // Assert
        Assert.Equal("Link clicked: abc123", result);
    }

    [Fact]
    public void Execute_WithEmptyKey_ReturnsExpectedMessage()
    {
        // Arrange
        var command = new LinkClickedCommand(string.Empty);

        // Act
        var result = command.Execute();

        // Assert
        Assert.Equal("Link clicked: ", result);
    }

    [Fact]
    public void Key_Property_ReturnsConstructorValue()
    {
        // Arrange
        var key = "test-key";
        var command = new LinkClickedCommand(key);

        // Act & Assert
        Assert.Equal(key, command.Key);
    }
}
