using Xunit;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests
{
    public class BroadcastTests
    {
        [Fact]
        public void Broadcast_DefaultStatus_ShouldBePending()
        {
            // Arrange
            var broadcast = new Broadcast();

            // Act
            var status = broadcast.Status;

            // Assert
            Assert.Equal("pending", status);
        }

        [Fact]
        public void Broadcast_ID_ShouldBeNullInitially()
        {
            // Arrange
            var broadcast = new Broadcast();

            // Act
            var id = broadcast.ID;

            // Assert
            Assert.Null(id);
        }

        [Fact]
        public void Broadcast_EmailId_ShouldBeNullInitially()
        {
            // Arrange
            var broadcast = new Broadcast();

            // Act
            var emailId = broadcast.EmailId;

            // Assert
            Assert.Null(emailId);
        }

        [Fact]
        public void Broadcast_Name_ShouldBeNullInitially()
        {
            // Arrange
            var broadcast = new Broadcast();

            // Act
            var name = broadcast.Name;

            // Assert
            Assert.Null(name);
        }

        [Fact]
        public void Broadcast_Slug_ShouldBeNullInitially()
        {
            // Arrange
            var broadcast = new Broadcast();

            // Act
            var slug = broadcast.Slug;

            // Assert
            Assert.Null(slug);
        }

        [Fact]
        public void Broadcast_ReplyTo_ShouldBeNullInitially()
        {
            // Arrange
            var broadcast = new Broadcast();

            // Act
            var replyTo = broadcast.ReplyTo;

            // Assert
            Assert.Null(replyTo);
        }
    }
}