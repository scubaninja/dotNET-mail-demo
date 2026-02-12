using Xunit;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

public class BroadcastTests
{
    private const string ValidMarkdown = @"---
Subject: Test Broadcast
Summary: Test broadcast summary
Slug: test-broadcast
SendToTag: customers
---

# Test Broadcast Email

This is a test broadcast.
";

    [Fact]
    public void Broadcast_DefaultStatus_IsPending()
    {
        // Arrange
        var broadcast = Broadcast.FromMarkdown(ValidMarkdown);

        // Act & Assert
        Assert.Equal("pending", broadcast.Status);
    }

    [Fact]
    public void Broadcast_DefaultSendToTag_IsAsterisk()
    {
        // Arrange
        var markdown = @"---
Subject: Test Broadcast
Summary: Test broadcast summary
---

# Test
";

        // Act
        var broadcast = Broadcast.FromMarkdown(markdown);

        // Assert
        Assert.Equal("*", broadcast.SendToTag);
    }

    [Fact]
    public void Broadcast_FromMarkdown_ParsesSuccessfully()
    {
        // Act
        var broadcast = Broadcast.FromMarkdown(ValidMarkdown);

        // Assert
        Assert.Equal("Test Broadcast", broadcast.Name);
        Assert.Equal("test-broadcast", broadcast.Slug);
        Assert.Equal("customers", broadcast.SendToTag);
    }

    [Fact]
    public void Broadcast_FromMarkdownEmail_ParsesSuccessfully()
    {
        // Arrange
        var markdownEmail = MarkdownEmail.FromString(ValidMarkdown);

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

        // Assert
        Assert.Equal("Test Broadcast", broadcast.Name);
        Assert.Equal("test-broadcast", broadcast.Slug);
        Assert.Equal("customers", broadcast.SendToTag);
    }

    [Fact]
    public void Broadcast_FromMarkdown_NullMarkdown_ThrowsException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Broadcast.FromMarkdown(null));
        Assert.Contains("Markdown content cannot be null or empty", exception.Message);
    }

    [Fact]
    public void Broadcast_FromMarkdown_EmptyMarkdown_ThrowsException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Broadcast.FromMarkdown(""));
        Assert.Contains("Markdown content cannot be null or empty", exception.Message);
    }

    [Fact]
    public void Broadcast_FromMarkdownEmail_NullDocument_ThrowsException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => Broadcast.FromMarkdownEmail(null));
        Assert.Contains("MarkdownEmail document cannot be null", exception.Message);
    }

    [Fact]
    public void Broadcast_CreatedAt_IsSet()
    {
        // Act
        var broadcast = Broadcast.FromMarkdown(ValidMarkdown);

        // Assert
        Assert.NotEqual(default(DateTimeOffset), broadcast.CreatedAt);
    }

    [Fact]
    public void Broadcast_ContactCount_NullConnection_ThrowsException()
    {
        // Arrange
        var broadcast = Broadcast.FromMarkdown(ValidMarkdown);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => broadcast.ContactCount(null));
        Assert.Contains("Database connection cannot be null", exception.Message);
    }
}
