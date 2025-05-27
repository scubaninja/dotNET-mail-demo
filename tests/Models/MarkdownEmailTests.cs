using Xunit;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests.Models;

public class MarkdownEmailTests
{
    [Fact]
    public void FromString_WithValidMarkdown_ReturnsValidEmail()
    {
        // Arrange
        var markdown = @"---
Subject: Test Email
Summary: This is a test email
---
# Test Content
This is test content.";

        // Act
        var email = MarkdownEmail.FromString(markdown);

        // Assert
        Assert.True(email.IsValid());
        Assert.NotNull(email.Html);
        Assert.Contains("<h1>Test Content</h1>", email.Html);
        Assert.Equal("test-email", email.Data.Slug);
    }

    [Fact]
    public void IsValid_WithMissingRequiredFields_ReturnsFalse()
    {
        // Arrange
        var markdown = @"---
Title: Just a title
---
# Test Content";

        // Act
        var email = MarkdownEmail.FromString(markdown);

        // Assert
        Assert.False(email.IsValid());
    }

    [Fact]
    public void FromString_WithDefaultSendToTag_SetsWildcard()
    {
        // Arrange
        var markdown = @"---
Subject: Test Email
Summary: This is a test email
---
Content";

        // Act
        var email = MarkdownEmail.FromString(markdown);

        // Assert
        Assert.Equal("*", email.Data.SendToTag);
    }
}
