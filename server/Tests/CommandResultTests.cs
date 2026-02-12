using Xunit;
using Tailwind.Data;

namespace Tailwind.Mail.Tests;

public class CommandResultTests
{
    [Fact]
    public void CommandResult_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var result = new CommandResult();

        // Assert
        Assert.Equal(0, result.Inserted);
        Assert.Equal(0, result.Updated);
        Assert.Equal(0, result.Deleted);
        Assert.Null(result.Data);
    }

    [Fact]
    public void CommandResult_CanSetInserted()
    {
        // Arrange
        var result = new CommandResult();

        // Act
        result.Inserted = 5;

        // Assert
        Assert.Equal(5, result.Inserted);
    }

    [Fact]
    public void CommandResult_CanSetUpdated()
    {
        // Arrange
        var result = new CommandResult();

        // Act
        result.Updated = 3;

        // Assert
        Assert.Equal(3, result.Updated);
    }

    [Fact]
    public void CommandResult_CanSetDeleted()
    {
        // Arrange
        var result = new CommandResult();

        // Act
        result.Deleted = 2;

        // Assert
        Assert.Equal(2, result.Deleted);
    }

    [Fact]
    public void CommandResult_CanSetDynamicData()
    {
        // Arrange
        var result = new CommandResult();
        var data = new { Success = true, Message = "Operation completed" };

        // Act
        result.Data = data;

        // Assert
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Success);
        Assert.Equal("Operation completed", result.Data.Message);
    }
}
