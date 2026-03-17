using System;
using System.Dynamic;
using System.Collections.Generic;
using Xunit;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

public class MarkdownEmailTests
{
    private const string ValidMarkdown = @"---
Subject: ""Hello World""
Summary: ""A test email""
---

# Hello

This is a test email.";

    private const string MarkdownWithSlug = @"---
Subject: ""Hello World""
Summary: ""A test email""
Slug: ""custom-slug""
---

# Hello";

    private const string MarkdownWithTag = @"---
Subject: ""Hello World""
Summary: ""A test email""
SendToTag: ""vip""
---

# Hello";

    private const string MarkdownMissingSubject = @"---
Summary: ""A test email""
---

# Hello";

    private const string MarkdownMissingSummary = @"---
Subject: ""Hello World""
---

# Hello";

    [Fact]
    public void FromString_WithValidMarkdown_ParsesFrontMatterSubject()
    {
        var email = MarkdownEmail.FromString(ValidMarkdown);

        Assert.NotNull(email.Data);
        Assert.Equal("Hello World", (string)email.Data.Subject);
    }

    [Fact]
    public void FromString_WithValidMarkdown_ParsesFrontMatterSummary()
    {
        var email = MarkdownEmail.FromString(ValidMarkdown);

        Assert.NotNull(email.Data);
        Assert.Equal("A test email", (string)email.Data.Summary);
    }

    [Fact]
    public void FromString_WithValidMarkdown_GeneratesHtml()
    {
        var email = MarkdownEmail.FromString(ValidMarkdown);

        Assert.NotNull(email.Html);
        Assert.Contains("Hello</h1>", email.Html);
    }

    [Fact]
    public void FromString_WithValidMarkdown_StoresMarkdown()
    {
        var email = MarkdownEmail.FromString(ValidMarkdown);

        Assert.Equal(ValidMarkdown, email.Markdown);
    }

    [Fact]
    public void FromString_WithoutSlug_GeneratesSlugFromSubject()
    {
        var email = MarkdownEmail.FromString(ValidMarkdown);

        Assert.Equal("hello-world", (string)email.Data.Slug);
    }

    [Fact]
    public void FromString_WithCustomSlug_UsesCustomSlug()
    {
        var email = MarkdownEmail.FromString(MarkdownWithSlug);

        Assert.Equal("custom-slug", (string)email.Data.Slug);
    }

    [Fact]
    public void FromString_WithoutSendToTag_DefaultsToAll()
    {
        var email = MarkdownEmail.FromString(ValidMarkdown);

        Assert.Equal("*", (string)email.Data.SendToTag);
    }

    [Fact]
    public void FromString_WithCustomSendToTag_UsesCustomTag()
    {
        var email = MarkdownEmail.FromString(MarkdownWithTag);

        Assert.Equal("vip", (string)email.Data.SendToTag);
    }

    [Fact]
    public void IsValid_WithSubjectAndSummary_ReturnsTrue()
    {
        var email = MarkdownEmail.FromString(ValidMarkdown);

        Assert.True(email.IsValid());
    }

    [Fact]
    public void IsValid_WithMissingSubject_ThrowsException()
    {
        // When Subject is missing from the dynamic Data, accessing Data.Subject throws
        var email = new MarkdownEmail();
        dynamic data = new ExpandoObject();
        ((IDictionary<string, object>)data)["Summary"] = "A test email";
        email.Data = data;

        Assert.ThrowsAny<Exception>(() => email.IsValid());
    }

    [Fact]
    public void IsValid_WithMissingSummary_ThrowsException()
    {
        // When Summary is missing from the dynamic Data, accessing Data.Summary throws
        var email = new MarkdownEmail();
        dynamic data = new ExpandoObject();
        ((IDictionary<string, object>)data)["Subject"] = "Hello World";
        email.Data = data;

        Assert.ThrowsAny<Exception>(() => email.IsValid());
    }

    [Fact]
    public void IsValid_WithNullData_ReturnsFalse()
    {
        var email = new MarkdownEmail();

        Assert.False(email.IsValid());
    }
}
