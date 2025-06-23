using Xunit;
using Tailwind.Mail.Models;
using System;
using System.IO;
using System.Dynamic;

namespace Tailwind.Mail.Tests.Models;

public class MarkdownEmailTests
{
    private const string ValidMarkdown = @"---
Subject: Test Email
Summary: This is a test email
Slug: test-email
SendToTag: newsletter
---

# Test Email Heading

This is a test email content.

## Secondary Heading

- Item 1
- Item 2
- Item 3
";

    [Fact]
    public void FromString_ValidMarkdown_ParsesCorrectly()
    {
        // Act
        var email = MarkdownEmail.FromString(ValidMarkdown);
        
        // Assert
        Assert.NotNull(email.Html);
        Assert.NotNull(email.Data);
        Assert.Equal("Test Email", email.Data.Subject);
        Assert.Equal("This is a test email", email.Data.Summary);
        Assert.Equal("test-email", email.Data.Slug);
        Assert.Equal("newsletter", email.Data.SendToTag);
        Assert.Contains("<h1>Test Email Heading</h1>", email.Html);
        Assert.Contains("<h2>Secondary Heading</h2>", email.Html);
        Assert.Contains("<li>Item 1</li>", email.Html);
    }
    
    [Fact]
    public void FromString_MarkdownWithoutSlug_GeneratesSlugFromSubject()
    {
        // Arrange
        var markdownWithoutSlug = @"---
Subject: Test Email Without Slug
Summary: This is a test email
---

Content here.
";
        
        // Act
        var email = MarkdownEmail.FromString(markdownWithoutSlug);
        
        // Assert
        Assert.Equal("test-email-without-slug", email.Data.Slug);
    }
    
    [Fact]
    public void FromString_MarkdownWithoutSendToTag_SetsDefaultTag()
    {
        // Arrange
        var markdownWithoutTag = @"---
Subject: Test Email
Summary: This is a test email
---

Content here.
";
        
        // Act
        var email = MarkdownEmail.FromString(markdownWithoutTag);
        
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
    public void IsValid_MissingSubject_ReturnsFalse()
    {
        // Arrange
        var markdownMissingSubject = @"---
Summary: This is a test email
---

Content here.
";
        var email = MarkdownEmail.FromString(markdownMissingSubject);
        
        // Act
        var isValid = email.IsValid();
        
        // Assert
        Assert.False(isValid);
    }
    
    [Fact]
    public void IsValid_MissingSummary_ReturnsFalse()
    {
        // Arrange
        var markdownMissingSummary = @"---
Subject: Test Email
---

Content here.
";
        var email = MarkdownEmail.FromString(markdownMissingSummary);
        
        // Act
        var isValid = email.IsValid();
        
        // Assert
        Assert.False(isValid);
    }
    
    [Fact]
    public void Render_NullMarkdown_ThrowsException()
    {
        // Arrange
        var email = new MarkdownEmail();
        
        // Act & Assert
        var exception = Assert.Throws<Exception>(() => MarkdownEmail.FromString(null));
        Assert.Contains("Markdown is null", exception.Message);
    }
    
    [Fact]
    public void FromFile_ValidFile_ParsesCorrectly()
    {
        // Arrange
        var tempFilePath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFilePath, ValidMarkdown);
            
            // Act
            var email = MarkdownEmail.FromFile(tempFilePath);
            
            // Assert
            Assert.NotNull(email.Html);
            Assert.NotNull(email.Data);
            Assert.Equal("Test Email", email.Data.Subject);
            Assert.Equal("This is a test email", email.Data.Summary);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }
}
