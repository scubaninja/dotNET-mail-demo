using Xunit;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;
using System;
using System.Data;
using Moq;
using Dapper;
using System.Dynamic;

namespace Tailwind.Mail.Tests.Commands;

public class CreateBroadcastTests
{
    private MarkdownEmail CreateTestMarkdownEmail()
    {
        var markdownEmail = new MarkdownEmail();
        dynamic data = new ExpandoObject();
        data.Subject = "Test Broadcast";
        data.Summary = "Test broadcast summary";
        data.Slug = "test-broadcast";
        data.SendToTag = "*";
        markdownEmail.Data = data;
        markdownEmail.Html = "<p>Test broadcast content</p>";
        return markdownEmail;
    }

    [Fact]
    public void Execute_ValidBroadcastToAllSubscribers_CreatesMessagesAndReturnsSuccess()
    {
        // Arrange
        var mdEmail = CreateTestMarkdownEmail();
        var command = new CreateBroadcast(mdEmail);
        
        var mockConnection = new Mock<IDbConnection>();
        var mockTransaction = new Mock<IDbTransaction>();
        
        mockConnection.Setup(c => c.BeginTransaction()).Returns(mockTransaction.Object);
        
        // Setup Insert to return IDs
        mockConnection.Setup(c => c.Insert(It.IsAny<Email>(), It.Is<IDbTransaction>(t => t == mockTransaction.Object)))
            .Returns(123); // Email ID
            
        mockConnection.Setup(c => c.Insert(It.IsAny<Broadcast>(), It.Is<IDbTransaction>(t => t == mockTransaction.Object)))
            .Returns(456); // Broadcast ID
            
        // Setup Execute for inserting messages
        mockConnection.Setup(c => c.Execute(
            It.IsAny<string>(), 
            It.IsAny<object>(), 
            It.IsAny<IDbTransaction>(), 
            It.IsAny<int?>(), 
            It.IsAny<CommandType>()))
            .Returns(10); // 10 messages created
            
        // Setup Execute for notification
        mockConnection.Setup(c => c.Execute(
            It.Is<string>(s => s.Contains("NOTIFY")), 
            It.IsAny<object>(), 
            It.Is<IDbTransaction>(t => t == mockTransaction.Object)))
            .Returns(1);
        
        // Act
        var result = command.Execute(mockConnection.Object);
        
        // Assert
        mockTransaction.Verify(t => t.Commit(), Times.Once);
        mockTransaction.Verify(t => t.Rollback(), Times.Never);
        
        Assert.Equal(10, result.Inserted);
        Assert.Equal(123, ((dynamic)result.Data).EmailId);
        Assert.Equal(456, ((dynamic)result.Data).BroadcastId);
        Assert.True((bool)((dynamic)result.Data).Notified);
    }
    
    [Fact]
    public void Execute_ValidBroadcastToSpecificTag_CreatesFilteredMessagesAndReturnsSuccess()
    {
        // Arrange
        var mdEmail = CreateTestMarkdownEmail();
        ((dynamic)mdEmail.Data).SendToTag = "newsletter";
        
        var command = new CreateBroadcast(mdEmail);
        
        var mockConnection = new Mock<IDbConnection>();
        var mockTransaction = new Mock<IDbTransaction>();
        
        mockConnection.Setup(c => c.BeginTransaction()).Returns(mockTransaction.Object);
        
        // Setup Insert to return IDs
        mockConnection.Setup(c => c.Insert(It.IsAny<Email>(), It.Is<IDbTransaction>(t => t == mockTransaction.Object)))
            .Returns(123); // Email ID
            
        mockConnection.Setup(c => c.Insert(It.IsAny<Broadcast>(), It.Is<IDbTransaction>(t => t == mockTransaction.Object)))
            .Returns(456); // Broadcast ID
            
        // Setup Execute for inserting messages
        mockConnection.Setup(c => c.Execute(
            It.IsAny<string>(), 
            It.Is<object>(o => ((dynamic)o).tagSlug == "newsletter"), 
            It.IsAny<IDbTransaction>(), 
            It.IsAny<int?>(), 
            It.IsAny<CommandType>()))
            .Returns(5); // 5 messages created for newsletter tag
            
        // Setup Execute for notification
        mockConnection.Setup(c => c.Execute(
            It.Is<string>(s => s.Contains("NOTIFY")), 
            It.IsAny<object>(), 
            It.Is<IDbTransaction>(t => t == mockTransaction.Object)))
            .Returns(1);
        
        // Act
        var result = command.Execute(mockConnection.Object);
        
        // Assert
        mockTransaction.Verify(t => t.Commit(), Times.Once);
        mockTransaction.Verify(t => t.Rollback(), Times.Never);
        
        Assert.Equal(5, result.Inserted);
        Assert.Equal(123, ((dynamic)result.Data).EmailId);
        Assert.Equal(456, ((dynamic)result.Data).BroadcastId);
        Assert.True((bool)((dynamic)result.Data).Notified);
    }
    
    [Fact]
    public void Execute_ExceptionDuringProcess_RollsBackTransactionAndRethrows()
    {
        // Arrange
        var mdEmail = CreateTestMarkdownEmail();
        var command = new CreateBroadcast(mdEmail);
        
        var mockConnection = new Mock<IDbConnection>();
        var mockTransaction = new Mock<IDbTransaction>();
        
        mockConnection.Setup(c => c.BeginTransaction()).Returns(mockTransaction.Object);
        
        // Setup Insert to throw an exception
        mockConnection.Setup(c => c.Insert(It.IsAny<Email>(), It.Is<IDbTransaction>(t => t == mockTransaction.Object)))
            .Throws(new Exception("Database error"));
        
        // Act & Assert
        var exception = Assert.Throws<Exception>(() => command.Execute(mockConnection.Object));
        
        mockTransaction.Verify(t => t.Commit(), Times.Never);
        mockTransaction.Verify(t => t.Rollback(), Times.Once);
        
        Assert.Equal("Database error", exception.Message);
    }
}
