using System.Data;
using Moq;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Extended tests for <see cref="BulkTagCommand"/> covering the full success path
/// where a new tag is created and contacts are upserted and tagged.
/// All database interactions are mocked so no live database is required.
/// </summary>
public class BulkTagCommandSuccessTests
{
    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a mock <see cref="IDbCommand"/> whose reader returns an empty result
    /// (used to simulate GetList returning no rows).
    /// </summary>
    private static IDbCommand BuildEmptyReaderCommand()
    {
        var table = new DataTable();
        table.Columns.Add("id",          typeof(int));
        table.Columns.Add("name",        typeof(string));
        table.Columns.Add("email",       typeof(string));
        table.Columns.Add("subscribed",  typeof(bool));
        table.Columns.Add("key",         typeof(string));
        table.Columns.Add("slug",        typeof(string));
        table.Columns.Add("description", typeof(string));
        table.Columns.Add("created_at",  typeof(DateTimeOffset));

        var paramsMock = new Mock<IDataParameterCollection>();
        paramsMock.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

        var cmdMock = new Mock<IDbCommand>();
        cmdMock.SetupAllProperties();
        cmdMock.Setup(c => c.Parameters).Returns(paramsMock.Object);
        cmdMock.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
        cmdMock.Setup(c => c.ExecuteReader()).Returns(() => table.CreateDataReader());
        cmdMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(() => table.CreateDataReader());
        return cmdMock.Object;
    }

    /// <summary>
    /// Builds a mock <see cref="IDbCommand"/> that simulates a successful INSERT
    /// RETURNING id for Dapper.SimpleCRUD with PostgreSQL dialect.
    /// </summary>
    private static IDbCommand BuildInsertCommand(int returnedId)
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(int));
        table.Rows.Add(returnedId);

        var paramsMock = new Mock<IDataParameterCollection>();
        paramsMock.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

        var cmdMock = new Mock<IDbCommand>();
        cmdMock.SetupAllProperties();
        cmdMock.Setup(c => c.Parameters).Returns(paramsMock.Object);
        cmdMock.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
        cmdMock.Setup(c => c.ExecuteScalar()).Returns(returnedId);
        cmdMock.Setup(c => c.ExecuteReader()).Returns(() => table.CreateDataReader());
        cmdMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(() => table.CreateDataReader());
        return cmdMock.Object;
    }

    /// <summary>
    /// Builds a mock <see cref="IDbCommand"/> simulating a successful
    /// INSERT/UPDATE non-query (execute).
    /// </summary>
    private static IDbCommand BuildNonQueryCommand()
    {
        var paramsMock = new Mock<IDataParameterCollection>();
        paramsMock.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

        var cmdMock = new Mock<IDbCommand>();
        cmdMock.SetupAllProperties();
        cmdMock.Setup(c => c.Parameters).Returns(paramsMock.Object);
        cmdMock.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
        cmdMock.Setup(c => c.ExecuteNonQuery()).Returns(1);
        return cmdMock.Object;
    }

    // ──────────────────────────────────────────────────────────────
    // Success path – new tag, new contact
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that when neither the tag nor the contact exists in the database,
    /// <see cref="BulkTagCommand.Execute"/> creates both records, links them, and
    /// commits the transaction with <c>Inserted = 1</c>.
    /// </summary>
    [Fact]
    public void Execute_NewTagNewContact_InsertsAndCommits()
    {
        var txMock = new Mock<IDbTransaction>();
        txMock.Setup(t => t.Commit());

        var connMock = new Mock<IDbConnection>();
        connMock.Setup(c => c.BeginTransaction()).Returns(txMock.Object);
        connMock.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(txMock.Object);

        // Sequence of CreateCommand calls:
        // 1. GetList<Tag> → empty reader (tag doesn't exist)
        // 2. Insert new Tag → returns ID 10
        // 3. GetList<Contact> → empty reader (contact doesn't exist)
        // 4. Insert new Contact → returns ID 100
        // 5. Execute tagged INSERT → non-query
        connMock.SetupSequence(c => c.CreateCommand())
                .Returns(BuildEmptyReaderCommand())   // GetList<Tag> empty
                .Returns(BuildInsertCommand(10))       // Insert Tag
                .Returns(BuildEmptyReaderCommand())   // GetList<Contact> empty
                .Returns(BuildInsertCommand(100))      // Insert Contact
                .Returns(BuildNonQueryCommand());      // tagged INSERT

        var command = new BulkTagCommand
        {
            Tag    = "newsletter",
            Emails = new[] { "alice@test.com" }
        };

        var result = command.Execute(connMock.Object);

        Assert.Equal(1, result.Inserted);
        Assert.Equal(0, result.Updated);
        txMock.Verify(t => t.Commit(), Times.Once);
    }

    // ──────────────────────────────────────────────────────────────
    // Success path – existing tag, existing contact
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that when the tag already exists and the contact already exists,
    /// <see cref="BulkTagCommand.Execute"/> does not insert either record and
    /// returns <c>Updated = 1</c> for the matched contact.
    /// </summary>
    [Fact]
    public void Execute_ExistingTagExistingContact_UpdatesAndCommits()
    {
        var txMock = new Mock<IDbTransaction>();
        txMock.Setup(t => t.Commit());

        // Build a reader for an existing tag
        var tagTable = new DataTable();
        tagTable.Columns.Add("id",   typeof(int));
        tagTable.Columns.Add("name", typeof(string));
        tagTable.Columns.Add("slug", typeof(string));
        tagTable.Rows.Add(10, "newsletter", "newsletter");

        var tagParamsMock = new Mock<IDataParameterCollection>();
        tagParamsMock.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

        var tagCmdMock = new Mock<IDbCommand>();
        tagCmdMock.SetupAllProperties();
        tagCmdMock.Setup(c => c.Parameters).Returns(tagParamsMock.Object);
        tagCmdMock.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
        tagCmdMock.Setup(c => c.ExecuteReader()).Returns(() => tagTable.CreateDataReader());
        tagCmdMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
                  .Returns(() => tagTable.CreateDataReader());

        // Build a reader for an existing contact
        var contactTable = new DataTable();
        contactTable.Columns.Add("id",         typeof(int));
        contactTable.Columns.Add("name",        typeof(string));
        contactTable.Columns.Add("email",       typeof(string));
        contactTable.Columns.Add("subscribed",  typeof(bool));
        contactTable.Columns.Add("key",         typeof(string));
        contactTable.Columns.Add("created_at",  typeof(DateTimeOffset));
        contactTable.Rows.Add(100, "Alice", "alice@test.com", true, Guid.NewGuid().ToString(), DateTimeOffset.UtcNow);

        var contactParamsMock = new Mock<IDataParameterCollection>();
        contactParamsMock.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

        var contactCmdMock = new Mock<IDbCommand>();
        contactCmdMock.SetupAllProperties();
        contactCmdMock.Setup(c => c.Parameters).Returns(contactParamsMock.Object);
        contactCmdMock.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
        contactCmdMock.Setup(c => c.ExecuteReader()).Returns(() => contactTable.CreateDataReader());
        contactCmdMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
                      .Returns(() => contactTable.CreateDataReader());

        var connMock = new Mock<IDbConnection>();
        connMock.Setup(c => c.BeginTransaction()).Returns(txMock.Object);
        connMock.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(txMock.Object);

        // Sequence:
        // 1. GetList<Tag>    → returns existing tag
        // 2. GetList<Contact> → returns existing contact (no tx passed in BulkTagCommand)
        // 3. Execute tagged INSERT (on-conflict-do-nothing) → non-query
        connMock.SetupSequence(c => c.CreateCommand())
                .Returns(tagCmdMock.Object)
                .Returns(contactCmdMock.Object)
                .Returns(BuildNonQueryCommand());

        var command = new BulkTagCommand
        {
            Tag    = "newsletter",
            Emails = new[] { "alice@test.com" }
        };

        var result = command.Execute(connMock.Object);

        Assert.Equal(0, result.Inserted);
        Assert.Equal(1, result.Updated);
        txMock.Verify(t => t.Commit(), Times.Once);
    }
}
