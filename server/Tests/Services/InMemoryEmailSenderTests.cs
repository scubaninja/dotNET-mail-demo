using Xunit;
using Tailwind.Mail.Services;
using Tailwind.Mail.Models;
using System.Threading.Tasks;
using System.Linq;

namespace Tailwind.Mail.Tests.Services;

public class InMemoryEmailSenderTests
{
    [Fact]
    public async Task Send_UpdatesMessageStatusAndReturnsSameMessage()
    {
        // Arrange
        var sender = new InMemoryEmailSender();
        var message = new Message("test-slug", "test@example.com", "Test Subject", "<p>Test Body</p>");
        
        // Act
        var result = await sender.Send(message);
        
        // Assert
        Assert.Same(message, result);
        Assert.Equal("sent", message.Status);
        Assert.NotEqual(default, message.SentAt);
    }
    
    [Fact]
    public async Task SendBulk_UpdatesAllMessagesAndReturnsCount()
    {
        // Arrange
        var sender = new InMemoryEmailSender();
        var messages = new[]
        {
            new Message("slug-1", "test1@example.com", "Subject 1", "<p>Body 1</p>"),
            new Message("slug-2", "test2@example.com", "Subject 2", "<p>Body 2</p>"),
            new Message("slug-3", "test3@example.com", "Subject 3", "<p>Body 3</p>")
        };
        
        // Act
        var count = await sender.SendBulk(messages);
        
        // Assert
        Assert.Equal(messages.Length, count);
        
        foreach (var message in messages)
        {
            Assert.Equal("sent", message.Status);
            Assert.NotEqual(default, message.SentAt);
        }
    }
}
