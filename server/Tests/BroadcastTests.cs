using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Tests;

/// <summary>Tests for <see cref="Broadcast"/> model creation and validation.</summary>
public class BroadcastTests
{
    private const string ValidMarkdown = """
        ---
        Subject: June Newsletter
        Summary: Our monthly update
        Slug: june-newsletter
        ---
        This month's highlights go here.
        """;

    private const string MarkdownWithTag = """
        ---
        Subject: VIP Offer
        Summary: An exclusive offer for VIP members
        SendToTag: vip
        ---
        Special content for our VIP members.
        """;

    private const string MarkdownNoSlug = """
        ---
        Subject: Quick Update
        Summary: A quick status update
        ---
        The team has been busy.
        """;

    [Fact]
    public void FromMarkdown_ValidMarkdown_SetsBroadcastName()
    {
        var broadcast = Broadcast.FromMarkdown(ValidMarkdown);
        Assert.Equal("June Newsletter", broadcast.Name);
    }

    [Fact]
    public void FromMarkdown_ValidMarkdown_SetsExplicitSlug()
    {
        var broadcast = Broadcast.FromMarkdown(ValidMarkdown);
        Assert.Equal("june-newsletter", broadcast.Slug);
    }

    [Fact]
    public void FromMarkdown_ValidMarkdown_DefaultsSendToTagToWildcard()
    {
        var broadcast = Broadcast.FromMarkdown(ValidMarkdown);
        Assert.Equal("*", broadcast.SendToTag);
    }

    [Fact]
    public void FromMarkdown_WithSendToTag_SetsSendToTag()
    {
        var broadcast = Broadcast.FromMarkdown(MarkdownWithTag);
        Assert.Equal("vip", broadcast.SendToTag);
    }

    [Fact]
    public void FromMarkdown_NoExplicitSlug_GeneratesSlugFromSubject()
    {
        var broadcast = Broadcast.FromMarkdown(MarkdownNoSlug);
        Assert.Equal("quick-update", broadcast.Slug);
    }

    [Fact]
    public void FromMarkdown_NullMarkdown_ThrowsArgumentException()
        => Assert.Throws<ArgumentException>(() => Broadcast.FromMarkdown(null!));

    [Fact]
    public void FromMarkdown_EmptyMarkdown_ThrowsArgumentException()
        => Assert.Throws<ArgumentException>(() => Broadcast.FromMarkdown(""));

    [Fact]
    public void FromMarkdown_WhitespaceMarkdown_ThrowsArgumentException()
        => Assert.Throws<ArgumentException>(() => Broadcast.FromMarkdown("   "));

    [Fact]
    public void FromMarkdownEmail_ValidDoc_SetsBroadcast()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);
        var broadcast = Broadcast.FromMarkdownEmail(doc);
        Assert.Equal("June Newsletter", broadcast.Name);
        Assert.Equal("june-newsletter", broadcast.Slug);
    }

    [Fact]
    public void FromMarkdownEmail_NullDoc_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => Broadcast.FromMarkdownEmail(null!));

    [Fact]
    public void DefaultStatus_IsPending()
    {
        var broadcast = Broadcast.FromMarkdown(ValidMarkdown);
        Assert.Equal("pending", broadcast.Status);
    }

    [Fact]
    public void NewBroadcast_HasNoId()
    {
        var broadcast = Broadcast.FromMarkdown(ValidMarkdown);
        Assert.Null(broadcast.ID);
    }

    [Fact]
    public void NewBroadcast_HasNoEmailId()
    {
        var broadcast = Broadcast.FromMarkdown(ValidMarkdown);
        Assert.Null(broadcast.EmailId);
    }
}
