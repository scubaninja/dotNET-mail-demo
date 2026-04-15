using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Tests;

/// <summary>Tests for <see cref="MarkdownEmail"/> parsing and validation.</summary>
public class MarkdownEmailTests
{
    private const string ValidMarkdown = """
        ---
        Subject: Welcome to Tailwind
        Summary: Your welcome email
        Slug: welcome-tailwind
        ---

        Hello! Welcome to our mailing list.
        """;

    private const string ValidMarkdownNoSlug = """
        ---
        Subject: Hello World
        Summary: A simple greeting
        ---
        This is the email body.
        """;

    private const string ValidMarkdownWithTag = """
        ---
        Subject: VIP Offer
        Summary: An exclusive offer
        SendToTag: vip-customers
        ---
        Special content for VIP members.
        """;

    [Fact]
    public void FromString_ValidMarkdown_ParsesSubject()
    {
        var email = MarkdownEmail.FromString(ValidMarkdown);
        Assert.Equal("Welcome to Tailwind", (string)email.Data!.Subject);
    }

    [Fact]
    public void FromString_ValidMarkdown_ParsesSummary()
    {
        var email = MarkdownEmail.FromString(ValidMarkdown);
        Assert.Equal("Your welcome email", (string)email.Data!.Summary);
    }

    [Fact]
    public void FromString_ValidMarkdown_ParsesExplicitSlug()
    {
        var email = MarkdownEmail.FromString(ValidMarkdown);
        Assert.Equal("welcome-tailwind", (string)email.Data!.Slug);
    }

    [Fact]
    public void FromString_ValidMarkdown_GeneratesNonEmptyHtml()
    {
        var email = MarkdownEmail.FromString(ValidMarkdown);
        Assert.NotNull(email.Html);
        Assert.NotEmpty(email.Html);
    }

    [Fact]
    public void FromString_ValidMarkdown_HtmlContainsBodyText()
    {
        var email = MarkdownEmail.FromString(ValidMarkdown);
        Assert.Contains("Welcome to our mailing list", email.Html);
    }

    [Fact]
    public void FromString_NoSlugProvided_GeneratesSlugFromSubject()
    {
        var email = MarkdownEmail.FromString(ValidMarkdownNoSlug);
        Assert.Equal("hello-world", (string)email.Data!.Slug);
    }

    [Fact]
    public void FromString_NoSendToTag_DefaultsToWildcard()
    {
        var email = MarkdownEmail.FromString(ValidMarkdown);
        Assert.Equal("*", (string)email.Data!.SendToTag);
    }

    [Fact]
    public void FromString_WithSendToTag_ParsesTag()
    {
        var email = MarkdownEmail.FromString(ValidMarkdownWithTag);
        Assert.Equal("vip-customers", (string)email.Data!.SendToTag);
    }

    [Fact]
    public void IsValid_WithSubjectAndSummary_ReturnsTrue()
    {
        var email = MarkdownEmail.FromString(ValidMarkdown);
        Assert.True(email.IsValid());
    }

    [Fact]
    public void IsValid_MissingSubject_Throws()
    {
        // When Subject is absent, Render() cannot generate a slug from it and throws
        var markdown = """
            ---
            Summary: Only a summary, no subject
            ---
            Body text.
            """;
        Assert.ThrowsAny<Exception>(() => MarkdownEmail.FromString(markdown));
    }

    [Fact]
    public void IsValid_MissingSummary_Throws()
    {
        // Subject present but Summary absent; IsValid() cannot access the missing key
        var markdown = """
            ---
            Subject: Only a subject, no summary
            ---
            Body text.
            """;
        var email = MarkdownEmail.FromString(markdown);
        Assert.ThrowsAny<Exception>(() => email.IsValid());
    }

    [Fact]
    public void IsValid_NullData_ReturnsFalse()
    {
        var email = new MarkdownEmail();
        Assert.False(email.IsValid());
    }

    [Fact]
    public void FromString_StoredMarkdown_CanBeReadBack()
    {
        var email = MarkdownEmail.FromString(ValidMarkdown);
        Assert.NotNull(email.Markdown);
        Assert.Contains("Welcome to Tailwind", email.Markdown);
    }

    [Fact]
    public void FromString_NullMarkdown_ThrowsException()
    {
        Assert.Throws<Exception>(() => MarkdownEmail.FromString(null!));
    }
}
