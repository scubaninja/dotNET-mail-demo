using System.Data;
using Moq;
using Tailwind.Data;
using Tailwind.Mail.Models;
using Tailwind.Mail.Services;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for <see cref="BackgroundSend"/>, the hosted background service that
/// periodically queries for pending messages and dispatches them via the
/// configured <see cref="IEmailSender"/>.
/// </summary>
public class BackgroundSendTests
{
    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a mock <see cref="IDbConnection"/> whose <c>CreateCommand</c>
    /// returns a command with an empty <see cref="DataTableReader"/>
    /// (simulating a query with no pending messages).
    /// </summary>
    private static Mock<IDbConnection> BuildEmptyQueryConnection()
    {
        var table = new DataTable();
        table.Columns.Add("id",        typeof(int));
        table.Columns.Add("subject",   typeof(string));
        table.Columns.Add("status",    typeof(string));
        table.Columns.Add("slug",      typeof(string));
        table.Columns.Add("html",      typeof(string));
        table.Columns.Add("send_at",   typeof(DateTimeOffset));
        table.Columns.Add("send_to",   typeof(string));
        table.Columns.Add("send_from", typeof(string));

        var paramsMock = new Mock<IDataParameterCollection>();
        paramsMock.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

        var cmdMock = new Mock<IDbCommand>();
        cmdMock.SetupAllProperties();
        cmdMock.Setup(c => c.Parameters).Returns(paramsMock.Object);
        cmdMock.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
        // Use a factory so each call returns a fresh reader
        cmdMock.Setup(c => c.ExecuteReader()).Returns(() => table.CreateDataReader());
        cmdMock.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
               .Returns(() => table.CreateDataReader());

        var connMock = new Mock<IDbConnection>();
        connMock.Setup(c => c.CreateCommand()).Returns(() => cmdMock.Object);
        return connMock;
    }

    // ──────────────────────────────────────────────────────────────
    // Constructor
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the <see cref="BackgroundSend"/> constructor accepts
    /// injected <see cref="IEmailSender"/> and <see cref="IDb"/> dependencies
    /// without throwing.
    /// </summary>
    [Fact]
    public void Constructor_WithDependencies_DoesNotThrow()
    {
        var senderMock = new Mock<IEmailSender>();
        var dbMock = new Mock<IDb>();
        dbMock.Setup(d => d.Connect()).Returns(BuildEmptyQueryConnection().Object);

        var service = new BackgroundSend(senderMock.Object, dbMock.Object);

        Assert.NotNull(service);
    }

    // ──────────────────────────────────────────────────────────────
    // ExecuteAsync – immediate cancellation
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the <see cref="BackgroundSend"/> service starts and stops
    /// cleanly when the cancellation token is already cancelled before
    /// <see cref="BackgroundSend.ExecuteAsync"/> is entered, so the while loop
    /// never runs.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ImmediatelyCancelled_StartsAndStopsCleanly()
    {
        var senderMock = new Mock<IEmailSender>();
        var dbMock = new Mock<IDb>();
        dbMock.Setup(d => d.Connect()).Returns(BuildEmptyQueryConnection().Object);

        using var cts = new CancellationTokenSource();
        var service = new BackgroundSend(senderMock.Object, dbMock.Object);

        // Cancel before start so the while loop condition is false immediately.
        cts.Cancel();

        await service.StartAsync(cts.Token);
        await service.StopAsync(CancellationToken.None);

        // With immediate cancellation the sender is never invoked.
        senderMock.Verify(s => s.SendBulk(It.IsAny<IEnumerable<Message>>()), Times.Never);
    }
}
