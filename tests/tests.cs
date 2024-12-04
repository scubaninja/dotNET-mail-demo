using System.Data;
using Dapper;
using Moq;
using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests
{
    public class BroadcastTests
    {
        [Fact]
        public void FromMarkdownEmail_ShouldCreateBroadcast()
        {
            // Arrange
            var markdownEmail = new MarkdownEmail
            {
                Data = new
                {
                    Subject = "Test Subject",
                    Slug = "test-subject",
                    SendToTag = "*"
                }
            };

            // Act
            var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

            // Assert
            Assert.NotNull(broadcast);
            Assert.Equal("Test Subject", broadcast.Name);
            Assert.Equal("test-subject", broadcast.Slug);
            Assert.Equal("*", broadcast.SendToTag);
        }

        [Fact]
        public void FromMarkdown_ShouldCreateBroadcast()
        {
            // Arrange
            var markdown = @"
---
Subject: Test Subject
Slug: test-subject
SendToTag: *
---
";

            // Act
            var broadcast = Broadcast.FromMarkdown(markdown);

            // Assert
            Assert.NotNull(broadcast);
            Assert.Equal("Test Subject", broadcast.Name);
            Assert.Equal("test-subject", broadcast.Slug);
            Assert.Equal("*", broadcast.SendToTag);
        }

        [Fact]
        public void ContactCount_ShouldReturnCorrectCount()
        {
            // Arrange
            var mockDbConnection = new Mock<IDbConnection>();
            mockDbConnection.Setup(conn => conn.ExecuteScalar<long>(It.IsAny<string>())).Returns(10);

            var broadcast = new Broadcast
            {
                SendToTag = "*"
            };

            // Act
            var count = broadcast.ContactCount(mockDbConnection.Object);

            // Assert
            Assert.Equal(10, count);
        }
    }
}