using System.Data;
using Moq;
using Moq.Language.Flow;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Extended tests for <see cref="ContactSignupCommand"/>,
/// <see cref="ContactOptOutCommand"/>, and <see cref="ContactOptinCommand"/>
/// covering the success path where all database calls complete without error.
/// All database interactions are mocked via <see cref="IDbConnection"/>.
/// </summary>
public class CommandSuccessPathTests
{
    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a mock <see cref="IDbCommand"/> whose <c>ExecuteReader</c> returns a
    /// <see cref="DataTableReader"/> over the supplied contacts.
    /// </summary>
    private static IDbCommand BuildContactReaderCommand(IEnumerable<Contact> contacts)
    {
        var table = new DataTable();
        table.Columns.Add("id",         typeof(int));
        table.Columns.Add("name",       typeof(string));
        table.Columns.Add("email",      typeof(string));
        table.Columns.Add("subscribed", typeof(bool));
        table.Columns.Add("key",        typeof(string));
        table.Columns.Add("created_at", typeof(DateTimeOffset));

        foreach (var c in contacts)
            table.Rows.Add(c.ID ?? 0, c.Name, c.Email, c.Subscribed, c.Key, DateTimeOffset.UtcNow);

        var paramsMock = new Mock<IDataParameterCollection>();
        paramsMock.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

        var cmdMock = new Mock<IDbCommand>();
        cmdMock.SetupAllProperties();
        cmdMock.Setup(c => c.Parameters).Returns(paramsMock.Object);
        cmdMock.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
        cmdMock.Setup(c => c.ExecuteReader()).Returns(table.CreateDataReader());
        cmdMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(table.CreateDataReader());
        return cmdMock.Object;
    }

    /// <summary>
    /// Builds a mock <see cref="IDbCommand"/> whose <c>ExecuteScalar</c> returns the
    /// supplied value and whose <c>ExecuteReader</c> returns a single-row reader with
    /// that value in the first column.  This simulates Dapper.SimpleCRUD's INSERT…RETURNING
    /// behaviour for PostgreSQL, which uses <c>QuerySingleOrDefault</c> internally.
    /// </summary>
    private static IDbCommand BuildScalarCommand(object returnValue)
    {
        // Build a single-row DataTable so that both ExecuteScalar AND
        // ExecuteReader paths (used by Dapper.SimpleCRUD for PostgreSQL) work.
        var table = new DataTable();
        table.Columns.Add("id", returnValue.GetType());
        table.Rows.Add(returnValue);

        var paramsMock = new Mock<IDataParameterCollection>();
        paramsMock.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

        var cmdMock = new Mock<IDbCommand>();
        cmdMock.SetupAllProperties();
        cmdMock.Setup(c => c.Parameters).Returns(paramsMock.Object);
        cmdMock.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
        cmdMock.Setup(c => c.ExecuteScalar()).Returns(returnValue);
        // Dapper.SimpleCRUD for PostgreSQL uses QuerySingleOrDefault which reads a DataReader
        cmdMock.Setup(c => c.ExecuteReader()).Returns(() => table.CreateDataReader());
        cmdMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(() => table.CreateDataReader());
        return cmdMock.Object;
    }

    /// <summary>
    /// Builds a mock <see cref="IDbCommand"/> whose <c>ExecuteNonQuery</c> returns 1
    /// (simulating a successful UPDATE) and whose reader returns an empty result.
    /// </summary>
    private static IDbCommand BuildNonQueryCommand()
    {
        var paramsMock = new Mock<IDataParameterCollection>();
        paramsMock.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

        // Single-row table with affected row count so that both ExecuteNonQuery
        // and any QuerySingleOrDefault fallback path in Dapper succeeds.
        var table = new DataTable();
        table.Columns.Add("affected", typeof(int));
        table.Rows.Add(1);

        var cmdMock = new Mock<IDbCommand>();
        cmdMock.SetupAllProperties();
        cmdMock.Setup(c => c.Parameters).Returns(paramsMock.Object);
        cmdMock.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
        cmdMock.Setup(c => c.ExecuteNonQuery()).Returns(1);
        cmdMock.Setup(c => c.ExecuteScalar()).Returns(1);
        cmdMock.Setup(c => c.ExecuteReader()).Returns(() => table.CreateDataReader());
        cmdMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(() => table.CreateDataReader());
        return cmdMock.Object;
    }

    // ──────────────────────────────────────────────────────────────
    // ContactSignupCommand – success path
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that when no duplicate contact exists, <see cref="ContactSignupCommand.Execute"/>
    /// inserts the contact and activity, commits the transaction, and returns a success result
    /// with <c>Inserted = 1</c>.
    /// </summary>
    [Fact]
    public void ContactSignupCommand_NoDuplicate_CommitsAndReturnsSuccess()
    {
        var txMock = new Mock<IDbTransaction>();
        txMock.Setup(t => t.Commit());

        var connMock = new Mock<IDbConnection>();
        connMock.Setup(c => c.BeginTransaction()).Returns(txMock.Object);
        connMock.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(txMock.Object);

        // Sequence:
        // 1. GetList<Contact> for duplicate check → empty result
        // 2. Insert Contact → returns ID 1
        // 3. Insert Activity → returns ID 2
        connMock.SetupSequence(c => c.CreateCommand())
                .Returns(BuildContactReaderCommand(Enumerable.Empty<Contact>()))
                .Returns(BuildScalarCommand(1))
                .Returns(BuildScalarCommand(2));

        var contact = new Contact("Alice", "alice@test.com");
        var command = new ContactSignupCommand(contact);

        var result = command.Execute(connMock.Object);

        Assert.Equal(1, result.Inserted);
        Assert.True((bool)result.Data.Success);
        txMock.Verify(t => t.Commit(), Times.Once);
    }

    // ──────────────────────────────────────────────────────────────
    // ContactOptOutCommand – success path
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that when a contact with the given key exists,
    /// <see cref="ContactOptOutCommand.Execute"/> updates the subscription flag,
    /// inserts an activity, commits, and returns a success result with
    /// <c>Updated = 1</c>.
    /// </summary>
    [Fact]
    public void ContactOptOutCommand_ContactFound_CommitsAndReturnsSuccess()
    {
        var txMock = new Mock<IDbTransaction>();
        txMock.Setup(t => t.Commit());

        var existing = new Contact("Alice", "alice@test.com")
        {
            ID         = 1,
            Subscribed = true,
            Key        = "test-key"
        };

        var connMock = new Mock<IDbConnection>();
        connMock.Setup(c => c.BeginTransaction()).Returns(txMock.Object);
        connMock.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(txMock.Object);

        // Sequence:
        // 1. GetList<Contact> by key → returns the existing contact
        // 2. Update Contact → non-query (1 row affected)
        // 3. Insert Activity → scalar returns ID
        connMock.SetupSequence(c => c.CreateCommand())
                .Returns(BuildContactReaderCommand(new[] { existing }))
                .Returns(BuildNonQueryCommand())
                .Returns(BuildScalarCommand(10));

        var command = new ContactOptOutCommand("test-key");
        var result  = command.Execute(connMock.Object);

        Assert.Equal(1, result.Updated);
        Assert.True((bool)result.Data.Success);
        txMock.Verify(t => t.Commit(), Times.Once);
    }

    // ──────────────────────────────────────────────────────────────
    // ContactOptinCommand – success path
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="ContactOptinCommand.Execute"/> updates the contact's
    /// subscription flag to true, inserts an activity, commits, and returns
    /// <c>Updated = 1</c>.
    /// </summary>
    [Fact]
    public void ContactOptinCommand_ValidContact_CommitsAndReturnsSuccess()
    {
        var txMock = new Mock<IDbTransaction>();
        txMock.Setup(t => t.Commit());

        var connMock = new Mock<IDbConnection>();
        connMock.Setup(c => c.BeginTransaction()).Returns(txMock.Object);
        connMock.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(txMock.Object);

        // Sequence:
        // 1. Update Contact → non-query
        // 2. Insert Activity → scalar returns ID
        connMock.SetupSequence(c => c.CreateCommand())
                .Returns(BuildNonQueryCommand())
                .Returns(BuildScalarCommand(10));

        var contact = new Contact("Alice", "alice@test.com") { ID = 1, Subscribed = false };
        var command = new ContactOptinCommand(contact);

        var result = command.Execute(connMock.Object);

        Assert.Equal(1, result.Updated);
        Assert.True((bool)result.Data.Success);
        txMock.Verify(t => t.Commit(), Times.Once);
    }
}
