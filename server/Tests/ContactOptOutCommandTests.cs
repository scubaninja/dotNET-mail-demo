using System.Data;
using Moq;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for <see cref="ContactOptOutCommand"/>, which unsubscribes a contact
/// identified by their unique key and records an activity log entry.
/// All database interactions are mocked so no live database is required.
/// </summary>
public class ContactOptOutCommandTests
{
    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a minimal <see cref="IDbConnection"/> mock that supports
    /// <see cref="IDbConnection.BeginTransaction()"/> and commits/rolls back on demand.
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

    /// <summary>
    /// Sets up the mock connection's <c>CreateCommand</c> so that Dapper's
    /// <c>GetList</c> returns the provided contacts.
    /// </summary>
    private static void SetupGetListReturns(Mock<IDbConnection> connMock, IEnumerable<Contact> contacts)
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(int));
        table.Columns.Add("name", typeof(string));
        table.Columns.Add("email", typeof(string));
        table.Columns.Add("subscribed", typeof(bool));
        table.Columns.Add("key", typeof(string));
        table.Columns.Add("created_at", typeof(DateTimeOffset));

        foreach (var c in contacts)
        {
            table.Rows.Add(c.ID ?? 0, c.Name, c.Email, c.Subscribed, c.Key, DateTimeOffset.UtcNow);
        }

        var paramsMock = new Mock<IDataParameterCollection>();
        paramsMock.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

        var cmdMock = new Mock<IDbCommand>();
        cmdMock.SetupAllProperties();
        cmdMock.Setup(c => c.Parameters).Returns(paramsMock.Object);
        cmdMock.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
        cmdMock.Setup(c => c.ExecuteReader()).Returns(table.CreateDataReader());
        cmdMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(table.CreateDataReader());

        connMock.Setup(c => c.CreateCommand()).Returns(cmdMock.Object);
    }

    // ──────────────────────────────────────────────────────────────
    // Constructor
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the <see cref="ContactOptOutCommand"/> constructor stores
    /// the supplied key on the <c>Key</c> property.
    /// </summary>
    [Fact]
    public void Constructor_SetsKeyProperty()
    {
        var command = new ContactOptOutCommand("some-key");

        Assert.Equal("some-key", command.Key);
    }

    // ──────────────────────────────────────────────────────────────
    // Execute – contact not found
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that when no contact with the given key exists,
    /// <see cref="ContactOptOutCommand.Execute"/> returns a failure result with a
    /// "Contact not found" message and does not increment <c>Updated</c>.
    /// </summary>
    [Fact]
    public void Execute_ContactNotFound_ReturnsFailureResult()
    {
        var (connMock, _) = BuildMocks();
        SetupGetListReturns(connMock, Enumerable.Empty<Contact>());

        var command = new ContactOptOutCommand("missing-key");
        var result = command.Execute(connMock.Object);

        Assert.Equal(0, result.Updated);
        Assert.False((bool)result.Data.Success);
        Assert.Equal("Contact not found", (string)result.Data.Message);
    }

    // ──────────────────────────────────────────────────────────────
    // Execute – exception path
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that when the database command throws an exception,
    /// <see cref="ContactOptOutCommand.Execute"/> calls <c>Rollback</c> on the
    /// transaction and returns a failure result containing the exception message.
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

        var command = new ContactOptOutCommand("error-key");
        var result = command.Execute(connMock.Object);

        Assert.False((bool)result.Data.Success);
        txMock.Verify(t => t.Rollback(), Times.Once);
    }
}
