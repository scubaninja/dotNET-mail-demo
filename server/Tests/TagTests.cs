using Xunit;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

public class TagTests
{
    [Fact]
    public void Tag_ParameterizedConstructor_SetsNameAndSlug()
    {
        // Arrange
        var name = "Test Tag";

        // Act
        var tag = new Tag(name);

        // Assert
        Assert.Equal("Test Tag", tag.Name);
        Assert.Equal("test-tag", tag.Slug);
    }

    [Fact]
    public void Tag_DefaultConstructor_InitializesSuccessfully()
    {
        // Arrange & Act
        var tag = new Tag();

        // Assert
        Assert.Null(tag.ID);
        Assert.Null(tag.Name);
        Assert.Null(tag.Slug);
        Assert.Null(tag.Description);
    }

    [Fact]
    public void Tag_Slug_IsLowercaseWithHyphens()
    {
        // Arrange
        var name = "My Complex Tag Name";

        // Act
        var tag = new Tag(name);

        // Assert
        Assert.Equal("my-complex-tag-name", tag.Slug);
    }

    [Fact]
    public void Tag_CanSetDescription()
    {
        // Arrange
        var tag = new Tag("Test");

        // Act
        tag.Description = "This is a test tag";

        // Assert
        Assert.Equal("This is a test tag", tag.Description);
    }
}

public class TaggedTests
{
    [Fact]
    public void Tagged_DefaultValues_AreNull()
    {
        // Arrange & Act
        var tagged = new Tagged();

        // Assert
        Assert.Null(tagged.ContactId);
        Assert.Null(tagged.TagId);
    }

    [Fact]
    public void Tagged_CanSetContactId()
    {
        // Arrange
        var tagged = new Tagged();

        // Act
        tagged.ContactId = 123;

        // Assert
        Assert.Equal(123, tagged.ContactId);
    }

    [Fact]
    public void Tagged_CanSetTagId()
    {
        // Arrange
        var tagged = new Tagged();

        // Act
        tagged.TagId = 456;

        // Assert
        Assert.Equal(456, tagged.TagId);
    }
}
