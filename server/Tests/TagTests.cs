using System;
using Xunit;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

public class TagTests
{
    [Fact]
    public void Constructor_WithName_SetsName()
    {
        var tag = new Tag("VIP Customers");

        Assert.Equal("VIP Customers", tag.Name);
    }

    [Fact]
    public void Constructor_WithName_GeneratesSlug()
    {
        var tag = new Tag("VIP Customers");

        Assert.Equal("vip-customers", tag.Slug);
    }

    [Fact]
    public void Constructor_WithLowercaseName_SetsSlugLowercase()
    {
        var tag = new Tag("newsletter");

        Assert.Equal("newsletter", tag.Slug);
    }

    [Fact]
    public void Constructor_WithSingleWord_SetsSlugToLowercase()
    {
        var tag = new Tag("Subscribers");

        Assert.Equal("subscribers", tag.Slug);
    }

    [Fact]
    public void DefaultConstructor_CreatesTagWithNullFields()
    {
        var tag = new Tag();

        Assert.Null(tag.Name);
        Assert.Null(tag.Slug);
        Assert.Null(tag.Description);
    }
}
