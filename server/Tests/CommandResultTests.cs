using System;
using Xunit;
using Tailwind.Data;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

public class CommandResultTests
{
    [Fact]
    public void CommandResult_DefaultInsertedIsZero()
    {
        var result = new CommandResult();

        Assert.Equal(0, result.Inserted);
    }

    [Fact]
    public void CommandResult_DefaultUpdatedIsZero()
    {
        var result = new CommandResult();

        Assert.Equal(0, result.Updated);
    }

    [Fact]
    public void CommandResult_DefaultDeletedIsZero()
    {
        var result = new CommandResult();

        Assert.Equal(0, result.Deleted);
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
    public void CommandResult_CanSetData()
    {
        var result = new CommandResult { Data = new { Success = true } };

        Assert.NotNull(result.Data);
    }
}
