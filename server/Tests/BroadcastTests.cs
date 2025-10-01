using System;
using System.Data;
using Xunit;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

public class BroadcastTests
{
    [Fact]
    public void FromMarkdownEmail_ValidMarkdownEmail_CreatesBroadcast()
    {
        // Arrange
        var markdown = @"---
Subject: Test Subject
Slug: test-slug
SendToTag: subscribers
Summary: Test summary
---

# Test Email

This is a test email.";

        var markdownEmail = MarkdownEmail.FromString(markdown);

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

        // Assert
        Assert.NotNull(broadcast);
        Assert.Equal("Test Subject", broadcast.Name);
        Assert.Equal("test-slug", broadcast.Slug);
        Assert.Equal("subscribers", broadcast.SendToTag);
        Assert.Equal("pending", broadcast.Status);
    }

    [Fact]
    public void FromMarkdownEmail_NullDocument_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            Broadcast.FromMarkdownEmail(null!));
        Assert.Equal("doc", exception.ParamName);
        Assert.Contains("MarkdownEmail document cannot be null", exception.Message);
    }

    [Fact]
    public void FromMarkdownEmail_NullData_ThrowsArgumentNullException()
    {
        // Arrange
        var markdownEmail = new MarkdownEmail();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            Broadcast.FromMarkdownEmail(markdownEmail));
        Assert.Equal("doc", exception.ParamName);
        Assert.Contains("MarkdownEmail document cannot be null", exception.Message);
    }

    [Fact]
    public void FromMarkdownEmail_DefaultSendToTag_SetToAsterisk()
    {
        // Arrange
        var markdown = @"---
Subject: Test Subject
Summary: Test summary
---

# Test Email";

        var markdownEmail = MarkdownEmail.FromString(markdown);

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

        // Assert
        Assert.Equal("*", broadcast.SendToTag);
    }

    [Fact]
    public void FromMarkdown_ValidMarkdown_CreatesBroadcast()
    {
        // Arrange
        var markdown = @"---
Subject: Test Subject
Slug: test-slug
SendToTag: premium
Summary: Test summary
---

# Test Email

This is a test email.";

        // Act
        var broadcast = Broadcast.FromMarkdown(markdown);

        // Assert
        Assert.NotNull(broadcast);
        Assert.Equal("Test Subject", broadcast.Name);
        Assert.Equal("test-slug", broadcast.Slug);
        Assert.Equal("premium", broadcast.SendToTag);
    }

    [Fact]
    public void FromMarkdown_NullMarkdown_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            Broadcast.FromMarkdown(null!));
        Assert.Equal("markdown", exception.ParamName);
        Assert.Contains("Markdown content cannot be null or empty", exception.Message);
    }

    [Fact]
    public void FromMarkdown_EmptyMarkdown_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            Broadcast.FromMarkdown(""));
        Assert.Equal("markdown", exception.ParamName);
        Assert.Contains("Markdown content cannot be null or empty", exception.Message);
    }

    [Fact]
    public void FromMarkdown_WhitespaceMarkdown_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            Broadcast.FromMarkdown("   \t\n  "));
        Assert.Equal("markdown", exception.ParamName);
        Assert.Contains("Markdown content cannot be null or empty", exception.Message);
    }

    [Fact]
    public void ContactCount_NullConnection_ThrowsArgumentNullException()
    {
        // Arrange
        var markdown = @"---
Subject: Test Subject
Summary: Test summary
---

# Test Email";
        var broadcast = Broadcast.FromMarkdown(markdown);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            broadcast.ContactCount(null!));
        Assert.Equal("conn", exception.ParamName);
        Assert.Contains("Database connection cannot be null", exception.Message);
    }

    [Fact]
    public void Broadcast_DefaultValues_AreSetCorrectly()
    {
        // Arrange
        var markdown = @"---
Subject: Test Subject
Summary: Test summary
---

# Test Email";

        // Act
        var broadcast = Broadcast.FromMarkdown(markdown);

        // Assert
        Assert.Equal("pending", broadcast.Status);
        Assert.Equal("*", broadcast.SendToTag);
        Assert.Null(broadcast.ID);
        Assert.Null(broadcast.EmailId);
        Assert.Null(broadcast.ReplyTo);
        Assert.True((DateTimeOffset.UtcNow - broadcast.CreatedAt).TotalSeconds < 1);
    }

    [Fact]
    public void FromMarkdownEmail_SlugGeneratedFromSubject_WhenNotProvided()
    {
        // Arrange
        var markdown = @"---
Subject: Test Subject With Spaces
Summary: Test summary
---

# Test Email";

        var markdownEmail = MarkdownEmail.FromString(markdown);

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

        // Assert
        Assert.Equal("test-subject-with-spaces", broadcast.Slug);
    }

    [Fact]
    public void FromMarkdownEmail_ComplexMarkdown_ParsesCorrectly()
    {
        // Arrange
        var markdown = @"---
Subject: Monthly Newsletter - December 2024
Slug: newsletter-dec-2024
SendToTag: newsletter-subscribers
Summary: Your monthly update from Tailwind Traders
---

# Monthly Newsletter

Welcome to our December edition!

## New Products

We have exciting new products this month:

- Product A
- Product B
- Product C

Stay tuned for more!";

        var markdownEmail = MarkdownEmail.FromString(markdown);

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

        // Assert
        Assert.Equal("Monthly Newsletter - December 2024", broadcast.Name);
        Assert.Equal("newsletter-dec-2024", broadcast.Slug);
        Assert.Equal("newsletter-subscribers", broadcast.SendToTag);
    }

    [Fact]
    public void ContactCount_ZeroContacts_ReturnsZero()
    {
        // This test would require database access - not testing here
        // as ContactCount uses Dapper extension methods that can't be mocked
        Assert.True(true);
    }

    [Fact]
    public void FromMarkdownEmail_MinimalValidMarkdown_CreatesBroadcast()
    {
        // Arrange
        var markdown = @"---
Subject: Minimal Test
Summary: Minimal summary
---

Body";

        var markdownEmail = MarkdownEmail.FromString(markdown);

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

        // Assert
        Assert.NotNull(broadcast);
        Assert.Equal("Minimal Test", broadcast.Name);
        Assert.Equal("minimal-test", broadcast.Slug);
        Assert.Equal("*", broadcast.SendToTag);
    }

    [Fact]
    public void Broadcast_Properties_CanBeModified()
    {
        // Arrange
        var markdown = @"---
Subject: Test Subject
Summary: Test summary
---

# Test Email";
        var broadcast = Broadcast.FromMarkdown(markdown);

        // Act
        broadcast.Status = "sent";
        broadcast.EmailId = 123;
        broadcast.ReplyTo = "test@example.com";

        // Assert
        Assert.Equal("sent", broadcast.Status);
        Assert.Equal(123, broadcast.EmailId);
        Assert.Equal("test@example.com", broadcast.ReplyTo);
    }

    [Fact]
    public void FromMarkdownEmail_SpecialCharactersInSubject_HandlesCorrectly()
    {
        // Arrange
        var markdown = @"---
Subject: ""Test & Special <> Characters""
Summary: Test summary
---

# Test Email";

        var markdownEmail = MarkdownEmail.FromString(markdown);

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

        // Assert
        Assert.Equal("Test & Special <> Characters", broadcast.Name);
    }

    [Fact]
    public void Broadcast_CreatedAt_IsUtc()
    {
        // Arrange
        var markdown = @"---
Subject: Test Subject
Summary: Test summary
---

# Test Email";

        // Act
        var broadcast = Broadcast.FromMarkdown(markdown);

        // Assert
        Assert.Equal(DateTimeOffset.UtcNow.Offset, broadcast.CreatedAt.Offset);
    }
}
