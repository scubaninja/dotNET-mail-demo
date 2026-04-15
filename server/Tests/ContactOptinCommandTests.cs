using System.Data;
using Moq;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Tests;

/// <summary>Tests for <see cref="ContactOptinCommand"/>.</summary>
public class ContactOptinCommandTests
{
    private static Contact PendingContact()
        => new Contact("Carol", "carol@example.com") { ID = 7, Subscribed = false, Key = "optin-key" };

    [Fact]
    public void Execute_Success_ReturnsUpdatedCount()
    {
        var (mockConn, mockCmd, _) = DbMockBuilder.Create();

        // Update → ExecuteNonQuery returns 1 row affected; Insert activity → ExecuteReader
        mockCmd.Setup(c => c.ExecuteNonQuery()).Returns(1);
        mockCmd
            .Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
            .Returns(DbMockBuilder.BuildIdReader(10)); // Insert activity id

        var cmd = new ContactOptinCommand(PendingContact());
        var result = cmd.Execute(mockConn.Object);

        Assert.Equal(1, result.Updated);
        Assert.True((bool)result.Data.Success);
        Assert.Equal("Contact subscribed", (string)result.Data.Message);
    }

    [Fact]
    public void Execute_Success_SetsContactSubscribedToTrue()
    {
        var (mockConn, mockCmd, _) = DbMockBuilder.Create();

        mockCmd.Setup(c => c.ExecuteNonQuery()).Returns(1);
        mockCmd
            .Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
            .Returns(DbMockBuilder.BuildIdReader(10));

        var contact = PendingContact();
        Assert.False(contact.Subscribed); // precondition

        var cmd = new ContactOptinCommand(contact);
        cmd.Execute(mockConn.Object);

        Assert.True(contact.Subscribed);
    }

    [Fact]
    public void Execute_Success_CommitsTransaction()
    {
        var (mockConn, mockCmd, mockTx) = DbMockBuilder.Create();

        mockCmd.Setup(c => c.ExecuteNonQuery()).Returns(1);
        mockCmd
            .Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
            .Returns(DbMockBuilder.BuildIdReader(10));

        var cmd = new ContactOptinCommand(PendingContact());
        cmd.Execute(mockConn.Object);

        mockTx.Verify(t => t.Commit(), Times.Once);
    }

    [Fact]
    public void Execute_WhenExceptionThrown_RollsBackTransaction()
    {
        var (mockConn, mockCmd, mockTx) = DbMockBuilder.Create();

        mockCmd.Setup(c => c.ExecuteNonQuery()).Throws(new Exception("DB error"));

        var cmd = new ContactOptinCommand(PendingContact());
        var result = cmd.Execute(mockConn.Object);

        mockTx.Verify(t => t.Rollback(), Times.Once);
        Assert.False((bool)result.Data.Success);
    }
}
