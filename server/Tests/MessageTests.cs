using Xunit;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

public class MessageTests
{
    [Fact]
    public void Message_ParameterizedConstructor_SetsProperties()
    {
        // Arrange
        var slug = "test-message";
        var sendTo = "test@example.com";
        var subject = "Test Subject";
        var html = "<p>Test content</p>";

        // Act
        var message = new Message(slug, sendTo, subject, html);

        // Assert
        Assert.Equal(slug, message.Slug);
        Assert.Equal(sendTo, message.SendTo);
        Assert.Equal(subject, message.Subject);
        Assert.Equal(html, message.Html);
    }

    [Fact]
    public void Message_DefaultConstructor_InitializesWithDefaults()
    {
        // Arrange & Act
        var message = new Message();

        // Assert
        Assert.Equal("broadcast", message.Source);
        Assert.Equal("pending", message.Status);
        Assert.Equal("noreply@tailwind.dev", message.SendFrom);
        Assert.NotEqual(default(DateTimeOffset), message.CreatedAt);
        Assert.NotEqual(default(DateTimeOffset), message.SendAt);
    }

    [Fact]
    public void Message_ReadyToSend_AllFieldsSet_ReturnsTrue()
    {
        // Arrange
        var message = new Message("test", "test@example.com", "Subject", "<p>HTML</p>");
        message.Status = "pending";
        message.SendFrom = "from@example.com";

        // Act
        var isReady = message.ReadyToSend();

        // Assert
        Assert.True(isReady);
    }

    [Fact]
    public void Message_ReadyToSend_MissingSendTo_ReturnsFalse()
    {
        // Arrange
        var message = new Message();
        message.Status = "pending";
        message.SendFrom = "from@example.com";
        message.Subject = "Subject";
        message.Html = "<p>HTML</p>";

        // Act
        var isReady = message.ReadyToSend();

        // Assert
        Assert.False(isReady);
    }

    [Fact]
    public void Message_ReadyToSend_MissingSendFrom_ReturnsFalse()
    {
        // Arrange
        var message = new Message();
        message.Status = "pending";
        message.SendTo = "to@example.com";
        message.SendFrom = "";
        message.Subject = "Subject";
        message.Html = "<p>HTML</p>";

        // Act
        var isReady = message.ReadyToSend();

        // Assert
        Assert.False(isReady);
    }

    [Fact]
    public void Message_ReadyToSend_MissingSubject_ReturnsFalse()
    {
        // Arrange
        var message = new Message();
        message.Status = "pending";
        message.SendTo = "to@example.com";
        message.SendFrom = "from@example.com";
        message.Html = "<p>HTML</p>";

        // Act
        var isReady = message.ReadyToSend();

        // Assert
        Assert.False(isReady);
    }

    [Fact]
    public void Message_ReadyToSend_MissingHtml_ReturnsFalse()
    {
        // Arrange
        var message = new Message();
        message.Status = "pending";
        message.SendTo = "to@example.com";
        message.SendFrom = "from@example.com";
        message.Subject = "Subject";

        // Act
        var isReady = message.ReadyToSend();

        // Assert
        Assert.False(isReady);
    }

    [Fact]
    public void Message_ReadyToSend_StatusNotPending_ReturnsFalse()
    {
        // Arrange
        var message = new Message("test", "to@example.com", "Subject", "<p>HTML</p>");
        message.Status = "sent";
        message.SendFrom = "from@example.com";

        // Act
        var isReady = message.ReadyToSend();

        // Assert
        Assert.False(isReady);
    }

    [Fact]
    public void Message_Sent_UpdatesStatusAndSentAt()
    {
        // Arrange
        var message = new Message();
        var beforeSent = DateTimeOffset.UtcNow;

        // Act
        message.Sent();

        // Assert
        Assert.Equal("sent", message.Status);
        Assert.True(message.SentAt >= beforeSent);
        Assert.True(message.SentAt <= DateTimeOffset.UtcNow);
    }
}
