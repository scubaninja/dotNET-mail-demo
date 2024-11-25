using System;
using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests.Models
{
  public class BroadcastTest
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
      Assert.Null(broadcast.Slug);
      Assert.Null(broadcast.ReplyTo);
      Assert.Equal("*", broadcast.SendToTag);
      Assert.True((DateTimeOffset.UtcNow - broadcast.CreatedAt).TotalSeconds < 1);
    }

    [Fact]
    public void Broadcast_SetProperties_ShouldBeSetCorrectly()
    {
      // Arrange
      var broadcast = new Broadcast
      {
        ID = 1,
        EmailId = 2,
        Status = "sent",
        Name = "Test Broadcast",
        Slug = "test-broadcast",
        ReplyTo = "test@example.com",
        SendToTag = "test-tag",
        CreatedAt = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero)
      };

      // Act & Assert
      Assert.Equal(1, broadcast.ID);
      Assert.Equal(2, broadcast.EmailId);
      Assert.Equal("sent", broadcast.Status);
      Assert.Equal("Test Broadcast", broadcast.Name);
      Assert.Equal("test-broadcast", broadcast.Slug);
      Assert.Equal("test@example.com", broadcast.ReplyTo);
      Assert.Equal("test-tag", broadcast.SendToTag);
      Assert.Equal(new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero), broadcast.CreatedAt);
    }

    [Fact]
    public void Broadcast_NullValues_ShouldBeHandledCorrectly()
    {
      // Arrange
      var broadcast = new Broadcast
      {
        Name = null,
        Slug = null,
        ReplyTo = null
      };

      // Act & Assert
      Assert.Null(broadcast.Name);
      Assert.Null(broadcast.Slug);
      Assert.Null(broadcast.ReplyTo);
    }
  }
}