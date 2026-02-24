using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

public class BroadcastTests
{
    private const string ValidMarkdown = @"---
Subject: Test Broadcast
Summary: Test summary
---
Email body.
";

    private const string ValidMarkdownWithTag = @"---
Subject: Tagged Broadcast
Summary: Summary here
Slug: tagged-broadcast
SendToTag: customers
---
Email body.
";

    [Fact]
    public void FromMarkdownEmail_WithValidEmail_SetsName()
    {
        // Arrange
        var email = MarkdownEmail.FromString(ValidMarkdown);

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(email);

        // Assert
        Assert.Equal("Test Broadcast", broadcast.Name);
    }

    [Fact]
    public void FromMarkdownEmail_WithValidEmail_SetsSlug()
    {
        // Arrange
        var email = MarkdownEmail.FromString(ValidMarkdown);

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(email);

        // Assert
        Assert.Equal("test-broadcast", broadcast.Slug);
    }

    [Fact]
    public void FromMarkdownEmail_WithDefaultTag_SendsToAll()
    {
        // Arrange
        var email = MarkdownEmail.FromString(ValidMarkdown);

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(email);

        // Assert
        Assert.Equal("*", broadcast.SendToTag);
    }

    [Fact]
    public void FromMarkdownEmail_WithSendToTag_SetsTag()
    {
        // Arrange
        var email = MarkdownEmail.FromString(ValidMarkdownWithTag);

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(email);

        // Assert
        Assert.Equal("customers", broadcast.SendToTag);
    }

    [Fact]
    public void FromMarkdownEmail_WithNullDocument_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => Broadcast.FromMarkdownEmail(null!));
    }

    [Fact]
    public void FromMarkdown_WithValidMarkdown_ReturnsBroadcast()
    {
        // Arrange & Act
        var broadcast = Broadcast.FromMarkdown(ValidMarkdown);

        // Assert
        Assert.NotNull(broadcast);
        Assert.Equal("Test Broadcast", broadcast.Name);
    }

    [Fact]
    public void FromMarkdown_WithNullMarkdown_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => Broadcast.FromMarkdown(null!));
    }

    [Fact]
    public void FromMarkdown_WithEmptyMarkdown_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => Broadcast.FromMarkdown(string.Empty));
    }

    [Fact]
    public void Broadcast_DefaultStatus_IsPending()
    {
        // Arrange
        var email = MarkdownEmail.FromString(ValidMarkdown);

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(email);

        // Assert
        Assert.Equal("pending", broadcast.Status);
    }
}
