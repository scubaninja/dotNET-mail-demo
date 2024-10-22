using Xunit;
using Tailwind.Mail.Models;
using System.Data;
using Moq;
using Dapper;

namespace Tailwind.Mail.Tests
{
    public class BroadcastTests
    {
        [Fact]
        public void FromMarkdownEmail_Should_Set_Name()
        {
            // Arrange
            var markdownEmail = new MarkdownEmail
            {
                Data = new EmailData { Subject = "Test Subject", Slug = "test-slug", SendToTag = "test-tag" }
            };

            // Act
            var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

            // Assert
            Assert.Equal("Test Subject", broadcast.Name);
        }

        [Fact]
        public void FromMarkdownEmail_Should_Set_Slug()
        {
            // Arrange
            var markdownEmail = new MarkdownEmail
            {
                Data = new EmailData { Subject = "Test Subject", Slug = "test-slug", SendToTag = "test-tag" }
            };

            // Act
            var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

            // Assert
            Assert.Equal("test-slug", broadcast.Slug);
        }

        [Fact]
        public void FromMarkdownEmail_Should_Set_SendToTag()
        {
            // Arrange
            var markdownEmail = new MarkdownEmail
            {
                Data = new EmailData { Subject = "Test Subject", Slug = "test-slug", SendToTag = "test-tag" }
            };

            // Act
            var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

            // Assert
            Assert.Equal("test-tag", broadcast.SendToTag);
        }

        [Fact]
        public void FromMarkdown_Should_Set_Name()
        {
            // Arrange
            var markdown = "# Test Subject\n\nSlug: test-slug\n\nSendToTag: test-tag";

            // Act
            var broadcast = Broadcast.FromMarkdown(markdown);

            // Assert
            Assert.Equal("Test Subject", broadcast.Name);
        }

        [Fact]
        public void FromMarkdown_Should_Set_Slug()
        {
            // Arrange
            var markdown = "# Test Subject\n\nSlug: test-slug\n\nSendToTag: test-tag";

            // Act
            var broadcast = Broadcast.FromMarkdown(markdown);

            // Assert
            Assert.Equal("test-slug", broadcast.Slug);
        }

        [Fact]
        public void FromMarkdown_Should_Set_SendToTag()
        {
            // Arrange
            var markdown = "# Test Subject\n\nSlug: test-slug\n\nSendToTag: test-tag";

            // Act
            var broadcast = Broadcast.FromMarkdown(markdown);

            // Assert
            Assert.Equal("test-tag", broadcast.SendToTag);
        }

        [Fact]
        public void ContactCount_Should_Return_Correct_Count_When_Tag_Is_Asterisk()
        {
            // Arrange
            var mockDbConnection = new Mock<IDbConnection>();
            mockDbConnection.Setup(conn => conn.ExecuteScalar<long>(It.IsAny<string>(), null, null, null, null))
                            .Returns(100);

            var broadcast = new Broadcast { SendToTag = "*" };

            // Act
            var count = broadcast.ContactCount(mockDbConnection.Object);

            // Assert
            Assert.Equal(100, count);
        }

        [Fact]
        public void ContactCount_Should_Return_Correct_Count_When_Tag_Is_Specified()
        {
            // Arrange
            var mockDbConnection = new Mock<IDbConnection>();
            mockDbConnection.Setup(conn => conn.ExecuteScalar<long>(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
                            .Returns(50);

            var broadcast = new Broadcast { SendToTag = "test-tag" };

            // Act
            var count = broadcast.ContactCount(mockDbConnection.Object);

            // Assert
            Assert.Equal(50, count);
        }
    }
}