using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for the <see cref="Email"/> model constructor, verifying that
/// it correctly maps properties from a parsed <see cref="MarkdownEmail"/> document.
/// </summary>
public class EmailModelTests
{
    private const string ValidMarkdown = @"---
Subject: Welcome Email
Summary: Onboarding email for new users
Slug: welcome-email
---
# Welcome

Thanks for signing up!
";

    // ──────────────────────────────────────────────────────────────
    // Constructor – happy path
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the <see cref="Email"/> constructor correctly populates
    /// <c>Slug</c>, <c>Subject</c>, <c>Preview</c>, and <c>Html</c> from
    /// a fully parsed <see cref="MarkdownEmail"/> document.
    /// </summary>
    [Fact]
    public void Constructor_ValidMarkdownEmail_SetsAllFields()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);
        var email = new Email(doc);

        Assert.Equal("welcome-email", email.Slug);
        Assert.Equal("Welcome Email", email.Subject);
        Assert.Equal("Onboarding email for new users", email.Preview);
        Assert.NotNull(email.Html);
        Assert.Contains("<h1", email.Html!);
    }

    /// <summary>
    /// Verifies that the <see cref="Email"/> constructor sets a default
    /// <c>DelayHours</c> of 0 and a non-null <c>CreatedAt</c> timestamp.
    /// </summary>
    [Fact]
    public void Constructor_ValidMarkdownEmail_DefaultsDelayHoursAndCreatedAt()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);
        var email = new Email(doc);

        Assert.Equal(0, email.DelayHours);
        Assert.True(email.CreatedAt > DateTimeOffset.MinValue);
    }

    // ──────────────────────────────────────────────────────────────
    // Constructor – error cases
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the <see cref="Email"/> constructor throws
    /// <see cref="InvalidDataException"/> when the document's <c>Data</c> is null.
    /// </summary>
    [Fact]
    public void Constructor_NullData_ThrowsInvalidDataException()
    {
        var doc = new MarkdownEmail(); // Data is null

        Assert.Throws<InvalidDataException>(() => new Email(doc));
    }

    /// <summary>
    /// Verifies that the <see cref="Email"/> constructor throws
    /// <see cref="InvalidDataException"/> when the document's <c>Html</c> is null
    /// (i.e., <c>Render</c> was never called or failed silently).
    /// </summary>
    [Fact]
    public void Constructor_NullHtml_ThrowsInvalidDataException()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);
        doc.Html = null; // Force Html to null after rendering

        Assert.Throws<InvalidDataException>(() => new Email(doc));
    }
}
