using System.Data;
using Moq;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Tests;

/// <summary>Tests for <see cref="ContactOptOutCommand"/>.</summary>
public class ContactOptOutCommandTests
{
    private static Contact ExistingContact(bool subscribed = true)
        => new Contact("Bob", "bob@example.com") { ID = 42, Subscribed = subscribed, Key = "some-key" };

    [Fact]
    public void Execute_ContactNotFound_ReturnsNotFoundMessage()
    {
        var (mockConn, mockCmd, _) = DbMockBuilder.Create();

        // GetList returns empty reader — contact does not exist
        mockCmd
            .Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
            .Returns(DbMockBuilder.BuildContactReader());

        var cmd = new ContactOptOutCommand("unknown-key");
        var result = cmd.Execute(mockConn.Object);

        Assert.Equal(0, result.Updated);
        Assert.False((bool)result.Data.Success);
        Assert.Equal("Contact not found", (string)result.Data.Message);
    }

    [Fact]
    public void Execute_ContactNotFound_DoesNotCommitTransaction()
    {
        var (mockConn, mockCmd, mockTx) = DbMockBuilder.Create();

        mockCmd
            .Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
            .Returns(DbMockBuilder.BuildContactReader());

        var cmd = new ContactOptOutCommand("unknown-key");
        cmd.Execute(mockConn.Object);

        mockTx.Verify(t => t.Commit(), Times.Never);
    }

    [Fact]
    public void Execute_ContactFound_ReturnsUpdatedCount()
    {
        var (mockConn, mockCmd, _) = DbMockBuilder.Create();

        // Sequence: GetList returns the contact, Update → ExecuteNonQuery, Insert activity → id
        mockCmd
            .SetupSequence(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
            .Returns(DbMockBuilder.BuildContactReader(new[] { ExistingContact() }))  // GetList
            .Returns(DbMockBuilder.BuildIdReader(99));                               // Insert activity

        mockCmd.Setup(c => c.ExecuteNonQuery()).Returns(1); // Update contact

        var cmd = new ContactOptOutCommand("some-key");
        var result = cmd.Execute(mockConn.Object);

        Assert.Equal(1, result.Updated);
        Assert.True((bool)result.Data.Success);
        Assert.Equal("Contact unsubscribed", (string)result.Data.Message);
    }

    [Fact]
    public void Execute_ContactFound_CommitsTransaction()
    {
        var (mockConn, mockCmd, mockTx) = DbMockBuilder.Create();

        mockCmd
            .SetupSequence(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
            .Returns(DbMockBuilder.BuildContactReader(new[] { ExistingContact() }))
            .Returns(DbMockBuilder.BuildIdReader(99));

        mockCmd.Setup(c => c.ExecuteNonQuery()).Returns(1);

        var cmd = new ContactOptOutCommand("some-key");
        cmd.Execute(mockConn.Object);

        mockTx.Verify(t => t.Commit(), Times.Once);
    }

    [Fact]
    public void Execute_WhenExceptionThrown_RollsBackTransaction()
    {
        var (mockConn, mockCmd, mockTx) = DbMockBuilder.Create();

        // GetList succeeds, Update throws
        mockCmd
            .Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
            .Returns(DbMockBuilder.BuildContactReader(new[] { ExistingContact() }));
        mockCmd.Setup(c => c.ExecuteNonQuery()).Throws(new Exception("DB error"));

        var cmd = new ContactOptOutCommand("some-key");
        var result = cmd.Execute(mockConn.Object);

        mockTx.Verify(t => t.Rollback(), Times.Once);
        Assert.False((bool)result.Data.Success);
    }
}
