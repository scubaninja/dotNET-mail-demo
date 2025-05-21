using System;
using System.Data;
using Dapper;
using Moq;
using Tailwind.Mail.Models;
using Xunit;

public class BroadcastTests {
    [Fact]
    public void FromMarkdownEmail_ValidInput_ReturnsBroadcast() {
        // Arrange
        var markdownEmail = new MarkdownEmail {
            Data = new MarkdownEmailData {
                Subject = "Test Subject",
                Slug = "test-slug",
                SendToTag = "test-tag"
            }
        };

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

        // Assert
        Assert.NotNull(broadcast);
        Assert.Equal("Test Subject", broadcast.Name);
        Assert.Equal("test-slug", broadcast.Slug);
        Assert.Equal("test-tag", broadcast.SendToTag);
    }

    [Fact]
    public void FromMarkdownEmail_NullInput_ThrowsArgumentNullException() {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Broadcast.FromMarkdownEmail(null));
    }

    [Fact]
    public void FromMarkdown_ValidInput_ReturnsBroadcast() {
        // Arrange
        var markdown = @"
        ---
        subject: Test Subject
        slug: test-slug
        sendToTag: test-tag
        ---
        ";

        // Act
        var broadcast = Broadcast.FromMarkdown(markdown);

        // Assert
        Assert.NotNull(broadcast);
        Assert.Equal("Test Subject", broadcast.Name);
        Assert.Equal("test-slug", broadcast.Slug);
        Assert.Equal("test-tag", broadcast.SendToTag);
    }

    [Fact]
    public void FromMarkdown_InvalidInput_ThrowsArgumentException() {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Broadcast.FromMarkdown(null));
        Assert.Throws<ArgumentException>(() => Broadcast.FromMarkdown(""));
    }

    [Fact]
    public void ContactCount_ValidConnection_ReturnsContactCount() {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        mockConnection
            .Setup(conn => conn.ExecuteScalar<long>(
                "SELECT COUNT(1) FROM mail.contacts WHERE subscribed = true"
            ))
            .Returns(100);

        var markdownEmail = new MarkdownEmail();
        markdownEmail.Data = new {
            Subject = "Test Subject",
            Slug = "test-slug",
            SendToTag = "*"
        };
        var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

        // Act
        var count = broadcast.ContactCount(mockConnection.Object);

        // Assert
        Assert.Equal(100, count);
    }

    [Fact]
    public void ContactCount_NullConnection_ThrowsArgumentNullException() {
        // Arrange
        var markdownEmail = new MarkdownEmail();
        markdownEmail.Data = new {
            Subject = "Test Subject",
            Slug = "test-slug",
            SendToTag = "*"
        };
        var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => broadcast.ContactCount(null));
    }

    [Fact]
    public void ContactCount_WithTag_ReturnsFilteredContactCount() {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        var sql = @"
          SELECT COUNT(1) AS count FROM mail.contacts 
          INNER JOIN mail.tagged ON mail.tagged.contact_id = mail.contacts.id
          INNER JOIN mail.tags ON mail.tags.id = mail.tagged.tag_id
          WHERE subscribed = true
          AND tags.slug = @tagSlug
        ";
        mockConnection
            .Setup(conn => conn.ExecuteScalar<long>(sql, new { tagSlug = "test-tag" }))
            .Returns(50);

        var markdownEmail = new MarkdownEmail();
        markdownEmail.Data = new {
            Subject = "Test Subject",
            Slug = "test-slug",
            SendToTag = "test-tag"
        };
        var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

        // Act
        var count = broadcast.ContactCount(mockConnection.Object);

        // Assert
        Assert.Equal(50, count);
    }
}
