using System.Data;
using Dapper;
using Moq;
using Tailwind.Data;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

public class DeleteContactCommandTests
{
  static DeleteContactCommandTests()
  {
    Dapper.SimpleCRUD.SetDialect(Dapper.SimpleCRUD.Dialect.PostgreSQL);
    SimpleCRUD.SetColumnNameResolver(new CustomResolver());
  }

  [Fact]
  public void Execute_ContactNotFound_ReturnsDeletedZero()
  {
    // Arrange
    var (mockConn, _) = BuildMockConnection(contactData: null);
    var cmd = new DeleteContactCommand(999);

    // Act
    var result = cmd.Execute(mockConn.Object);

    // Assert
    Assert.Equal(0, result.Deleted);
  }

  [Fact]
  public void Execute_ContactExists_CommitsTransactionAndReturnsDeletedOne()
  {
    // Arrange
    var contact = new Contact { ID = 1, Name = "Test User", Email = "test@example.com" };
    var (mockConn, mockTx) = BuildMockConnection(contactData: contact);
    var cmd = new DeleteContactCommand(1);

    // Act
    var result = cmd.Execute(mockConn.Object);

    // Assert
    Assert.Equal(1, result.Deleted);
    mockTx.Verify(t => t.Commit(), Times.Once);
  }

  [Fact]
  public void Execute_DatabaseThrowsException_RollsBackAndReturnsDeletedNegativeOne()
  {
    // Arrange
    var (mockConn, mockTx) = BuildMockConnection(throws: true);
    var cmd = new DeleteContactCommand(1);

    // Act
    var result = cmd.Execute(mockConn.Object);

    // Assert
    Assert.Equal(-1, result.Deleted);
    mockTx.Verify(t => t.Rollback(), Times.Once);
  }

  // --- helpers ---

  private static (Mock<IDbConnection>, Mock<IDbTransaction>) BuildMockConnection(
    Contact? contactData = null, bool throws = false)
  {
    var mockConn = new Mock<IDbConnection>();
    var mockTx = new Mock<IDbTransaction>();

    mockConn.Setup(c => c.BeginTransaction()).Returns(mockTx.Object);
    mockConn.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(mockTx.Object);
    mockConn.SetupGet(c => c.State).Returns(ConnectionState.Open);

    if (throws)
    {
      var throwingCmd = BuildThrowingCommand();
      mockConn.Setup(c => c.CreateCommand()).Returns(throwingCmd.Object);
    }
    else
    {
      // Queue of commands: 1 SELECT (GetList) followed by 4 DELETEs
      // (activity, tagged, subscriptions, contact). Extra calls fall back to
      // the non-query command so the test stays stable if the internal count shifts.
      var selectCmd = BuildReaderCommand(contactData != null
        ? BuildContactTable(contactData)
        : new DataTable());
      var deleteCmd = BuildNonQueryCommand();

      var cmdQueue = new Queue<IDbCommand>(new IDbCommand[]
      {
        selectCmd.Object,
        deleteCmd.Object, deleteCmd.Object,
        deleteCmd.Object, deleteCmd.Object,
      });
      mockConn.Setup(c => c.CreateCommand())
        .Returns(() => cmdQueue.Count > 0 ? cmdQueue.Dequeue() : deleteCmd.Object);
    }

    return (mockConn, mockTx);
  }

  private static Mock<IDbCommand> BuildReaderCommand(DataTable table)
  {
    var mockCmd = new Mock<IDbCommand>();
    var mockParams = new Mock<IDataParameterCollection>();
    var mockParam = new Mock<IDbDataParameter>();

    mockCmd.SetupAllProperties();
    mockCmd.SetupGet(c => c.Parameters).Returns(mockParams.Object);
    mockCmd.Setup(c => c.CreateParameter()).Returns(mockParam.Object);
    mockParam.SetupAllProperties();
    mockParams.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

    mockCmd.Setup(c => c.ExecuteReader()).Returns(() => new DataTableReader(table));
    mockCmd.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
      .Returns(() => new DataTableReader(table));
    mockCmd.Setup(c => c.ExecuteNonQuery()).Returns(0);

    return mockCmd;
  }

  private static Mock<IDbCommand> BuildNonQueryCommand()
  {
    var mockCmd = new Mock<IDbCommand>();
    var mockParams = new Mock<IDataParameterCollection>();
    var mockParam = new Mock<IDbDataParameter>();

    mockCmd.SetupAllProperties();
    mockCmd.SetupGet(c => c.Parameters).Returns(mockParams.Object);
    mockCmd.Setup(c => c.CreateParameter()).Returns(mockParam.Object);
    mockParam.SetupAllProperties();
    mockParams.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

    mockCmd.Setup(c => c.ExecuteNonQuery()).Returns(1);
    var empty = new DataTable();
    mockCmd.Setup(c => c.ExecuteReader()).Returns(() => new DataTableReader(empty));
    mockCmd.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
      .Returns(() => new DataTableReader(empty));

    return mockCmd;
  }

  private static Mock<IDbCommand> BuildThrowingCommand()
  {
    var mockCmd = new Mock<IDbCommand>();
    var mockParams = new Mock<IDataParameterCollection>();
    var mockParam = new Mock<IDbDataParameter>();

    mockCmd.SetupAllProperties();
    mockCmd.SetupGet(c => c.Parameters).Returns(mockParams.Object);
    mockCmd.Setup(c => c.CreateParameter()).Returns(mockParam.Object);
    mockParam.SetupAllProperties();
    mockParams.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

    mockCmd.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
      .Throws(new InvalidOperationException("Simulated database failure"));

    return mockCmd;
  }

  private static DataTable BuildContactTable(Contact contact)
  {
    var table = new DataTable();
    table.Columns.Add("id", typeof(int));
    table.Columns.Add("email", typeof(string));
    table.Columns.Add("name", typeof(string));
    table.Columns.Add("subscribed", typeof(bool));
    table.Columns.Add("key", typeof(string));
    table.Columns.Add("created_at", typeof(DateTimeOffset));

    table.Rows.Add(
      contact.ID ?? 0,
      contact.Email ?? string.Empty,
      contact.Name ?? string.Empty,
      contact.Subscribed,
      contact.Key,
      contact.CreatedAt == default ? DateTimeOffset.UtcNow : contact.CreatedAt
    );
    return table;
  }
}
