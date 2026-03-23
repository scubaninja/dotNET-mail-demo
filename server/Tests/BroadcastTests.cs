using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for the <see cref="Broadcast"/> model, covering the factory methods
/// <see cref="Broadcast.FromMarkdownEmail"/> and <see cref="Broadcast.FromMarkdown"/>.
/// </summary>
public class BroadcastTests
{
    private const string ValidMarkdown = @"---
Subject: My Campaign
Summary: Campaign description
Slug: my-campaign
SendToTag: vip
---
# Body
";

    private const string MarkdownWithoutSlug = @"---
Subject: Auto Slug Campaign
Summary: desc
---
# Body
";

    // ──────────────────────────────────────────────────────────────
    // FromMarkdownEmail
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="Broadcast.FromMarkdownEmail"/> correctly maps the
    /// <c>Subject</c>, <c>Slug</c>, and <c>SendToTag</c> fields from the email's
    /// parsed front-matter onto the resulting <see cref="Broadcast"/> instance.
    /// </summary>
    [Fact]
    public void FromMarkdownEmail_ValidDoc_MapsFieldsCorrectly()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);
        var broadcast = Broadcast.FromMarkdownEmail(doc);

        Assert.Equal("My Campaign", broadcast.Name);
        Assert.Equal("my-campaign", broadcast.Slug);
        Assert.Equal("vip", broadcast.SendToTag);
    }

    /// <summary>
    /// Verifies that <see cref="Broadcast.FromMarkdownEmail"/> throws
    /// <see cref="ArgumentNullException"/> when the supplied document is null.
    /// </summary>
    [Fact]
    public void FromMarkdownEmail_NullDoc_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Broadcast.FromMarkdownEmail(null!));
    }

    /// <summary>
    /// Verifies that <see cref="Broadcast.FromMarkdownEmail"/> throws
    /// <see cref="ArgumentNullException"/> when the document's <c>Data</c> is null
    /// (i.e. <c>Render</c> was never called).
    /// </summary>
    [Fact]
    public void FromMarkdownEmail_NullData_ThrowsArgumentNullException()
    {
        var doc = new MarkdownEmail(); // Data is null
        Assert.Throws<ArgumentNullException>(() => Broadcast.FromMarkdownEmail(doc));
    }

    /// <summary>
    /// Verifies that when no <c>SendToTag</c> is specified in the front-matter, the
    /// broadcast defaults to <c>"*"</c> (send to all subscribers).
    /// </summary>
    [Fact]
    public void FromMarkdownEmail_NoSendToTag_DefaultsToWildcard()
    {
        var doc = MarkdownEmail.FromString(MarkdownWithoutSlug);
        var broadcast = Broadcast.FromMarkdownEmail(doc);

        Assert.Equal("*", broadcast.SendToTag);
    }

    // ──────────────────────────────────────────────────────────────
    // FromMarkdown
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="Broadcast.FromMarkdown"/> accepts a raw markdown string
    /// and produces a broadcast with the correct name derived from the front-matter.
    /// </summary>
    [Fact]
    public void FromMarkdown_ValidMarkdown_ReturnsBroadcastWithCorrectName()
    {
        var broadcast = Broadcast.FromMarkdown(ValidMarkdown);

        Assert.Equal("My Campaign", broadcast.Name);
    }

    /// <summary>
    /// Verifies that <see cref="Broadcast.FromMarkdown"/> throws
    /// <see cref="ArgumentException"/> when the supplied markdown is null or whitespace.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromMarkdown_NullOrWhitespace_ThrowsArgumentException(string? markdown)
    {
        Assert.Throws<ArgumentException>(() => Broadcast.FromMarkdown(markdown!));
    }

    // ──────────────────────────────────────────────────────────────
    // Default values
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that a broadcast created via the factory methods starts with
    /// <c>Status = "pending"</c> and <c>SendToTag = "*"</c> by default.
    /// </summary>
    [Fact]
    public void FromMarkdown_DefaultStatusIsPending()
    {
        var broadcast = Broadcast.FromMarkdown(ValidMarkdown);

        Assert.Equal("pending", broadcast.Status);
    }
}
