using System;
using System.Data;
using Moq;
using Xunit;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

public class ContactOptinCommandTests
{
    [Fact]
    public void Constructor_SetsContact()
    {
        var contact = new Contact("Test User", "test@example.com");

        var cmd = new ContactOptinCommand(contact);

        Assert.Equal(contact, cmd.Contact);
    }

    [Fact]
    public void Execute_WhenConnectionThrows_ReturnsErrorResult()
    {
        var mockConnection = new Mock<IDbConnection>();
        var mockTransaction = new Mock<IDbTransaction>();
        var mockCommand = new Mock<IDbCommand>();
        var mockParameters = new Mock<IDataParameterCollection>();
        var mockParam = new Mock<IDbDataParameter>();

        mockConnection.Setup(c => c.BeginTransaction()).Returns(mockTransaction.Object);
        mockConnection.SetupGet(c => c.State).Returns(ConnectionState.Open);
        mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);
        mockCommand.SetupGet(c => c.Parameters).Returns(mockParameters.Object);
        mockCommand.Setup(c => c.CreateParameter()).Returns(mockParam.Object);
        mockCommand.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
            .Throws(new Exception("Database connection failed"));

        var contact = new Contact("Test User", "test@example.com") { ID = 1 };
        var cmd = new ContactOptinCommand(contact);
        var result = cmd.Execute(mockConnection.Object);

        Assert.NotNull(result);
        mockTransaction.Verify(t => t.Rollback(), Times.Once);
    }
}
