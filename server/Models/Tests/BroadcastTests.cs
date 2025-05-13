using Xunit;
using Moq;
using System.Data;
using Dapper;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests;

public class BroadcastTests
{
    [Fact]
    public void FromMarkdownEmail_WithValidDoc_CreatesBroadcast()
    {
        // Arrange
        var doc = new MarkdownEmail {
            Data = new MarkdownMetadata {
                Subject = "Test Subject",
                Slug = "test-slug",
                SendToTag = "test-tag"
            }
        };

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(doc);

        // Assert
        Assert.Equal("Test Subject", broadcast.Name);
        Assert.Equal("test-slug", broadcast.Slug);
        Assert.Equal("test-tag", broadcast.SendToTag);
    }

    [Fact]
    public void ContactCount_WithWildcardTag_ReturnsAllSubscribed()
    {
        // Arrange
        var mockConn = new Mock<IDbConnection>();
        mockConn.Setup(x => x.ExecuteScalar<long>(
            It.IsAny<string>(),
            null,
            null,
            null,
            null
        )).Returns(10);

        var broadcast = Broadcast.FromMarkdown("""
            ---
            slug: test
            subject: Test
            sendToTag: "*"
            ---
            Test content
            """);

        // Act
        var count = broadcast.ContactCount(mockConn.Object);

        // Assert
        Assert.Equal(10, count);
    }
}
