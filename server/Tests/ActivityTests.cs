using Xunit;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

public class ActivityTests
{
    [Fact]
    public void Activity_DefaultConstructor_InitializesKey()
    {
        // Arrange & Act
        var activity = new Activity();

        // Assert
        Assert.NotNull(activity.Key);
        Assert.NotEmpty(activity.Key);
    }

    [Fact]
    public void Activity_Key_IsGuid()
    {
        // Arrange & Act
        var activity = new Activity();

        // Assert
        Assert.True(Guid.TryParse(activity.Key, out _));
    }

    [Fact]
    public void Activity_CreatedAt_IsSet()
    {
        // Arrange & Act
        var activity = new Activity();

        // Assert
        Assert.NotEqual(default(DateTimeOffset), activity.CreatedAt);
    }

    [Fact]
    public void Activity_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var activity = new Activity();

        // Assert
        Assert.Null(activity.ID);
        Assert.Null(activity.ContactId);
        Assert.Null(activity.Description);
    }

    [Fact]
    public void Activity_CanSetProperties()
    {
        // Arrange
        var activity = new Activity();

        // Act
        activity.ContactId = 123;
        activity.Description = "Test activity";
        activity.Key = "custom-key";

        // Assert
        Assert.Equal(123, activity.ContactId);
        Assert.Equal("Test activity", activity.Description);
        Assert.Equal("custom-key", activity.Key);
    }
}
