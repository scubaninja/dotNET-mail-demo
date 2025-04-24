using Xunit;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

public class EmailTests
{
    [Fact]
    public void Constructor_WithValidMarkdownEmail_CreatesEmail()
    {
        // Arrange
        var doc = new MarkdownEmail {
            Data = new MarkdownMetadata {
                Slug = "test-slug",
                Subject = "Test Subject",
                Summary = "Test Preview"
            },
            Html = "<p>Test content</p>"
        };

        // Act
        var email = new Email(doc);

        // Assert
        Assert.Equal("test-slug", email.Slug);
        Assert.Equal("Test Subject", email.Subject);
        Assert.Equal("Test Preview", email.Preview);
        Assert.Equal("<p>Test content</p>", email.Html);
    }

    [Fact]
    public void Constructor_WithInvalidMarkdownEmail_ThrowsException()
    {
        // Arrange
        var doc = new MarkdownEmail { Html = "<p>Test</p>" };

        // Act & Assert
        Assert.Throws<InvalidDataException>(() => new Email(doc));
    }
}
