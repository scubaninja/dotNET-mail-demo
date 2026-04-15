using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Tests;

/// <summary>Tests for the <see cref="Email"/> model, which is persisted to the database.</summary>
public class EmailModelTests
{
    private const string ValidMarkdown = """
        ---
        Subject: Test Email
        Summary: A test email preview
        Slug: test-email
        ---
        Email body content here.
        """;

    private const string MarkdownNoSlug = """
        ---
        Subject: Auto Slug Email
        Summary: An email where the slug is derived from the subject
        ---
        Some content.
        """;

    [Fact]
    public void Email_FromMarkdownEmail_SetsSlugFromFrontMatter()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);
        var email = new Email(doc);
        Assert.Equal("test-email", email.Slug);
    }

    [Fact]
    public void Email_FromMarkdownEmail_SetsSubject()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);
        var email = new Email(doc);
        Assert.Equal("Test Email", email.Subject);
    }

    [Fact]
    public void Email_FromMarkdownEmail_SetsPreviewFromSummary()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);
        var email = new Email(doc);
        Assert.Equal("A test email preview", email.Preview);
    }

    [Fact]
    public void Email_FromMarkdownEmail_SetsHtml()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);
        var email = new Email(doc);
        Assert.NotNull(email.Html);
        Assert.Contains("Email body content here", email.Html);
    }

    [Fact]
    public void Email_DefaultDelayHours_IsZero()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);
        var email = new Email(doc);
        Assert.Equal(0, email.DelayHours);
    }

    [Fact]
    public void Email_NewInstance_HasNoId()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);
        var email = new Email(doc);
        Assert.Null(email.ID);
    }

    [Fact]
    public void Email_FromMarkdownEmail_AutoSlugFromSubject()
    {
        var doc = MarkdownEmail.FromString(MarkdownNoSlug);
        var email = new Email(doc);
        Assert.Equal("auto-slug-email", email.Slug);
    }

    [Fact]
    public void Email_NullData_ThrowsInvalidDataException()
    {
        var doc = new MarkdownEmail();
        Assert.Throws<InvalidDataException>(() => new Email(doc));
    }

    [Fact]
    public void Email_NullHtml_ThrowsInvalidDataException()
    {
        var doc = new MarkdownEmail();
        Assert.ThrowsAny<Exception>(() => new Email(doc));
    }
}
