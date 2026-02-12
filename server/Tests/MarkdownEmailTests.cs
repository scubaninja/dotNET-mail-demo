using Xunit;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

public class MarkdownEmailTests
{
    private const string ValidMarkdown = @"---
Subject: Test Subject
Summary: Test Summary
---

# Test Email

This is a test email body.
";

    private const string ValidMarkdownWithSlug = @"---
Subject: Test Subject
Summary: Test Summary
Slug: custom-slug
---

# Test Email

This is a test email body.
";

    [Fact]
    public void FromString_ValidMarkdown_ParsesSuccessfully()
    {
        // Act
        var email = MarkdownEmail.FromString(ValidMarkdown);

        // Assert
        Assert.NotNull(email);
        Assert.NotNull(email.Html);
        Assert.NotNull(email.Data);
        Assert.Equal("Test Subject", email.Data.Subject);
        Assert.Equal("Test Summary", email.Data.Summary);
    }

    [Fact]
    public void FromString_GeneratesHtml()
    {
        // Act
        var email = MarkdownEmail.FromString(ValidMarkdown);

        // Assert
        Assert.Contains("<h1 id=\"test-email\">Test Email</h1>", email.Html);
        Assert.Contains("<p>This is a test email body.</p>", email.Html);
    }

    [Fact]
    public void FromString_NoSlug_GeneratesSlugFromSubject()
    {
        // Act
        var email = MarkdownEmail.FromString(ValidMarkdown);

        // Assert
        Assert.Equal("test-subject", email.Data.Slug);
    }

    [Fact]
    public void FromString_WithSlug_UsesProvidedSlug()
    {
        // Act
        var email = MarkdownEmail.FromString(ValidMarkdownWithSlug);

        // Assert
        Assert.Equal("custom-slug", email.Data.Slug);
    }

    [Fact]
    public void FromString_NoSendToTag_DefaultsToAsterisk()
    {
        // Act
        var email = MarkdownEmail.FromString(ValidMarkdown);

        // Assert
        Assert.Equal("*", email.Data.SendToTag);
    }

    [Fact]
    public void IsValid_ValidEmail_ReturnsTrue()
    {
        // Arrange
        var email = MarkdownEmail.FromString(ValidMarkdown);

        // Act
        var isValid = email.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_NullData_ReturnsFalse()
    {
        // Arrange
        var email = new MarkdownEmail();

        // Act
        var isValid = email.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void FromString_NullMarkdown_ThrowsException()
    {
        // Arrange
        var email = new MarkdownEmail();

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => MarkdownEmail.FromString(null!));
        Assert.Contains("Markdown is null", exception.Message);
    }
}
