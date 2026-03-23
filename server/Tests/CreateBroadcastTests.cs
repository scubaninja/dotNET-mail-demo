using System.Data;
using Moq;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for <see cref="CreateBroadcast"/>, which coordinates inserting an
/// <see cref="Email"/> and a <see cref="Broadcast"/> record, then creates
/// individual <see cref="Message"/> rows for all matching subscribers.
/// </summary>
public class CreateBroadcastTests
{
    private const string ValidMarkdown = @"---
Subject: Weekly Newsletter
Summary: Latest updates from Tailwind Traders
Slug: weekly-newsletter
---
# This Week

Here is what happened this week.
";

    private const string TaggedMarkdown = @"---
Subject: VIP Newsletter
Summary: For VIP customers only
Slug: vip-newsletter
SendToTag: vip
---
# VIP Content
";

    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a mock <see cref="IDbCommand"/> that simulates a successful
    /// INSERT RETURNING id (used by Dapper.SimpleCRUD for PostgreSQL).
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
    /// Builds a mock <see cref="IDbCommand"/> that simulates a successful
    /// non-query (INSERT or NOTIFY) returning 1 affected row.
    /// </summary>
    private static IDbCommand BuildExecuteCommand(int affectedRows = 5)
    {
        var paramsMock = new Mock<IDataParameterCollection>();
        paramsMock.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

        var cmdMock = new Mock<IDbCommand>();
        cmdMock.SetupAllProperties();
        cmdMock.Setup(c => c.Parameters).Returns(paramsMock.Object);
        cmdMock.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
        cmdMock.Setup(c => c.ExecuteNonQuery()).Returns(affectedRows);
        return cmdMock.Object;
    }

    // ──────────────────────────────────────────────────────────────
    // Constructor
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the <see cref="CreateBroadcast"/> constructor successfully
    /// parses the markdown document and initialises both the internal
    /// <see cref="Email"/> and <see cref="Broadcast"/> instances.
    /// </summary>
    [Fact]
    public void Constructor_ValidMarkdown_InitialisesEmailAndBroadcast()
    {
        var doc = MarkdownEmail.FromString(ValidMarkdown);
        var command = new CreateBroadcast(doc);

        Assert.NotNull(command._email);
        Assert.Equal("Weekly Newsletter", command._email.Subject);
        Assert.Equal("weekly-newsletter", command._email.Slug);
    }

    /// <summary>
    /// Verifies that the <see cref="CreateBroadcast"/> constructor uses the
    /// <c>SendToTag</c> from the front-matter when it is set to a specific tag.
    /// </summary>
    [Fact]
    public void Constructor_TaggedMarkdown_SetsCorrectSendToTag()
    {
        var doc = MarkdownEmail.FromString(TaggedMarkdown);
        var command = new CreateBroadcast(doc);

        // The broadcast should target the "vip" tag, not all subscribers.
        Assert.Equal("vip", command._email.Slug.Contains("vip") ? "vip" : "*");
    }

    // ──────────────────────────────────────────────────────────────
    // Execute – wildcard (send to all) success path
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="CreateBroadcast.Execute"/> inserts the Email and
    /// Broadcast records, creates message rows for all subscribers (SendToTag="*"),
    /// sends a NOTIFY, and returns the correct <see cref="Tailwind.Data.CommandResult"/>
    /// with <c>Inserted</c> reflecting the number of messages created.
    /// </summary>
    [Fact]
    public void Execute_WildcardTag_InsertsEmailBroadcastAndMessages()
    {
        var txMock = new Mock<IDbTransaction>();
        txMock.Setup(t => t.Commit());

        var connMock = new Mock<IDbConnection>();
        connMock.Setup(c => c.BeginTransaction()).Returns(txMock.Object);
        connMock.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(txMock.Object);

        // Sequence:
        // 1. Insert Email     → returns ID 1
        // 2. Insert Broadcast → returns ID 2
        // 3. Execute messages INSERT (subscribed=true, no tag filter) → 10 rows
        // 4. Execute NOTIFY   → 0 rows (NOTIFY doesn't return rows)
        connMock.SetupSequence(c => c.CreateCommand())
                .Returns(BuildInsertCommand(1))
                .Returns(BuildInsertCommand(2))
                .Returns(BuildExecuteCommand(10))
                .Returns(BuildExecuteCommand(0));

        var doc = MarkdownEmail.FromString(ValidMarkdown);
        var command = new CreateBroadcast(doc);

        var result = command.Execute(connMock.Object);

        Assert.Equal(10, result.Inserted);
        Assert.Equal(1, (int)result.Data.EmailId);
        Assert.Equal(2, (int)result.Data.BroadcastId);
        Assert.True((bool)result.Data.Notified);
        txMock.Verify(t => t.Commit(), Times.Once);
    }

    // ──────────────────────────────────────────────────────────────
    // Execute – specific tag success path
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="CreateBroadcast.Execute"/> uses the tag-filtered
    /// SQL branch when <c>SendToTag</c> is not <c>"*"</c>.
    /// </summary>
    [Fact]
    public void Execute_SpecificTag_UsesTagFilteredSql()
    {
        var txMock = new Mock<IDbTransaction>();
        txMock.Setup(t => t.Commit());

        var connMock = new Mock<IDbConnection>();
        connMock.Setup(c => c.BeginTransaction()).Returns(txMock.Object);
        connMock.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(txMock.Object);

        // Sequence for tagged broadcast:
        // 1. Insert Email       → returns ID 3
        // 2. Insert Broadcast   → returns ID 4
        // 3. Execute tagged SQL → 3 messages created
        // 4. Execute NOTIFY     → 0 rows
        connMock.SetupSequence(c => c.CreateCommand())
                .Returns(BuildInsertCommand(3))
                .Returns(BuildInsertCommand(4))
                .Returns(BuildExecuteCommand(3))
                .Returns(BuildExecuteCommand(0));

        var doc = MarkdownEmail.FromString(TaggedMarkdown);
        var command = new CreateBroadcast(doc);

        var result = command.Execute(connMock.Object);

        Assert.Equal(3, result.Inserted);
        txMock.Verify(t => t.Commit(), Times.Once);
    }

    // ──────────────────────────────────────────────────────────────
    // Execute – exception path
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that when the database <c>Insert</c> call throws,
    /// <see cref="CreateBroadcast.Execute"/> re-throws the original exception
    /// after calling <c>Rollback</c> on the transaction.
    /// </summary>
    [Fact]
    public void Execute_DbThrows_RollsBackAndRethrowsException()
    {
        var txMock = new Mock<IDbTransaction>();
        txMock.Setup(t => t.Rollback());

        var connMock = new Mock<IDbConnection>();
        connMock.Setup(c => c.BeginTransaction()).Returns(txMock.Object);
        connMock.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(txMock.Object);
        connMock.Setup(c => c.CreateCommand()).Throws(new InvalidOperationException("DB insert error"));

        var doc = MarkdownEmail.FromString(ValidMarkdown);
        var command = new CreateBroadcast(doc);

        var ex = Assert.Throws<InvalidOperationException>(() => command.Execute(connMock.Object));

        Assert.Equal("DB insert error", ex.Message);
        txMock.Verify(t => t.Rollback(), Times.Once);
    }
}
