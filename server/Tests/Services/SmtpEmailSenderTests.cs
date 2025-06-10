using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Tailwind.Mail.Services;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests.Services
{
    public class SmtpEmailSenderTests
    {
        [Fact]
        public async Task SendAsync_ValidMessage_SendsEmail()
        {
            // This test is more of an integration test than a unit test
            // For a real test, you'd want to mock the SMTP client
            // Here we'll just verify the method doesn't throw with valid inputs

            // Arrange - create a test message
            var message = new Message
            {
                SendTo = "test@example.com",
                SendFrom = "sender@example.com",
                Subject = "Test Email",
                Html = "<p>Test content</p>"
            };

            // Create the sender with test configuration
            // In a real test, you'd inject mock configuration
            var config = new Mock<IConfiguration>();
            config.Setup(c => c.Get("SMTP_HOST")).Returns("localhost");
            config.Setup(c => c.Get("SMTP_USER")).Returns("testuser");
            config.Setup(c => c.Get("SMTP_PASSWORD")).Returns("testpass");
            
            // Skip actual sending in tests
            var sender = new TestSmtpEmailSender(config.Object);

            // Act
            var exception = await Record.ExceptionAsync(async () => 
                await sender.SendAsync(message));

            // Assert
            Assert.Null(exception); // No exception means success
            Assert.True(sender.SendCalled);
        }

        [Fact]
        public async Task SendAsync_NullMessage_ThrowsException()
        {
            // Arrange
            var config = new Mock<IConfiguration>();
            var sender = new TestSmtpEmailSender(config.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await sender.SendAsync(null));
        }

        // Test implementation that doesn't actually send emails
        private class TestSmtpEmailSender : SmtpEmailSender
        {
            public bool SendCalled { get; private set; }

            public TestSmtpEmailSender(IConfiguration config) : base(config)
            {
                SendCalled = false;
            }

            // Override to prevent actual sending
            protected override Task SendMailAsync(Message message)
            {
                SendCalled = true;
                return Task.CompletedTask;
            }
        }
    }

    // Mock interface for configuration
    public interface IConfiguration
    {
        string Get(string key);
    }
}
