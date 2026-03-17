using System;
using System.Data;
using Moq;
using Xunit;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

public class ContactSignupCommandTests
{
    [Fact]
    public void Constructor_SetsContact()
    {
        var contact = new Contact("Test User", "test@example.com");

        var cmd = new ContactSignupCommand(contact);

        Assert.Equal(contact, cmd.Contact);
    }

    [Fact]
    public void Execute_WhenGetListThrows_ExceptionPropagates()
    {
        // In ContactSignupCommand, GetList is called outside the try-catch block,
        // so exceptions from the DB propagate to the caller.
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
            .Throws(new Exception("Database error"));

        var contact = new Contact("Test User", "test@example.com");
        var cmd = new ContactSignupCommand(contact);

        Assert.ThrowsAny<Exception>(() => cmd.Execute(mockConnection.Object));
    }
}
