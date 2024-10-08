using System;
using System.Data;
using Xunit;
using Moq;
using Tailwind.Mail.Models;

namespace Tailwind.Mail.Tests
{
  public class BroadcastTest
  {
    private readonly Mock<IDbConnection> _mockDbConnection;

    public BroadcastTest()
    {
      _mockDbConnection = new Mock<IDbConnection>();
    }

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
    }

    [Fact]
    public void FromMarkdown_ShouldCreateBroadcast()
    {
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
    }

    [Fact]
    public void ContactCount_ShouldReturnCorrectCount_WhenSendToTagIsWildcard()
    {
      // Arrange
      var broadcast = new Broadcast { SendToTag = "*" };
      _mockDbConnection.Setup(conn => conn.ExecuteScalar<long>(It.IsAny<string>())).Returns(100);

      // Act
      var count = broadcast.ContactCount(_mockDbConnection.Object);

      // Assert
      Assert.Equal(100, count);
    }

    [Fact]
    public void ContactCount_ShouldReturnCorrectCount_WhenSendToTagIsSpecific()
    {
      // Arrange
      var broadcast = new Broadcast { SendToTag = "test-tag" };
      var sql = @"
        select count(1) as count from mail.contacts 
        inner join mail.tagged on mail.tagged.contact_id = mail.contacts.id
        inner join mail.tags on mail.tags.id = mail.tagged.tag_id
        where subscribed = true
        and tags.slug = @tagSlug
      ";
      _mockDbConnection.Setup(conn => conn.ExecuteScalar<long>(sql, It.IsAny<object>())).Returns(50);

      // Act
      var count = broadcast.ContactCount(_mockDbConnection.Object);

      // Assert
      Assert.Equal(50, count);
    }
  }
}