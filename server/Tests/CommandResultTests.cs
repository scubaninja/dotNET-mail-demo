using Tailwind.Data;
using Xunit;

namespace Tailwind.Tests;

/// <summary>Tests for the <see cref="CommandResult"/> model.</summary>
public class CommandResultTests
{
    [Fact]
    public void CommandResult_DefaultValues_AreAllZero()
    {
        var result = new CommandResult();
        Assert.Equal(0, result.Inserted);
        Assert.Equal(0, result.Updated);
        Assert.Equal(0, result.Deleted);
    }

    [Fact]
    public void CommandResult_DefaultData_IsNull()
    {
        var result = new CommandResult();
        Assert.Null(result.Data);
    }

    [Fact]
    public void CommandResult_CanSetInserted()
    {
        var result = new CommandResult { Inserted = 5 };
        Assert.Equal(5, result.Inserted);
    }

    [Fact]
    public void CommandResult_CanSetUpdated()
    {
        var result = new CommandResult { Updated = 3 };
        Assert.Equal(3, result.Updated);
    }

    [Fact]
    public void CommandResult_CanSetDeleted()
    {
        var result = new CommandResult { Deleted = 2 };
        Assert.Equal(2, result.Deleted);
    }

    [Fact]
    public void CommandResult_CanSetDynamicData()
    {
        var result = new CommandResult { Data = new { Success = true, Message = "ok" } };
        Assert.NotNull(result.Data);
        Assert.True((bool)result.Data.Success);
    }
}
