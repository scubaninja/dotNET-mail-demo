using Moq;
using Xunit;
using System.Data;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;
using Dapper;

namespace Tailwind.Mail.Tests.Commands;

public class ContactSignupCommandTests
{
    private readonly Mock<IDbConnection> _mockConn;
    private readonly Mock<IDbTransaction> _mockTransaction;

    public ContactSignupCommandTests()
    {
        _mockConn = new Mock<IDbConnection>();
        _mockTransaction = new Mock<IDbTransaction>();
        _mockConn.Setup(c => c.BeginTransaction())
                 .Returns(_mockTransaction.Object);
    }

    [Fact]
    public void Execute_WithNewContact_InsertsContactAndActivity()
    {
        // Arrange
        var contact = new Contact { Email = "test@test.com", Name = "Test User" };
        var command = new ContactSignupCommand(contact);
        
        _mockConn.Setup(c => c.GetList<Contact>(It.IsAny<object>(), It.IsAny<IDbTransaction>()))
                 .Returns(new List<Contact>());
        
        _mockConn.Setup(c => c.Insert(It.IsAny<Contact>(), It.IsAny<IDbTransaction>()))
                 .Returns(1);

        // Act
        var result = command.Execute(_mockConn.Object);

        // Assert
        Assert.True(result.Data.Success);
        Assert.Equal(1, result.Inserted);
        _mockTransaction.Verify(t => t.Commit(), Times.Once);
    }

    [Fact]
    public void Execute_WithExistingContact_ReturnsError()
    {
        // Arrange
        var contact = new Contact { Email = "existing@test.com" };
        var command = new ContactSignupCommand(contact);
        
        _mockConn.Setup(c => c.GetList<Contact>(It.IsAny<object>(), It.IsAny<IDbTransaction>()))
                 .Returns(new List<Contact> { new Contact() });

        // Act
        var result = command.Execute(_mockConn.Object);

        // Assert
        Assert.False(result.Data.Success);
        Assert.Equal("User exists", result.Data.Message);
        _mockTransaction.Verify(t => t.Commit(), Times.Never);
    }
}
