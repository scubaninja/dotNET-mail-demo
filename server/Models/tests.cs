using System;
using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests
{
    public class BroadcastTests
    {
        [Fact]
        public void Broadcast_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var broadcast = new Broadcast();

            // Assert
            Assert.Null(broadcast.ID);
            Assert.Null(broadcast.EmailId);
            Assert.Equal("pending", broadcast.Status);
            Assert.Null(broadcast.Name);
            Assert.True(broadcast.CreatedAt <= DateTimeOffset.UtcNow);
        }

        [Fact]
        public void Broadcast_ID_ShouldBeNullByDefault()
        {
            // Arrange & Act
            var broadcast = new Broadcast();

            // Assert
            Assert.Null(broadcast.ID);
        }

        [Fact]
        public void Broadcast_EmailId_ShouldBeNullByDefault()
        {
            // Arrange & Act
            var broadcast = new Broadcast();

            // Assert
            Assert.Null(broadcast.EmailId);
        }

        [Fact]
        public void Broadcast_Status_ShouldBePendingByDefault()
        {
            // Arrange & Act
            var broadcast = new Broadcast();

            // Assert
            Assert.Equal("pending", broadcast.Status);
        }

        [Fact]
        public void Broadcast_Name_ShouldBeNullByDefault()
        {
            // Arrange & Act
            var broadcast = new Broadcast();

            // Assert
            Assert.Null(broadcast.Name);
        }

        [Fact]
        public void Broadcast_CreatedAt_ShouldBeSetToCurrentTime()
        {
            // Arrange & Act
            var broadcast = new Broadcast();

            // Assert
            Assert.True(broadcast.CreatedAt <= DateTimeOffset.UtcNow);
        }

        [Fact]
        public void FromMarkdownEmail_ShouldSetNameCorrectly()
        {
            // Arrange
            var markdownEmail = new MarkdownEmail
            {
                Data = new EmailData
                {
                    Subject = "Test Subject"
                }
            };

            // Act
            var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

            // Assert
            Assert.Equal("Test Subject", broadcast.Name);
        }
    }

    // Mock classes to support the tests
    public class MarkdownEmail
    {
        public EmailData Data { get; set; }
    }

    public class EmailData
    {
        public string Subject { get; set; }
    }
}