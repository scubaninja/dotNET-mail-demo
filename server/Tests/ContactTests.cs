using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

public class ContactTests
{
    [Fact]
    public void Contact_NameEmailConstructor_SetsProperties()
    {
        // Arrange & Act
        var contact = new Contact("Jane Doe", "jane@example.com");

        // Assert
        Assert.Equal("Jane Doe", contact.Name);
        Assert.Equal("jane@example.com", contact.Email);
    }

    [Fact]
    public void Contact_DefaultConstructor_KeyIsNotEmpty()
    {
        // Arrange & Act
        var contact = new Contact();

        // Assert
        Assert.NotNull(contact.Key);
        Assert.NotEmpty(contact.Key);
    }

    [Fact]
    public void Contact_NameEmailConstructor_KeyIsGenerated()
    {
        // Arrange & Act
        var contact = new Contact("Test User", "test@example.com");

        // Assert
        Assert.NotNull(contact.Key);
        Assert.NotEmpty(contact.Key);
    }

    [Fact]
    public void Contact_TwoInstances_HaveUniqueKeys()
    {
        // Arrange & Act
        var contact1 = new Contact("User One", "one@example.com");
        var contact2 = new Contact("User Two", "two@example.com");

        // Assert
        Assert.NotEqual(contact1.Key, contact2.Key);
    }
}
