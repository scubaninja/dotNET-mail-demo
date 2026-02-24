using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

public class EmailTests
{
    private const string ValidMarkdown = @"---
Subject: Welcome Email
Summary: Welcome to our service
---
# Welcome

Thanks for joining!
";

    [Fact]
    public void Email_Constructor_SetsSlugFromData()
    {
        // Arrange
        var markdownEmail = MarkdownEmail.FromString(ValidMarkdown);

        // Act
        var email = new Email(markdownEmail);

        // Assert
        Assert.Equal("welcome-email", email.Slug);
    }

    [Fact]
    public void Email_Constructor_SetsSubjectFromData()
    {
        // Arrange
        var markdownEmail = MarkdownEmail.FromString(ValidMarkdown);

        // Act
        var email = new Email(markdownEmail);

        // Assert
        Assert.Equal("Welcome Email", email.Subject);
    }

    [Fact]
    public void Email_Constructor_SetsPreviewFromSummary()
    {
        // Arrange
        var markdownEmail = MarkdownEmail.FromString(ValidMarkdown);

        // Act
        var email = new Email(markdownEmail);

        // Assert
        Assert.Equal("Welcome to our service", email.Preview);
    }

    [Fact]
    public void Email_Constructor_SetsHtml()
    {
        // Arrange
        var markdownEmail = MarkdownEmail.FromString(ValidMarkdown);

        // Act
        var email = new Email(markdownEmail);

        // Assert
        Assert.NotNull(email.Html);
        Assert.NotEmpty(email.Html);
    }

    [Fact]
    public void Email_Constructor_WithNullData_ThrowsInvalidDataException()
    {
        // Arrange
        var markdownEmail = new MarkdownEmail();

        // Act & Assert
        Assert.Throws<InvalidDataException>(() => new Email(markdownEmail));
    }
}
