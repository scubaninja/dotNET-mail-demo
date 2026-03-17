using System;
using Xunit;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

public class BroadcastTests
{
    private const string ValidMarkdown = @"---
Subject: ""Weekly Update""
Summary: ""This week's highlights""
Slug: ""weekly-update""
SendToTag: ""subscribers""
---

# Weekly Update

Here is this week's content.";

    private const string MinimalMarkdown = @"---
Subject: ""Minimal Email""
Summary: ""A minimal email""
---

# Body";

    [Fact]
    public void FromMarkdownEmail_WithValidEmail_SetsName()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);

        var broadcast = Broadcast.FromMarkdownEmail(doc);

        Assert.Equal("Weekly Update", broadcast.Name);
    }

    [Fact]
    public void FromMarkdownEmail_WithValidEmail_SetsSlug()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);

        var broadcast = Broadcast.FromMarkdownEmail(doc);

        Assert.Equal("weekly-update", broadcast.Slug);
    }

    [Fact]
    public void FromMarkdownEmail_WithValidEmail_SetsSendToTag()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);

        var broadcast = Broadcast.FromMarkdownEmail(doc);

        Assert.Equal("subscribers", broadcast.SendToTag);
    }

    [Fact]
    public void FromMarkdownEmail_WithNoSendToTag_DefaultsToAll()
    {
        var doc = MarkdownEmail.FromString(MinimalMarkdown);

        var broadcast = Broadcast.FromMarkdownEmail(doc);

        Assert.Equal("*", broadcast.SendToTag);
    }

    [Fact]
    public void FromMarkdownEmail_WithNoSlug_GeneratesSlugFromSubject()
    {
        var doc = MarkdownEmail.FromString(MinimalMarkdown);

        var broadcast = Broadcast.FromMarkdownEmail(doc);

        Assert.Equal("minimal-email", broadcast.Slug);
    }

    [Fact]
    public void FromMarkdownEmail_WithNullEmail_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Broadcast.FromMarkdownEmail(null!));
    }

    [Fact]
    public void FromMarkdownEmail_WithNullData_ThrowsArgumentNullException()
    {
        var doc = new MarkdownEmail();

        Assert.Throws<ArgumentNullException>(() => Broadcast.FromMarkdownEmail(doc));
    }

    [Fact]
    public void FromMarkdown_WithValidMarkdown_CreatesBroadcast()
    {
        var broadcast = Broadcast.FromMarkdown(ValidMarkdown);

        Assert.NotNull(broadcast);
        Assert.Equal("Weekly Update", broadcast.Name);
    }

    [Fact]
    public void FromMarkdown_WithEmptyString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Broadcast.FromMarkdown(string.Empty));
    }

    [Fact]
    public void FromMarkdown_WithNullString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Broadcast.FromMarkdown(null!));
    }

    [Fact]
    public void FromMarkdown_WithWhitespaceOnly_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Broadcast.FromMarkdown("   "));
    }

    [Fact]
    public void NewBroadcast_HasPendingStatus()
    {
        var broadcast = Broadcast.FromMarkdown(ValidMarkdown);

        Assert.Equal("pending", broadcast.Status);
    }
}
