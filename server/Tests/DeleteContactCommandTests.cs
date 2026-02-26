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
    private static Mock<IDbConnection> CreateMockConnection(IDataReader reader)
    {
        var mockParams = new Mock<IDataParameterCollection>();
        mockParams.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

        var mockCommand = new Mock<IDbCommand>();
        mockCommand.SetupAllProperties();
        mockCommand.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
        mockCommand.Setup(c => c.Parameters).Returns(mockParams.Object);
        mockCommand.Setup(c => c.ExecuteReader()).Returns(reader);
        mockCommand.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(reader);
        mockCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);

        var mockTransaction = new Mock<IDbTransaction>();

        var mockConn = new Mock<IDbConnection>();
        mockConn.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);
        mockConn.Setup(c => c.BeginTransaction()).Returns(mockTransaction.Object);
        mockConn.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(mockTransaction.Object);

        return mockConn;
    }

    private static IDataReader EmptyContactReader()
    {
        var table = new System.Data.DataTable();
        table.Columns.Add("id", typeof(int));
        table.Columns.Add("email", typeof(string));
        table.Columns.Add("name", typeof(string));
        table.Columns.Add("key", typeof(string));
        table.Columns.Add("subscribed", typeof(bool));
        table.Columns.Add("created_at", typeof(DateTimeOffset));
        return new DataTableReader(table);
    }

    private static IDataReader SingleContactReader(int id, string email, string key)
    {
        var table = new System.Data.DataTable();
        table.Columns.Add("id", typeof(int));
        table.Columns.Add("email", typeof(string));
        table.Columns.Add("name", typeof(string));
        table.Columns.Add("key", typeof(string));
        table.Columns.Add("subscribed", typeof(bool));
        table.Columns.Add("created_at", typeof(DateTimeOffset));
        table.Rows.Add(id, email, "Test User", key, true, DateTimeOffset.UtcNow);
        return new DataTableReader(table);
    }

    [Fact]
    public void Execute_ContactNotFound_ReturnsFailure()
    {
        // Arrange
        var mockConn = CreateMockConnection(EmptyContactReader());
        var cmd = new DeleteContactCommand("nonexistent-key");

        // Act
        var result = cmd.Execute(mockConn.Object);

        // Assert
        Assert.Equal(0, result.Deleted);
        Assert.False((bool)result.Data.Success);
        Assert.Equal("Contact not found", (string)result.Data.Message);
    }

    [Fact]
    public void Execute_ContactFound_ReturnsSuccess()
    {
        // Arrange
        var contactKey = Guid.NewGuid().ToString();
        var reader = SingleContactReader(1, "test@example.com", contactKey);
        var mockConn = CreateMockConnection(reader);
        var cmd = new DeleteContactCommand(contactKey);

        // Act
        var result = cmd.Execute(mockConn.Object);

        // Assert
        Assert.Equal(1, result.Deleted);
        Assert.True((bool)result.Data.Success);
        Assert.Equal("Account permanently deleted", (string)result.Data.Message);
    }
}
