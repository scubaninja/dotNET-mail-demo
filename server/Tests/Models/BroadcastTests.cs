using System;
using System.Data;
using Xunit;
using Moq;
using Dapper;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests.Models
{
    public class BroadcastTests
    {
        [Fact]
        public void FromMarkdownEmail_ValidInput_CreatesBroadcast()
        {
            // Arrange
            string markdown = @"---
Subject: Test Broadcast
Slug: test-broadcast
SendToTag: premium-users
---

# Broadcast Content";
            var markdownEmail = MarkdownEmail.FromString(markdown);

            // Act
            var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

            // Assert
            Assert.Equal("Test Broadcast", broadcast.Name);
            Assert.Equal("test-broadcast", broadcast.Slug);
            Assert.Equal("premium-users", broadcast.SendToTag);
            Assert.Equal("pending", broadcast.Status);
            Assert.Null(broadcast.ID);
            Assert.Null(broadcast.EmailId);
        }

        [Fact]
        public void FromMarkdownEmail_NullInput_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Broadcast.FromMarkdownEmail(null));
        }

        [Fact]
        public void FromMarkdown_EmptyString_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => Broadcast.FromMarkdown(""));
        }

        [Fact]
        public void ContactCount_AllContacts_ReturnsCorrectCount()
        {
            // Arrange
            string markdown = @"---
Subject: Test Broadcast
Summary: Test Summary
SendToTag: *
---

Content";
            var broadcast = Broadcast.FromMarkdown(markdown);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(c => c.ExecuteScalar<long>(
                It.Is<string>(s => s.Contains("COUNT(1) FROM mail.contacts")),
                null, null, null, null))
                .Returns(42);

            // Act
            var count = broadcast.ContactCount(mockConnection.Object);

            // Assert
            Assert.Equal(42, count);
        }

        [Fact]
        public void ContactCount_SpecificTag_ReturnsCorrectCount()
        {
            // Arrange
            string markdown = @"---
Subject: Test Broadcast
Summary: Test Summary
SendToTag: premium
---

Content";
            var broadcast = Broadcast.FromMarkdown(markdown);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(c => c.ExecuteScalar<long>(
                It.Is<string>(s => s.Contains("tags.slug = @tagSlug")),
                It.Is<object>(p => ((dynamic)p).tagSlug == "premium"),
                null, null, null))
                .Returns(10);

            // Act
            var count = broadcast.ContactCount(mockConnection.Object);

            // Assert
            Assert.Equal(10, count);
        }

        [Fact]
        public void ContactCount_NullConnection_ThrowsException()
        {
            // Arrange
            var broadcast = Broadcast.FromMarkdown(@"---
Subject: Test
Summary: Test
---
Content");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => broadcast.ContactCount(null));
        }
    }
}
