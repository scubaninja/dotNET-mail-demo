using System.Data;
using Moq;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for <see cref="ContactOptinCommand"/>, which re-subscribes a contact
/// and records an activity log entry inside a database transaction.
/// All database interactions are mocked so no live database is required.
/// </summary>
public class ContactOptinCommandTests
{
    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a minimal <see cref="IDbConnection"/> mock that supports
    /// <see cref="IDbConnection.BeginTransaction()"/>.
    /// </summary>
    private static (Mock<IDbConnection> conn, Mock<IDbTransaction> tx) BuildMocks()
    {
        var txMock = new Mock<IDbTransaction>();
        txMock.Setup(t => t.Commit());
        txMock.Setup(t => t.Rollback());

        var connMock = new Mock<IDbConnection>();
        connMock.Setup(c => c.BeginTransaction()).Returns(txMock.Object);
        connMock.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(txMock.Object);

        return (connMock, txMock);
    }

    // ──────────────────────────────────────────────────────────────
    // Constructor
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the <see cref="ContactOptinCommand"/> constructor stores
    /// the supplied <see cref="Contact"/> on the <c>Contact</c> property.
    /// </summary>
    [Fact]
    public void Constructor_SetsContactProperty()
    {
        var contact = new Contact("Alice", "alice@test.com");
        var command = new ContactOptinCommand(contact);

        Assert.Equal(contact, command.Contact);
    }

    // ──────────────────────────────────────────────────────────────
    // Execute – exception path
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that when the database command throws an exception,
    /// <see cref="ContactOptinCommand.Execute"/> calls <c>Rollback</c> on the
    /// transaction and returns a failure <see cref="Tailwind.Data.CommandResult"/>
    /// whose <c>Data.Success</c> is <c>false</c>.
    /// </summary>
    [Fact]
    public void Execute_DbThrows_RollsBackAndReturnsFailure()
    {
        var txMock = new Mock<IDbTransaction>();
        txMock.Setup(t => t.Rollback());

        var connMock = new Mock<IDbConnection>();
        connMock.Setup(c => c.BeginTransaction()).Returns(txMock.Object);
        connMock.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(txMock.Object);
        connMock.Setup(c => c.CreateCommand()).Throws(new InvalidOperationException("DB error"));

        var contact = new Contact("Alice", "alice@test.com") { ID = 1 };
        var command = new ContactOptinCommand(contact);

        var result = command.Execute(connMock.Object);

        Assert.Equal(0, result.Updated);
        Assert.False((bool)result.Data.Success);
        txMock.Verify(t => t.Rollback(), Times.Once);
    }

    // ──────────────────────────────────────────────────────────────
    // Execute – contact subscribed flag set
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="ContactOptinCommand.Execute"/> sets
    /// <c>Contact.Subscribed</c> to <c>true</c> before attempting the database
    /// update, regardless of the contact's prior subscription state.
    /// </summary>
    [Fact]
    public void Execute_SetsContactSubscribedToTrue()
    {
        var (connMock, _) = BuildMocks();
        // Cause the Update call to fail early so we can inspect the contact state.
        connMock.Setup(c => c.CreateCommand()).Throws(new InvalidOperationException("DB error"));

        var contact = new Contact("Alice", "alice@test.com") { ID = 1, Subscribed = false };
        var command = new ContactOptinCommand(contact);

        command.Execute(connMock.Object);

        // Despite the DB error the in-memory object should already be marked subscribed.
        Assert.True(command.Contact.Subscribed);
    }
}
