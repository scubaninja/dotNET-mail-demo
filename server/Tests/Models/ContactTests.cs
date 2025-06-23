using Xunit;
using Tailwind.Mail.Models;
using System;

namespace Tailwind.Mail.Tests.Models;

public class ContactTests
{
    [Fact]
    public void Contact_Constructor_SetsNameAndEmail()
    {
        // Arrange
        string expectedName = "Test User";
        string expectedEmail = "test@example.com";
        
        // Act
        var contact = new Contact(expectedName, expectedEmail);
        
        // Assert
        Assert.Equal(expectedName, contact.Name);
        Assert.Equal(expectedEmail, contact.Email);
    }
    
    [Fact]
    public void Contact_DefaultConstructor_SetsEmptyValues()
    {
        // Act
        var contact = new Contact();
        
        // Assert
        Assert.Null(contact.Name);
        Assert.Null(contact.Email);
        Assert.False(contact.Subscribed);
        Assert.NotNull(contact.Key);
        Assert.Null(contact.ID);
    }
    
    [Fact]
    public void Contact_CreatesUniqueKeys()
    {
        // Arrange
        var contact1 = new Contact();
        var contact2 = new Contact();
        
        // Assert
        Assert.NotEqual(contact1.Key, contact2.Key);
    }
}
