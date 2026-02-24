using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

public class TagTests
{
    [Fact]
    public void Tag_Constructor_SetsNameAndSlug()
    {
        // Arrange & Act
        var tag = new Tag("Newsletter");

        // Assert
        Assert.Equal("Newsletter", tag.Name);
        Assert.Equal("newsletter", tag.Slug);
    }

    [Fact]
    public void Tag_Constructor_SlugReplacesSpacesWithHyphens()
    {
        // Arrange & Act
        var tag = new Tag("My Tag Name");

        // Assert
        Assert.Equal("my-tag-name", tag.Slug);
    }

    [Fact]
    public void Tag_DefaultConstructor_PropertiesAreNull()
    {
        // Arrange & Act
        var tag = new Tag();

        // Assert
        Assert.Null(tag.Name);
        Assert.Null(tag.Slug);
        Assert.Null(tag.ID);
    }
}
