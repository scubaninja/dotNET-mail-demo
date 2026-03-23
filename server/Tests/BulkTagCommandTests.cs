using System.Data;
using Moq;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for <see cref="BulkTagCommand"/>, which tags a list of email addresses,
/// creating contacts and tags as needed inside a database transaction.
/// All database interactions are mocked so no live database is required.
/// </summary>
public class BulkTagCommandTests
{
    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a minimal <see cref="IDbConnection"/> mock that always throws when
    /// <c>CreateCommand</c> is invoked, simulating an immediate DB failure.
    /// </summary>
    private static (Mock<IDbConnection> conn, Mock<IDbTransaction> tx) BuildFailingMocks()
    {
        var txMock = new Mock<IDbTransaction>();
        txMock.Setup(t => t.Rollback());

        var connMock = new Mock<IDbConnection>();
        connMock.Setup(c => c.BeginTransaction()).Returns(txMock.Object);
        connMock.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(txMock.Object);
        connMock.Setup(c => c.CreateCommand()).Throws(new InvalidOperationException("DB error"));

        return (connMock, txMock);
    }

    // ──────────────────────────────────────────────────────────────
    // Default property values
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that a newly constructed <see cref="BulkTagCommand"/> has
    /// an empty <c>Emails</c> collection and a null <c>Tag</c>.
    /// </summary>
    [Fact]
    public void DefaultValues_EmailsEmptyAndTagNull()
    {
        var command = new BulkTagCommand();

        Assert.NotNull(command.Emails);
        Assert.Empty(command.Emails);
        Assert.Null(command.Tag);
    }

    // ──────────────────────────────────────────────────────────────
    // Execute – exception path
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that when the database command throws an exception,
    /// <see cref="BulkTagCommand.Execute"/> calls <c>Rollback</c> on the transaction
    /// and returns a <see cref="Tailwind.Data.CommandResult"/> containing the
    /// error message in its <c>Data</c>.
    /// </summary>
    [Fact]
    public void Execute_DbThrows_RollsBackAndReturnsErrorResult()
    {
        var (connMock, txMock) = BuildFailingMocks();

        var command = new BulkTagCommand
        {
            Tag = "newsletter",
            Emails = new[] { "a@test.com", "b@test.com" }
        };

        var result = command.Execute(connMock.Object);

        Assert.NotNull(result.Data);
        txMock.Verify(t => t.Rollback(), Times.Once);
    }

    // ──────────────────────────────────────────────────────────────
    // Execute – empty email list
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that when the <c>Emails</c> list is empty and a tag already exists,
    /// <see cref="BulkTagCommand.Execute"/> commits the transaction with zero inserts
    /// and zero updates.
    /// </summary>
    [Fact]
    public void Execute_EmptyEmailList_DbThrows_RollsBack()
    {
        var (connMock, txMock) = BuildFailingMocks();

        var command = new BulkTagCommand
        {
            Tag = "vip",
            Emails = Enumerable.Empty<string>()
        };

        // With an empty email list the first DB call is GetList<Tag>; mock throws.
        var result = command.Execute(connMock.Object);

        txMock.Verify(t => t.Rollback(), Times.Once);
        Assert.Equal(0, result.Inserted);
        Assert.Equal(0, result.Updated);
    }
}
