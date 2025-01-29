using System;
using System.Data;
using Dapper;
using Moq;
using Xunit;
using Tailwind.Mail.Models;

public class BroadcastTests
{
    [Fact]
    public void FromMarkdownEmail_ShouldSetName()
    {
        // Arrange
        var doc = new MarkdownEmail { Data = new EmailData { Subject = "Test Subject", Slug = "test-slug", SendToTag = "test-tag" } };

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(doc);

        // Assert
        Assert.Equal("Test Subject", broadcast.Name);
    }

    [Fact]
    public void FromMarkdownEmail_ShouldSetSlug()
    {
        // Arrange
        var doc = new MarkdownEmail { Data = new EmailData { Subject = "Test Subject", Slug = "test-slug", SendToTag = "test-tag" } };

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(doc);

        // Assert
        Assert.Equal("test-slug", broadcast.Slug);
    }

    [Fact]
    public void FromMarkdownEmail_ShouldSetSendToTag()
    {
        // Arrange
        var doc = new MarkdownEmail { Data = new EmailData { Subject = "Test Subject", Slug = "test-slug", SendToTag = "test-tag" } };

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(doc);

        // Assert
        Assert.Equal("test-tag", broadcast.SendToTag);
    }

    [Fact]
    public void ContactCount_ShouldReturnAllSubscribedContacts_WhenSendToTagIsAsterisk()
    {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        mockConnection.Setup(conn => conn.ExecuteScalar<long>(It.IsAny<string>())).Returns(100);
        var broadcast = new Broadcast { SendToTag = "*" };

        // Act
        var count = broadcast.ContactCount(mockConnection.Object);

        // Assert
        Assert.Equal(100, count);
    }

    [Fact]
    public void ContactCount_ShouldReturnTaggedContacts_WhenSendToTagIsSpecified()
    {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        var sql = @"
            select count(1) as count from mail.contacts 
            inner join mail.tagged on mail.tagged.contact_id = mail.contacts.id
            inner join mail.tags on mail.tags.id = mail.tagged.tag_id
            where subscribed = true
            and tags.slug = @tagSlug
        ";
        mockConnection.Setup(conn => conn.ExecuteScalar<long>(sql, It.IsAny<object>())).Returns(50);
        var broadcast = new Broadcast { SendToTag = "test-tag" };

        // Act
        var count = broadcast.ContactCount(mockConnection.Object);

        // Assert
        Assert.Equal(50, count);
    }
}

// Mock classes to simulate the actual classes used in the Broadcast class
public class MarkdownEmail
{
    public EmailData Data { get; set; }
}

public class EmailData
{
    public string Subject { get; set; }
    public string Slug { get; set; }
    public string SendToTag { get; set; }
}