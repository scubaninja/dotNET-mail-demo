using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

public class MarkdownEmailTests
{
    private const string ValidMarkdown = @"---
Subject: Hello World
Summary: A test email summary
---
# Hello

This is the body of the email.
";

    private const string ValidMarkdownWithSlugAndTag = @"---
Subject: My Email
Summary: Summary text
Slug: my-custom-slug
SendToTag: vip
---
Body content here.
";

    [Fact]
    public void FromString_WithValidMarkdown_SetsSubjectAndSummary()
    {
        // Arrange & Act
        var email = MarkdownEmail.FromString(ValidMarkdown);

        // Assert
        Assert.Equal("Hello World", (string)email.Data.Subject);
        Assert.Equal("A test email summary", (string)email.Data.Summary);
    }

    [Fact]
    public void FromString_WithValidMarkdown_GeneratesHtml()
    {
        // Arrange & Act
        var email = MarkdownEmail.FromString(ValidMarkdown);

        // Assert
        Assert.NotNull(email.Html);
        Assert.NotEmpty(email.Html);
    }

    [Fact]
    public void FromString_WithValidMarkdown_GeneratesSlugFromSubject()
    {
        // Arrange & Act
        var email = MarkdownEmail.FromString(ValidMarkdown);

        // Assert
        Assert.Equal("hello-world", (string)email.Data.Slug);
    }

    [Fact]
    public void FromString_WithCustomSlug_UsesProvidedSlug()
    {
        // Arrange & Act
        var email = MarkdownEmail.FromString(ValidMarkdownWithSlugAndTag);

        // Assert
        Assert.Equal("my-custom-slug", (string)email.Data.Slug);
    }

    [Fact]
    public void FromString_WithSendToTag_SetsSendToTag()
    {
        // Arrange & Act
        var email = MarkdownEmail.FromString(ValidMarkdownWithSlugAndTag);

        // Assert
        Assert.Equal("vip", (string)email.Data.SendToTag);
    }

    [Fact]
    public void FromString_WithoutSendToTag_DefaultsToStar()
    {
        // Arrange & Act
        var email = MarkdownEmail.FromString(ValidMarkdown);

        // Assert
        Assert.Equal("*", (string)email.Data.SendToTag);
    }

    [Fact]
    public void IsValid_WithSubjectAndSummary_ReturnsTrue()
    {
        // Arrange
        var email = MarkdownEmail.FromString(ValidMarkdown);

        // Act & Assert
        Assert.True(email.IsValid());
    }

    [Fact]
    public void FromFile_WithNonExistentFile_ThrowsException()
    {
        // Arrange & Act & Assert
        Assert.ThrowsAny<IOException>(() => MarkdownEmail.FromFile("/tmp/nonexistent-email-file.md"));
    }
}
