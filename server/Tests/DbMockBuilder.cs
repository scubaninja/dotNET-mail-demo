using System.Data;
using Moq;
using Tailwind.Mail.Models;

namespace Tailwind.Tests;

/// <summary>
/// Provides shared mock-database infrastructure for command tests that
/// require a mocked <see cref="IDbConnection"/>.
/// </summary>
internal static class DbMockBuilder
{
    /// <summary>
    /// Creates a fully configured mock triple: connection, command, and transaction.
    /// The command supports sequential <c>ExecuteReader</c> calls via
    /// <c>SetupSequence</c> on the returned <c>Cmd</c> mock.
    /// </summary>
    public static (Mock<IDbConnection> Conn, Mock<IDbCommand> Cmd, Mock<IDbTransaction> Tx) Create()
    {
        var mockConn = new Mock<IDbConnection>();
        var mockCmd  = new Mock<IDbCommand>();
        var mockTx   = new Mock<IDbTransaction>();
        var mockParams = new Mock<IDataParameterCollection>();

        mockCmd.SetupAllProperties();
        mockCmd.Setup(c => c.Parameters).Returns(mockParams.Object);
        mockCmd.Setup(c => c.CreateParameter()).Returns(() =>
        {
            var p = new Mock<IDbDataParameter>();
            p.SetupAllProperties();
            return p.Object;
        });
        mockParams.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

        mockConn.Setup(c => c.State).Returns(ConnectionState.Open);
        mockConn.Setup(c => c.CreateCommand()).Returns(mockCmd.Object);
        mockConn.Setup(c => c.BeginTransaction()).Returns(mockTx.Object);
        mockConn.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(mockTx.Object);

        return (mockConn, mockCmd, mockTx);
    }

    /// <summary>
    /// Builds an <see cref="IDataReader"/> that returns zero or more <see cref="Contact"/> rows.
    /// Column names use snake_case to match the Dapper <c>CustomResolver</c>.
    /// </summary>
    public static IDataReader BuildContactReader(IEnumerable<Contact>? contacts = null)
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(int));
        table.Columns.Add("name", typeof(string));
        table.Columns.Add("email", typeof(string));
        table.Columns.Add("subscribed", typeof(bool));
        table.Columns.Add("key", typeof(string));
        table.Columns.Add("created_at", typeof(DateTime));

        foreach (var c in contacts ?? Enumerable.Empty<Contact>())
            table.Rows.Add(c.ID ?? 0, c.Name ?? "", c.Email ?? "", c.Subscribed, c.Key, DateTime.UtcNow);

        return table.CreateDataReader();
    }

    /// <summary>
    /// Builds an <see cref="IDataReader"/> that returns a single row with an <c>id</c> column,
    /// as expected by Dapper.SimpleCRUD's PostgreSQL RETURNING-based INSERT.
    /// </summary>
    public static IDataReader BuildIdReader(int id = 1)
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(int));
        table.Rows.Add(id);
        return table.CreateDataReader();
    }
}
