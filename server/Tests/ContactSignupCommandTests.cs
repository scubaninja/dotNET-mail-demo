using System.Data;
using Moq;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Tests;

/// <summary>Tests for <see cref="ContactSignupCommand"/>.</summary>
public class ContactSignupCommandTests
{
    private static Contact NewContact(string name = "Alice", string email = "alice@example.com")
        => new Contact(name, email);

    [Fact]
    public void Execute_WhenContactAlreadyExists_ReturnsUserExistsMessage()
    {
        var (mockConn, mockCmd, _) = DbMockBuilder.Create();

        // GetList returns a reader containing an existing contact
        var existing = new Contact("Alice", "alice@example.com") { ID = 1, Subscribed = true };
        mockCmd
            .Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
            .Returns(DbMockBuilder.BuildContactReader(new[] { existing }));

        var cmd = new ContactSignupCommand(NewContact());
        var result = cmd.Execute(mockConn.Object);

        Assert.Equal(0, result.Inserted);
        Assert.False((bool)result.Data.Success);
        Assert.Equal("User exists", (string)result.Data.Message);
    }

    [Fact]
    public void Execute_WhenContactAlreadyExists_DoesNotCommitTransaction()
    {
        var (mockConn, mockCmd, mockTx) = DbMockBuilder.Create();

        var existing = new Contact("Alice", "alice@example.com") { ID = 1, Subscribed = true };
        mockCmd
            .Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
            .Returns(DbMockBuilder.BuildContactReader(new[] { existing }));

        var cmd = new ContactSignupCommand(NewContact());
        cmd.Execute(mockConn.Object);

        mockTx.Verify(t => t.Commit(), Times.Never);
    }

    [Fact]
    public void Execute_NewContact_ReturnsInsertedCount()
    {
        var (mockConn, mockCmd, _) = DbMockBuilder.Create();

        // Sequence: GetList (empty), Insert contact (id=1), Insert activity (id=2)
        mockCmd
            .SetupSequence(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
            .Returns(DbMockBuilder.BuildContactReader())       // GetList → no match
            .Returns(DbMockBuilder.BuildIdReader(1))           // Insert contact → id 1
            .Returns(DbMockBuilder.BuildIdReader(2));          // Insert activity → id 2

        var cmd = new ContactSignupCommand(NewContact());
        var result = cmd.Execute(mockConn.Object);

        Assert.Equal(1, result.Inserted);
        Assert.True((bool)result.Data.Success);
    }

    [Fact]
    public void Execute_NewContact_CommitsTransaction()
    {
        var (mockConn, mockCmd, mockTx) = DbMockBuilder.Create();

        mockCmd
            .SetupSequence(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
            .Returns(DbMockBuilder.BuildContactReader())
            .Returns(DbMockBuilder.BuildIdReader(1))
            .Returns(DbMockBuilder.BuildIdReader(2));

        var cmd = new ContactSignupCommand(NewContact());
        cmd.Execute(mockConn.Object);

        mockTx.Verify(t => t.Commit(), Times.Once);
    }

    [Fact]
    public void Execute_WhenExceptionThrown_RollsBackTransaction()
    {
        var (mockConn, mockCmd, mockTx) = DbMockBuilder.Create();

        // GetList returns empty (new contact path), but Insert throws
        mockCmd
            .SetupSequence(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
            .Returns(DbMockBuilder.BuildContactReader())
            .Throws(new Exception("DB error"));

        var cmd = new ContactSignupCommand(NewContact());
        var result = cmd.Execute(mockConn.Object);

        mockTx.Verify(t => t.Rollback(), Times.Once);
        Assert.False((bool)result.Data.Success);
    }
}
