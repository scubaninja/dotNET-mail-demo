using System;
using Xunit;
using Moq;
using Dapper;
using System.Data;
using Tailwind.Mail.Models;

public class BroadcastTests
{
    [Fact]
    public void FromMarkdownEmail_Should_Create_Broadcast()
    {
        // Arrange
        var markdownEmail = new MarkdownEmail
        {
            Data = new EmailData
            {
                Subject = "Test Subject",
                Slug = "test-slug",
                SendToTag = "test-tag"
            }
        };

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

        // Assert
        Assert.NotNull(broadcast);
    }

    [Fact]
    public void FromMarkdownEmail_Should_Set_Name()
    {
        // Arrange
        var markdownEmail = new MarkdownEmail
        {
            Data = new EmailData
            {
                Subject = "Test Subject",
                Slug = "test-slug",
                SendToTag = "test-tag"
            }
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
            Data = new EmailData
            {
                Subject = "Test Subject",
                Slug = "test-slug",
                SendToTag = "test-tag"
            }
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
            Data = new EmailData
            {
                Subject = "Test Subject",
                Slug = "test-slug",
                SendToTag = "test-tag"
            }
        };

        // Act
        var broadcast = Broadcast.FromMarkdownEmail(markdownEmail);

        // Assert
        Assert.Equal("test-tag", broadcast.SendToTag);
    }

    [Fact]
    public void FromMarkdown_Should_Create_Broadcast()
    {
        // Arrange
        var markdown = "Some markdown content";

        // Mock the MarkdownEmail.FromString method
        var markdownEmail = new MarkdownEmail
        {
            Data = new EmailData
            {
                Subject = "Test Subject",
                Slug = "test-slug",
                SendToTag = "test-tag"
            }
        };

        // Act
        var broadcast = Broadcast.FromMarkdown(markdown);

        // Assert
        Assert.NotNull(broadcast);
    }

    [Fact]
    public void FromMarkdown_Should_Set_Name()
    {
        // Arrange
        var markdown = "Some markdown content";

        // Mock the MarkdownEmail.FromString method
        var markdownEmail = new MarkdownEmail
        {
            Data = new EmailData
            {
                Subject = "Test Subject",
                Slug = "test-slug",
                SendToTag = "test-tag"
            }
        };

        // Act
        var broadcast = Broadcast.FromMarkdown(markdown);

        // Assert
        Assert.Equal("Test Subject", broadcast.Name);
    }

    [Fact]
    public void FromMarkdown_Should_Set_Slug()
    {
        // Arrange
        var markdown = "Some markdown content";

        // Mock the MarkdownEmail.FromString method
        var markdownEmail = new MarkdownEmail
        {
            Data = new EmailData
            {
                Subject = "Test Subject",
                Slug = "test-slug",
                SendToTag = "test-tag"
            }
        };

        // Act
        var broadcast = Broadcast.FromMarkdown(markdown);

        // Assert
        Assert.Equal("test-slug", broadcast.Slug);
    }

    [Fact]
    public void FromMarkdown_Should_Set_SendToTag()
    {
        // Arrange
        var markdown = "Some markdown content";

        // Mock the MarkdownEmail.FromString method
        var markdownEmail = new MarkdownEmail
        {
            Data = new EmailData
            {
                Subject = "Test Subject",
                Slug = "test-slug",
                SendToTag = "test-tag"
            }
        };

        // Act
        var broadcast = Broadcast.FromMarkdown(markdown);

        // Assert
        Assert.Equal("test-tag", broadcast.SendToTag);
    }

    [Fact]
    public void ContactCount_Should_Return_Correct_Count()
    {
        // Arrange
        var broadcast = new Broadcast { SendToTag = "test-tag" };
        var mockConnection = new Mock<IDbConnection>();
        mockConnection.Setup(conn => conn.ExecuteScalar<long>(It.IsAny<string>(), It.IsAny<object>()))
                      .Returns(10);

        // Act
        var count = broadcast.ContactCount(mockConnection.Object);

        // Assert
        Assert.Equal(10, count);
    }
}