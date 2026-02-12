using Xunit;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

public class ContactTests
{
    [Fact]
    public void Contact_DefaultConstructor_InitializesKey()
    {
        // Arrange & Act
        var contact = new Contact();

        // Assert
        Assert.NotNull(contact.Key);
        Assert.NotEmpty(contact.Key);
    }

    [Fact]
    public void Contact_ParameterizedConstructor_SetsNameAndEmail()
    {
        // Arrange
        var name = "John Doe";
        var email = "john@example.com";

        // Act
        var contact = new Contact(name, email);

        // Assert
        Assert.Equal(name, contact.Name);
        Assert.Equal(email, contact.Email);
        Assert.NotNull(contact.Key);
    }

    [Fact]
    public void Contact_Key_IsGuid()
    {
        // Arrange & Act
        var contact = new Contact();

        // Assert
        Assert.True(Guid.TryParse(contact.Key, out _));
    }

    [Fact]
    public void Contact_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var contact = new Contact();

        // Assert
        Assert.False(contact.Subscribed);
        Assert.Null(contact.ID);
    }

    [Fact]
    public void Contact_MultipleInstances_GenerateUniqueKeys()
    {
        // Arrange & Act
        var contact1 = new Contact();
        var contact2 = new Contact();
        var contact3 = new Contact();

        // Assert
        Assert.NotEqual(contact1.Key, contact2.Key);
        Assert.NotEqual(contact2.Key, contact3.Key);
        Assert.NotEqual(contact1.Key, contact3.Key);
    }
}
