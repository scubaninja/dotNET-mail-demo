using Xunit;
using Tailwind.Mail.Commands;

namespace Tailwind.Mail.Tests;

public class BulkTagCommandTests
{
    [Fact]
    public void BulkTagCommand_DefaultConstructor_InitializesWithEmptyList()
    {
        // Arrange & Act
        var command = new BulkTagCommand();

        // Assert
        Assert.NotNull(command.Emails);
        Assert.Empty(command.Emails);
        Assert.Null(command.Tag);
    }

    [Fact]
    public void BulkTagCommand_CanSetTag()
    {
        // Arrange
        var command = new BulkTagCommand();

        // Act
        command.Tag = "newsletter";

        // Assert
        Assert.Equal("newsletter", command.Tag);
    }

    [Fact]
    public void BulkTagCommand_CanSetEmails()
    {
        // Arrange
        var command = new BulkTagCommand();
        var emails = new List<string> 
        { 
            "user1@example.com", 
            "user2@example.com",
            "user3@example.com"
        };

        // Act
        command.Emails = emails;

        // Assert
        Assert.Equal(3, command.Emails.Count());
        Assert.Contains("user1@example.com", command.Emails);
        Assert.Contains("user2@example.com", command.Emails);
        Assert.Contains("user3@example.com", command.Emails);
    }

    [Fact]
    public void BulkTagCommand_EmptyEmailsList_IsValid()
    {
        // Arrange & Act
        var command = new BulkTagCommand
        {
            Tag = "customers",
            Emails = new List<string>()
        };

        // Assert
        Assert.NotNull(command.Emails);
        Assert.Empty(command.Emails);
    }

    [Fact]
    public void BulkTagCommand_CanHandleManyEmails()
    {
        // Arrange
        var command = new BulkTagCommand();
        var emails = Enumerable.Range(1, 100)
            .Select(i => $"user{i}@example.com")
            .ToList();

        // Act
        command.Emails = emails;

        // Assert
        Assert.Equal(100, command.Emails.Count());
    }
}
