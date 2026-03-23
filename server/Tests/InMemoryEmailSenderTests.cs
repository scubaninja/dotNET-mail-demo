using Tailwind.Mail.Models;
using Tailwind.Mail.Services;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for <see cref="InMemoryEmailSender"/>, the in-process email sender used
/// during automated testing that stores sent messages in memory rather than
/// transmitting them over a network.
/// </summary>
public class InMemoryEmailSenderTests
{
    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a fully-populated <see cref="Message"/> ready to be sent.
    /// </summary>
    private static Message BuildMessage(string slug = "welcome", string to = "user@test.com")
        => new Message(slug, to, "Test Subject", "<p>Hello</p>")
        {
            SendFrom = "noreply@tailwind.dev"
        };

    // ──────────────────────────────────────────────────────────────
    // Send
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="InMemoryEmailSender.Send"/> transitions the message
    /// status to <c>"sent"</c> and returns the same message instance.
    /// </summary>
    [Fact]
    public async Task Send_ValidMessage_MarksMsgAsSentAndReturnsIt()
    {
        var sender = new InMemoryEmailSender();
        var message = BuildMessage();

        var result = await sender.Send(message);

        Assert.Equal("sent", result.Status);
        Assert.Same(message, result);
    }

    /// <summary>
    /// Verifies that calling <see cref="InMemoryEmailSender.Send"/> does not throw
    /// even when called multiple times in succession.
    /// </summary>
    [Fact]
    public async Task Send_CalledMultipleTimes_DoesNotThrow()
    {
        var sender = new InMemoryEmailSender();

        for (var i = 0; i < 5; i++)
        {
            var msg = BuildMessage(slug: $"slug-{i}", to: $"user{i}@test.com");
            await sender.Send(msg);
        }
    }

    // ──────────────────────────────────────────────────────────────
    // SendBulk
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="InMemoryEmailSender.SendBulk"/> marks every message
    /// in the batch as <c>"sent"</c> and returns the correct count.
    /// </summary>
    [Fact]
    public async Task SendBulk_MultipleMessages_MarksAllAsSentAndReturnsCount()
    {
        var sender = new InMemoryEmailSender();
        var messages = new[]
        {
            BuildMessage("slug-1", "a@test.com"),
            BuildMessage("slug-2", "b@test.com"),
            BuildMessage("slug-3", "c@test.com")
        };

        var count = await sender.SendBulk(messages);

        Assert.Equal(3, count);
        Assert.All(messages, m => Assert.Equal("sent", m.Status));
    }

    /// <summary>
    /// Verifies that <see cref="InMemoryEmailSender.SendBulk"/> returns 0 when
    /// passed an empty collection without throwing.
    /// </summary>
    [Fact]
    public async Task SendBulk_EmptyCollection_ReturnsZero()
    {
        var sender = new InMemoryEmailSender();

        var count = await sender.SendBulk(Enumerable.Empty<Message>());

        Assert.Equal(0, count);
    }

    // ──────────────────────────────────────────────────────────────
    // Initial state
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that a new <see cref="InMemoryEmailSender"/> starts with an
    /// empty <c>Sent</c> collection.
    /// </summary>
    [Fact]
    public void NewInstance_SentCollectionIsEmpty()
    {
        var sender = new InMemoryEmailSender();

        Assert.NotNull(sender.Sent);
        Assert.Empty(sender.Sent);
    }
}
