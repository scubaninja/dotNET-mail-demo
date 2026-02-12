using Xunit;
using Tailwind.Mail.Commands;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

public class ContactSignupCommandTests
{
    [Fact]
    public void ContactSignupCommand_Constructor_SetsContact()
    {
        // Arrange
        var contact = new Contact("John Doe", "john@example.com");

        // Act
        var command = new ContactSignupCommand(contact);

        // Assert
        Assert.NotNull(command.Contact);
        Assert.Equal(contact, command.Contact);
        Assert.Equal("john@example.com", command.Contact.Email);
        Assert.Equal("John Doe", command.Contact.Name);
    }

    [Fact]
    public void ContactSignupCommand_Contact_HasUniqueKey()
    {
        // Arrange
        var contact = new Contact("Jane Doe", "jane@example.com");

        // Act
        var command = new ContactSignupCommand(contact);

        // Assert
        Assert.NotNull(command.Contact.Key);
        Assert.NotEmpty(command.Contact.Key);
        Assert.True(Guid.TryParse(command.Contact.Key, out _));
    }
}

public class ContactOptOutCommandTests
{
    [Fact]
    public void ContactOptOutCommand_Constructor_SetsKey()
    {
        // Arrange
        var key = "test-contact-key";

        // Act
        var command = new ContactOptOutCommand(key);

        // Assert
        Assert.Equal(key, command.Key);
    }

    [Fact]
    public void ContactOptOutCommand_Key_IsAccessible()
    {
        // Arrange
        var key = "another-key-123";
        var command = new ContactOptOutCommand(key);

        // Act
        var retrievedKey = command.Key;

        // Assert
        Assert.Equal(key, retrievedKey);
    }
}

public class ContactOptinCommandTests
{
    [Fact]
    public void ContactOptinCommand_Constructor_SetsContact()
    {
        // Arrange
        var contact = new Contact("Alice", "alice@example.com");

        // Act
        var command = new ContactOptinCommand(contact);

        // Assert
        Assert.NotNull(command.Contact);
        Assert.Equal(contact, command.Contact);
    }

    [Fact]
    public void ContactOptinCommand_Contact_PropertiesPreserved()
    {
        // Arrange
        var contact = new Contact("Bob", "bob@example.com")
        {
            Subscribed = false,
            ID = 123
        };

        // Act
        var command = new ContactOptinCommand(contact);

        // Assert
        Assert.Equal(123, command.Contact.ID);
        Assert.Equal("bob@example.com", command.Contact.Email);
        Assert.False(command.Contact.Subscribed); // Should be false initially
    }
}
