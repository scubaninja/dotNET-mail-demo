using System;
using System.Data;
using Moq;
using Xunit;

namespace Tailwind.Mail.Models.Tests
{
  public class BroadcastTest
  {
    [Fact]
    public void FromMarkdownEmail_ShouldCreateBroadcast()
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
      Assert.Equal("Test Subject", broadcast.Name);
      Assert.Equal("test-slug", broadcast.Slug);
      Assert.Equal("test-tag", broadcast.SendToTag);
    }

    [Fact]
    public void FromMarkdown_ShouldCreateBroadcast()
    {
      // Arrange
      var markdown = "some markdown content";
      var markdownEmail = new MarkdownEmail
      {
        Data = new EmailData
        {
          Subject = "Test Subject",
          Slug = "test-slug",
          SendToTag = "test-tag"
        }
      };
      Mock<MarkdownEmail> mockMarkdownEmail = new Mock<MarkdownEmail>();
      mockMarkdownEmail.Setup(m => m.FromString(markdown)).Returns(markdownEmail);

      // Act
      var broadcast = Broadcast.FromMarkdown(markdown);

      // Assert
      Assert.NotNull(broadcast);
      Assert.Equal("Test Subject", broadcast.Name);
      Assert.Equal("test-slug", broadcast.Slug);
      Assert.Equal("test-tag", broadcast.SendToTag);
    }

    [Fact]
    public void ContactCount_ShouldReturnTotalContacts_WhenSendToTagIsAsterisk()
    {
      // Arrange
      var broadcast = new Broadcast { SendToTag = "*" };
      var mockConnection = new Mock<IDbConnection>();
      mockConnection.Setup(conn => conn.ExecuteScalar<long>(It.IsAny<string>())).Returns(100);

      // Act
      var contactCount = broadcast.ContactCount(mockConnection.Object);

      // Assert
      Assert.Equal(100, contactCount);
    }

    [Fact]
    public void ContactCount_ShouldReturnTaggedContacts_WhenSendToTagIsSpecificTag()
    {
      // Arrange
      var broadcast = new Broadcast { SendToTag = "test-tag" };
      var mockConnection = new Mock<IDbConnection>();
      mockConnection.Setup(conn => conn.ExecuteScalar<long>(It.IsAny<string>(), It.IsAny<object>())).Returns(50);

      // Act
      var contactCount = broadcast.ContactCount(mockConnection.Object);

      // Assert
      Assert.Equal(50, contactCount);
    }
  }

  // Mock classes for MarkdownEmail and EmailData
  public class MarkdownEmail
  {
    public EmailData Data { get; set; }

    public static MarkdownEmail FromString(string markdown)
    {
      // Mock implementation
      return new MarkdownEmail();
    }
  }

  public class EmailData
  {
    public string Subject { get; set; }
    public string Slug { get; set; }
    public string SendToTag { get; set; }
  }
}