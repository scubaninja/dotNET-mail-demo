using System.Data;
using Moq;
using Xunit;
using Tailwind.Mail.Services;
using Tailwind.Data;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests.Services;

public class BackgroundSendTests
{
    private readonly Mock<IEmailSender> _mockEmailSender;
    private readonly Mock<IDb> _mockDb;
    private readonly Mock<IDbConnection> _mockConn;

    public BackgroundSendTests()
    {
        _mockEmailSender = new Mock<IEmailSender>();
        _mockDb = new Mock<IDb>();
        _mockConn = new Mock<IDbConnection>();
        _mockDb.Setup(db => db.Connect()).Returns(_mockConn.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithPendingMessages_SendsEmails()
    {
        // Arrange
        var messages = new List<dynamic>
        {
            new { id = 1, subject = "Test", status = "pending", slug = "test", 
                  html = "test", send_at = DateTime.UtcNow, send_to = "test@test.com", 
                  send_from = "from@test.com" }
        };
        
        _mockConn.Setup(c => c.QueryAsync(It.IsAny<string>()))
                 .ReturnsAsync(messages);
        
        _mockEmailSender.Setup(s => s.SendBulk(It.IsAny<List<Message>>()))
                       .ReturnsAsync(1);

        var service = new BackgroundSend(_mockEmailSender.Object, _mockDb.Object);
        var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(2000); // Wait for one iteration
        cts.Cancel();
        await service.StopAsync(cts.Token);

        // Assert
        _mockEmailSender.Verify(s => s.SendBulk(It.Is<List<Message>>(
            m => m.Count == 1 && m[0].Subject == "Test"
        )), Times.AtLeast(1));
    }
}
