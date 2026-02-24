using Tailwind.Data;
using Xunit;

namespace Tailwind.Mail.Tests;

public class CommandResultTests
{
    [Fact]
    public void CommandResult_DefaultValues_AreZero()
    {
        // Arrange & Act
        var result = new CommandResult();

        // Assert
        Assert.Equal(0, result.Inserted);
        Assert.Equal(0, result.Updated);
        Assert.Equal(0, result.Deleted);
    }

    [Fact]
    public void CommandResult_CanSetInserted()
    {
        // Arrange & Act
        var result = new CommandResult { Inserted = 5 };

        // Assert
        Assert.Equal(5, result.Inserted);
    }

    [Fact]
    public void CommandResult_CanSetUpdated()
    {
        // Arrange & Act
        var result = new CommandResult { Updated = 3 };

        // Assert
        Assert.Equal(3, result.Updated);
    }

    [Fact]
    public void CommandResult_CanSetDeleted()
    {
        // Arrange & Act
        var result = new CommandResult { Deleted = 2 };

        // Assert
        Assert.Equal(2, result.Deleted);
    }

    [Fact]
    public void CommandResult_CanSetData()
    {
        // Arrange
        var data = new { Success = true, Message = "OK" };

        // Act
        var result = new CommandResult { Data = data };

        // Assert
        Assert.NotNull(result.Data);
    }
}
