using Microsoft.CSharp.RuntimeBinder;
using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for <see cref="MarkdownEmail"/> including parsing YAML front-matter,
/// converting Markdown to HTML, and validation.
/// </summary>
public class MarkdownEmailTests
{
    private const string ValidMarkdown = @"---
Subject: Hello World
Summary: A brief description
Slug: hello-world
SendToTag: '*'
---
# Hello World

This is the body.
";

    private const string MarkdownMissingSlug = @"---
Subject: Test Subject
Summary: Test Summary
---
# Body
";

    private const string MarkdownMissingSubject = @"---
Summary: Only Summary
---
# Body
";

    private const string MarkdownMissingSummary = @"---
Subject: Only Subject
---
# Body
";

    // ──────────────────────────────────────────────────────────────
    // FromString
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="MarkdownEmail.FromString"/> correctly parses the YAML
    /// front-matter, sets the <c>Subject</c> property, and generates HTML from the body.
    /// </summary>
    [Fact]
    public void FromString_ValidMarkdown_ParsesSubjectAndHtml()
    {
        var email = MarkdownEmail.FromString(ValidMarkdown);

        Assert.NotNull(email.Data);
        Assert.Equal("Hello World", (string)email.Data.Subject);
        Assert.NotNull(email.Html);
        Assert.Contains("<h1", email.Html!);
    }

    /// <summary>
    /// Verifies that <see cref="MarkdownEmail.FromString"/> auto-generates a slug
    /// from the subject when no explicit <c>Slug</c> key is present in the front-matter.
    /// </summary>
    [Fact]
    public void FromString_NoSlugInFrontMatter_AutoGeneratesSlugFromSubject()
    {
        var email = MarkdownEmail.FromString(MarkdownMissingSlug);

        // Slug should be derived: "Test Subject" -> "test subject" -> "test-subject"
        Assert.Equal("test-subject", (string)email.Data!.Slug);
    }

    /// <summary>
    /// Verifies that <see cref="MarkdownEmail.FromString"/> defaults <c>SendToTag</c>
    /// to <c>"*"</c> when no <c>SendToTag</c> key exists in the front-matter.
    /// </summary>
    [Fact]
    public void FromString_NoSendToTagInFrontMatter_DefaultsToWildcard()
    {
        var email = MarkdownEmail.FromString(MarkdownMissingSlug);

        Assert.Equal("*", (string)email.Data!.SendToTag);
    }

    /// <summary>
    /// Verifies that a <see cref="MarkdownEmail"/> constructed directly (without calling
    /// <c>FromString</c>) has null Data and Html properties.
    /// </summary>
    [Fact]
    public void DefaultConstructor_DataAndHtmlAreNull()
    {
        var email = new MarkdownEmail();

        Assert.Null(email.Data);
        Assert.Null(email.Html);
    }

    // ──────────────────────────────────────────────────────────────
    // IsValid
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="MarkdownEmail.IsValid"/> returns <c>true</c>
    /// when both <c>Subject</c> and <c>Summary</c> are present in the front-matter.
    /// </summary>
    [Fact]
    public void IsValid_SubjectAndSummaryPresent_ReturnsTrue()
    {
        var email = MarkdownEmail.FromString(ValidMarkdown);

        Assert.True(email.IsValid());
    }

    /// <summary>
    /// Verifies that <see cref="MarkdownEmail.IsValid"/> returns <c>false</c>
    /// when the <c>Data</c> property is null (i.e., <c>Render</c> was never called).
    /// </summary>
    [Fact]
    public void IsValid_DataIsNull_ReturnsFalse()
    {
        var email = new MarkdownEmail();

        Assert.False(email.IsValid());
    }

    /// <summary>
    /// Verifies that <see cref="MarkdownEmail.IsValid"/> throws
    /// <see cref="RuntimeBinderException"/> when the front-matter is missing the
    /// <c>Summary</c> key, because the dynamic property access on the ExpandoObject
    /// fails at runtime.
    /// </summary>
    [Fact]
    public void IsValid_MissingSummary_ThrowsRuntimeBinderException()
    {
        var email = MarkdownEmail.FromString(MarkdownMissingSummary);

        Assert.Throws<RuntimeBinderException>(() => email.IsValid());
    }

    /// <summary>
    /// Verifies that <see cref="MarkdownEmail.FromString"/> throws
    /// <see cref="RuntimeBinderException"/> when the front-matter is missing the
    /// <c>Subject</c> key, because the slug auto-generation accesses
    /// <c>Data.Subject</c> dynamically during <c>Render</c>.
    /// </summary>
    [Fact]
    public void FromString_MissingSubject_ThrowsRuntimeBinderException()
    {
        Assert.Throws<RuntimeBinderException>(() => MarkdownEmail.FromString(MarkdownMissingSubject));
    }

    // ──────────────────────────────────────────────────────────────
    // FromFile
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="MarkdownEmail.FromFile"/> reads a file from disk
    /// and produces the same result as <see cref="MarkdownEmail.FromString"/>.
    /// </summary>
    [Fact]
    public void FromFile_ValidFile_ParsesCorrectly()
    {
        var path = Path.GetTempFileName() + ".md";
        try
        {
            File.WriteAllText(path, ValidMarkdown);
            var email = MarkdownEmail.FromFile(path);

            Assert.NotNull(email.Data);
            Assert.Equal("Hello World", (string)email.Data.Subject);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
