using System.Data;
using Moq;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;
using Tailwind.Data;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for <see cref="ContactSignupCommand"/>, which inserts a new contact
/// record and a corresponding activity entry inside a database transaction.
/// All database interactions are mocked so no live database is required.
/// </summary>
public class ContactSignupCommandTests
{
    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a minimal mock <see cref="IDbConnection"/> that supports
    /// <see cref="IDbConnection.BeginTransaction()"/> and returns the provided
    /// <see cref="IDbTransaction"/> mock.
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
    /// Verifies that the <see cref="ContactSignupCommand"/> constructor stores
    /// the supplied <see cref="Contact"/> on the <c>Contact</c> property.
    /// </summary>
    [Fact]
    public void Constructor_SetsContactProperty()
    {
        var contact = new Contact("Alice", "alice@test.com");
        var command = new ContactSignupCommand(contact);

        Assert.Equal(contact, command.Contact);
    }

    // ──────────────────────────────────────────────────────────────
    // Execute – duplicate contact
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that when Dapper's <c>GetList</c> returns an existing contact with
    /// the same email, <see cref="ContactSignupCommand.Execute"/> returns a
    /// <see cref="CommandResult"/> whose <c>Data.Success</c> is <c>false</c>
    /// and <c>Inserted</c> remains 0.
    /// </summary>
    [Fact]
    public void Execute_DuplicateEmail_ReturnsFailureResult()
    {
        var (connMock, _) = BuildMocks();

        // Simulate GetList returning an existing contact by setting up CreateCommand
        // to return a command that produces a non-empty reader.
        var existing = new Contact("Alice", "alice@test.com") { ID = 1 };
        SetupGetListReturns(connMock, new List<Contact> { existing });

        var contact = new Contact("Alice2", "alice@test.com");
        var command = new ContactSignupCommand(contact);

        var result = command.Execute(connMock.Object);

        Assert.Equal(0, result.Inserted);
        Assert.False((bool)result.Data.Success);
        Assert.Equal("User exists", (string)result.Data.Message);
    }

    // ──────────────────────────────────────────────────────────────
    // Execute – exception inside try/catch (Insert fails)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that when the database Insert call inside the try block throws,
    /// <see cref="ContactSignupCommand.Execute"/> calls <c>Rollback</c> on the
    /// transaction and returns a failure result containing the exception message.
    /// The GetList (duplicate-check) is set up to succeed with an empty result so that
    /// execution enters the try block before the simulated failure.
    /// </summary>
    [Fact]
    public void Execute_InsertDbThrows_RollsBackAndReturnsFailure()
    {
        var txMock = new Mock<IDbTransaction>();
        txMock.Setup(t => t.Rollback());

        var connMock = new Mock<IDbConnection>();
        connMock.Setup(c => c.BeginTransaction()).Returns(txMock.Object);
        connMock.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(txMock.Object);

        // First call to CreateCommand is for GetList – return an empty reader so the
        // duplicate-check passes.  Subsequent calls (Insert) throw to simulate a DB error.
        var emptyCmd = BuildEmptyReaderCommand();
        connMock.SetupSequence(c => c.CreateCommand())
                .Returns(emptyCmd)
                .Throws(new InvalidOperationException("DB insert error"));

        var contact = new Contact("Alice", "alice@test.com");
        var command = new ContactSignupCommand(contact);

        var result = command.Execute(connMock.Object);

        Assert.Equal(0, result.Inserted);
        Assert.False((bool)result.Data.Success);
        txMock.Verify(t => t.Rollback(), Times.Once);
    }

    // ──────────────────────────────────────────────────────────────
    // Private helpers
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Configures the mock <see cref="IDbConnection"/> so that <c>CreateCommand</c>
    /// returns a command whose <c>ExecuteReader</c> produces a reader over the given
    /// contacts, allowing Dapper's <c>GetList</c> to return them.
    /// </summary>
    private static void SetupGetListReturns(Mock<IDbConnection> connMock, IEnumerable<Contact> contacts)
    {
        connMock.Setup(c => c.CreateCommand()).Returns(BuildReaderCommand(contacts));
    }

    /// <summary>
    /// Builds a mock <see cref="IDbCommand"/> whose reader returns an empty contact table.
    /// </summary>
    private static IDbCommand BuildEmptyReaderCommand() => BuildReaderCommand(Enumerable.Empty<Contact>());

    /// <summary>
    /// Builds a mock <see cref="IDbCommand"/> whose <c>ExecuteReader</c> returns a
    /// <see cref="DataTableReader"/> over the supplied contacts.
    /// </summary>
    private static IDbCommand BuildReaderCommand(IEnumerable<Contact> contacts)
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

        return cmdMock.Object;
    }
}
