using System;
using Xunit;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

public class EmailModelTests
{
    private const string ValidMarkdown = @"---
Subject: ""Hello World""
Summary: ""A preview of the email""
Slug: ""hello-world""
---

# Hello

This is a test email with some content.";

    [Fact]
    public void Constructor_WithValidMarkdownEmail_SetsSubject()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);

        var email = new Email(doc);

        Assert.Equal("Hello World", email.Subject);
    }

    [Fact]
    public void Constructor_WithValidMarkdownEmail_SetsSlug()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);

        var email = new Email(doc);

        Assert.Equal("hello-world", email.Slug);
    }

    [Fact]
    public void Constructor_WithValidMarkdownEmail_SetsPreview()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);

        var email = new Email(doc);

        Assert.Equal("A preview of the email", email.Preview);
    }

    [Fact]
    public void Constructor_WithValidMarkdownEmail_SetsHtml()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);

        var email = new Email(doc);

        Assert.NotNull(email.Html);
        Assert.Contains("Hello</h1>", email.Html);
    }

    [Fact]
    public void Constructor_WithValidMarkdownEmail_DefaultDelayHoursIsZero()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);

        var email = new Email(doc);

        Assert.Equal(0, email.DelayHours);
    }

    [Fact]
    public void Constructor_WithNullData_ThrowsInvalidDataException()
    {
        var doc = new MarkdownEmail
        {
            Html = "<p>Hello</p>"
        };

        Assert.Throws<InvalidDataException>(() => new Email(doc));
    }

    [Fact]
    public void Constructor_WithNullHtml_ThrowsInvalidDataException()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);
        doc.Html = null;

        Assert.Throws<InvalidDataException>(() => new Email(doc));
    }
}
