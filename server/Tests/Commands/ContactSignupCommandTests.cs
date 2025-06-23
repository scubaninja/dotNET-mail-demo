using Xunit;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;
using System;
using System.Data;
using Moq;
using Dapper;
using System.Collections.Generic;
using System.Linq;

namespace Tailwind.Mail.Tests.Commands;

public class ContactSignupCommandTests
{
    [Fact]
    public void Execute_NewUser_InsertsContactAndActivity()
    {
        // Arrange
        var contact = new Contact("Test User", "test@example.com");
        var command = new ContactSignupCommand(contact);
        
        var mockConnection = new Mock<IDbConnection>();
        var mockTransaction = new Mock<IDbTransaction>();
        
        mockConnection.Setup(c => c.BeginTransaction()).Returns(mockTransaction.Object);
        
        // Setup to return empty list for GetList (user doesn't exist)
        mockConnection.Setup(c => c.GetList<Contact>(
            It.IsAny<object>(), 
            It.Is<IDbTransaction>(t => t == mockTransaction.Object)))
            .Returns(new List<Contact>().AsQueryable());
        
        // Setup Insert to return an ID
        mockConnection.Setup(c => c.Insert(
            It.IsAny<Contact>(), 
            It.Is<IDbTransaction>(t => t == mockTransaction.Object)))
            .Returns(1);
        
        mockConnection.Setup(c => c.Insert(
            It.IsAny<Activity>(), 
            It.Is<IDbTransaction>(t => t == mockTransaction.Object)));
        
        // Act
        var result = command.Execute(mockConnection.Object);
        
        // Assert
        mockTransaction.Verify(t => t.Commit(), Times.Once);
        mockTransaction.Verify(t => t.Rollback(), Times.Never);
        
        Assert.Equal(1, result.Inserted);
        Assert.True((bool)((dynamic)result.Data).Success);
        Assert.Equal(1, ((dynamic)result.Data).ID);
    }
    
    [Fact]
    public void Execute_ExistingUser_ReturnsError()
    {
        // Arrange
        var contact = new Contact("Test User", "existing@example.com");
        var command = new ContactSignupCommand(contact);
        
        var mockConnection = new Mock<IDbConnection>();
        var mockTransaction = new Mock<IDbTransaction>();
        
        mockConnection.Setup(c => c.BeginTransaction()).Returns(mockTransaction.Object);
        
        // Setup to return a user (user exists)
        var existingContacts = new List<Contact> { new Contact("Existing", "existing@example.com") };
        mockConnection.Setup(c => c.GetList<Contact>(
            It.IsAny<object>(), 
            It.Is<IDbTransaction>(t => t == mockTransaction.Object)))
            .Returns(existingContacts.AsQueryable());
        
        // Act
        var result = command.Execute(mockConnection.Object);
        
        // Assert
        mockTransaction.Verify(t => t.Commit(), Times.Never);
        
        Assert.False((bool)((dynamic)result.Data).Success);
        Assert.Equal("User exists", ((dynamic)result.Data).Message);
    }
    
    [Fact]
    public void Execute_ExceptionDuringInsert_RollsBackTransaction()
    {
        // Arrange
        var contact = new Contact("Test User", "test@example.com");
        var command = new ContactSignupCommand(contact);
        
        var mockConnection = new Mock<IDbConnection>();
        var mockTransaction = new Mock<IDbTransaction>();
        
        mockConnection.Setup(c => c.BeginTransaction()).Returns(mockTransaction.Object);
        
        // Setup to return empty list for GetList (user doesn't exist)
        mockConnection.Setup(c => c.GetList<Contact>(
            It.IsAny<object>(), 
            It.Is<IDbTransaction>(t => t == mockTransaction.Object)))
            .Returns(new List<Contact>().AsQueryable());
        
        // Setup Insert to throw an exception
        mockConnection.Setup(c => c.Insert(
            It.IsAny<Contact>(), 
            It.Is<IDbTransaction>(t => t == mockTransaction.Object)))
            .Throws(new Exception("Database error"));
        
        // Act
        var result = command.Execute(mockConnection.Object);
        
        // Assert
        mockTransaction.Verify(t => t.Commit(), Times.Never);
        mockTransaction.Verify(t => t.Rollback(), Times.Once);
        
        Assert.False((bool)((dynamic)result.Data).Success);
        Assert.Equal("Database error", ((dynamic)result.Data).Message);
    }
}
