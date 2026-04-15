using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Tests;

/// <summary>Tests for the <see cref="Tag"/> model.</summary>
public class TagTests
{
    [Fact]
    public void Tag_Constructor_WithName_SetsNameProperty()
    {
        var tag = new Tag("Newsletter");
        Assert.Equal("Newsletter", tag.Name);
    }

    [Fact]
    public void Tag_Constructor_SingleWord_GeneratesLowercaseSlug()
    {
        var tag = new Tag("Newsletter");
        Assert.Equal("newsletter", tag.Slug);
    }

    [Fact]
    public void Tag_Constructor_MultiWord_GeneratesHyphenatedSlug()
    {
        var tag = new Tag("New Arrivals");
        Assert.Equal("new-arrivals", tag.Slug);
    }

    [Fact]
    public void Tag_Constructor_AlreadyLowercase_SlugMatchesName()
    {
        var tag = new Tag("vip");
        Assert.Equal("vip", tag.Slug);
    }

    [Theory]
    [InlineData("Top Customers", "top-customers")]
    [InlineData("Monthly VIP", "monthly-vip")]
    [InlineData("welcome", "welcome")]
    public void Tag_Constructor_VariousNames_ProducesExpectedSlugs(string name, string expectedSlug)
    {
        var tag = new Tag(name);
        Assert.Equal(expectedSlug, tag.Slug);
    }

    [Fact]
    public void Tag_DefaultConstructor_HasNullProperties()
    {
        var tag = new Tag();
        Assert.Null(tag.Name);
        Assert.Null(tag.Slug);
        Assert.Null(tag.Description);
        Assert.Null(tag.ID);
    }
}
