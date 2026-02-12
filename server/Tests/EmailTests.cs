using Xunit;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

public class EmailTests
{
    private const string ValidMarkdown = @"---
Subject: Test Email Subject
Summary: Test email summary for preview
Slug: test-email-slug
---

# Test Email

This is a test email body with some **bold** text.
";

    [Fact]
    public void Email_Constructor_FromValidMarkdownEmail_SetsProperties()
    {
        // Arrange
        var markdownEmail = MarkdownEmail.FromString(ValidMarkdown);

        // Act
        var email = new Email(markdownEmail);

        // Assert
        Assert.Equal("test-email-slug", email.Slug);
        Assert.Equal("Test Email Subject", email.Subject);
        Assert.Equal("Test email summary for preview", email.Preview);
        Assert.NotNull(email.Html);
        Assert.Contains("<h1 id=\"test-email\">Test Email</h1>", email.Html);
    }

    [Fact]
    public void Email_Constructor_NullData_ThrowsInvalidDataException()
    {
        // Arrange
        var markdownEmail = new MarkdownEmail();

        // Act & Assert
        var exception = Assert.Throws<InvalidDataException>(() => new Email(markdownEmail));
        Assert.Contains("Markdown document should contain Slug, Subject, and Summary", exception.Message);
    }

    [Fact]
    public void Email_Constructor_NullHtml_ThrowsInvalidDataException()
    {
        // Arrange
        var markdownEmail = new MarkdownEmail();
        markdownEmail.Data = new { Slug = "test", Subject = "Test", Summary = "Test" };

        // Act & Assert
        var exception = Assert.Throws<InvalidDataException>(() => new Email(markdownEmail));
        Assert.Contains("There should be HTML generated", exception.Message);
    }

    [Fact]
    public void Email_DefaultDelayHours_IsZero()
    {
        // Arrange
        var markdownEmail = MarkdownEmail.FromString(ValidMarkdown);

        // Act
        var email = new Email(markdownEmail);

        // Assert
        Assert.Equal(0, email.DelayHours);
    }

    [Fact]
    public void Email_CreatedAt_IsSet()
    {
        // Arrange
        var markdownEmail = MarkdownEmail.FromString(ValidMarkdown);

        // Act
        var email = new Email(markdownEmail);

        // Assert
        Assert.NotEqual(default(DateTimeOffset), email.CreatedAt);
    }
}
