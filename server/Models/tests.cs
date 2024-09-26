using Xunit;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests
{
    public class BroadcastTests
    {
        [Fact]
        public void Broadcast_SetAndGetProperties_ShouldWorkCorrectly()
        {
            // Arrange
            var broadcast = new Broadcast();
            int? id = 1;
            int? emailId = 2;
            string status = "sent";
            string name = "Test Broadcast";
            string slug = "test-broadcast";

            // Act
            broadcast.ID = id;
            broadcast.EmailId = emailId;
            broadcast.Status = status;
            broadcast.Name = name;
            broadcast.Slug = slug;

            // Assert
            Assert.Equal(id, broadcast.ID);
            Assert.Equal(emailId, broadcast.EmailId);
            Assert.Equal(status, broadcast.Status);
            Assert.Equal(name, broadcast.Name);
            Assert.Equal(slug, broadcast.Slug);
        }
    }
}