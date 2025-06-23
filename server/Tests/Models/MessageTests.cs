using Xunit;
using Tailwind.Mail.Models;
using System;

namespace Tailwind.Mail.Tests.Models;

public class MessageTests
{
    [Fact]
    public void Message_Constructor_SetsProperties()
    {
        // Arrange
        string expectedSlug = "welcome-email";
        string expectedSendTo = "test@example.com";
        string expectedSubject = "Welcome to the service";
        string expectedHtml = "<p>Welcome!</p>";
        
        // Act
        var message = new Message(expectedSlug, expectedSendTo, expectedSubject, expectedHtml);
        
        // Assert
        Assert.Equal(expectedSlug, message.Slug);
        Assert.Equal(expectedSendTo, message.SendTo);
        Assert.Equal(expectedSubject, message.Subject);
        Assert.Equal(expectedHtml, message.Html);
        Assert.Equal("pending", message.Status);
        Assert.Equal("broadcast", message.Source);
        Assert.Equal("noreply@tailwind.dev", message.SendFrom);
    }
    
    [Fact]
    public void Message_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var message = new Message();
        
        // Assert
        Assert.Null(message.ID);
        Assert.Equal("broadcast", message.Source);
        Assert.Null(message.Slug);
        Assert.Equal("pending", message.Status);
        Assert.Null(message.SendTo);
        Assert.Equal("noreply@tailwind.dev", message.SendFrom);
        Assert.Null(message.Subject);
        Assert.Null(message.Html);
    }
    
    [Fact]
    public void Sent_SetsStatusToSentAndUpdatesTimestamp()
    {
        // Arrange
        var message = new Message("test-slug", "test@example.com", "Test", "<p>Test</p>");
        var beforeSent = DateTimeOffset.UtcNow;
        
        // Act
        message.Sent();
        
        // Assert
        Assert.Equal("sent", message.Status);
        Assert.True(message.SentAt >= beforeSent);
        Assert.True(message.SentAt <= DateTimeOffset.UtcNow);
    }
    
    [Theory]
    [InlineData("test@example.com", "sender@example.com", "Subject", "<p>Body</p>", "pending", true)]
    [InlineData("", "sender@example.com", "Subject", "<p>Body</p>", "pending", false)]
    [InlineData("test@example.com", "", "Subject", "<p>Body</p>", "pending", false)]
    [InlineData("test@example.com", "sender@example.com", "", "<p>Body</p>", "pending", false)]
    [InlineData("test@example.com", "sender@example.com", "Subject", "", "pending", false)]
    [InlineData("test@example.com", "sender@example.com", "Subject", "<p>Body</p>", "sent", false)]
    public void ReadyToSend_ReturnsExpectedResult(string sendTo, string sendFrom, string subject, string html, string status, bool expected)
    {
        // Arrange
        var message = new Message
        {
            SendTo = sendTo,
            SendFrom = sendFrom,
            Subject = subject,
            Html = html,
            Status = status
        };
        
        // Act
        var result = message.ReadyToSend();
        
        // Assert
        Assert.Equal(expected, result);
    }
}
