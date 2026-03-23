using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for the <see cref="Message"/> model, covering state transitions,
/// validation, and constructor behaviour.
/// </summary>
public class MessageTests
{
    // ──────────────────────────────────────────────────────────────
    // Constructors
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the parameterised constructor correctly initialises
    /// all required fields and applies the expected defaults.
    /// </summary>
    [Fact]
    public void Constructor_WithRequiredFields_SetsProperties()
    {
        var message = new Message("welcome", "user@test.com", "Welcome!", "<p>Hello</p>");

        Assert.Equal("welcome", message.Slug);
        Assert.Equal("user@test.com", message.SendTo);
        Assert.Equal("Welcome!", message.Subject);
        Assert.Equal("<p>Hello</p>", message.Html);
        Assert.Equal("pending", message.Status);
        Assert.Equal("noreply@tailwind.dev", message.SendFrom);
    }

    /// <summary>
    /// Verifies that the default (parameterless) constructor initialises
    /// <c>Status</c> to <c>"pending"</c> and <c>SendFrom</c> to the default address.
    /// </summary>
    [Fact]
    public void DefaultConstructor_SetsDefaultStatusAndSendFrom()
    {
        var message = new Message();

        Assert.Equal("pending", message.Status);
        Assert.Equal("noreply@tailwind.dev", message.SendFrom);
    }

    // ──────────────────────────────────────────────────────────────
    // Sent()
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that calling <see cref="Message.Sent"/> transitions <c>Status</c>
    /// to <c>"sent"</c> and sets <c>SentAt</c> to a recent timestamp.
    /// </summary>
    [Fact]
    public void Sent_SetsStatusToSentAndUpdatesTimestamp()
    {
        var before = DateTimeOffset.UtcNow;
        var message = new Message("slug", "to@test.com", "Subject", "<p>body</p>");

        message.Sent();

        Assert.Equal("sent", message.Status);
        Assert.True(message.SentAt >= before);
    }

    // ──────────────────────────────────────────────────────────────
    // ReadyToSend()
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="Message.ReadyToSend"/> returns <c>true</c> for a
    /// fully-populated pending message.
    /// </summary>
    [Fact]
    public void ReadyToSend_AllFieldsPresent_ReturnsTrue()
    {
        var message = new Message("slug", "to@test.com", "Subject", "<p>body</p>")
        {
            SendFrom = "from@test.com"
        };

        Assert.True(message.ReadyToSend());
    }

    /// <summary>
    /// Verifies that <see cref="Message.ReadyToSend"/> returns <c>false</c> after
    /// <see cref="Message.Sent"/> has been called, because the status is no longer
    /// <c>"pending"</c>.
    /// </summary>
    [Fact]
    public void ReadyToSend_StatusIsSent_ReturnsFalse()
    {
        var message = new Message("slug", "to@test.com", "Subject", "<p>body</p>");
        message.Sent();

        Assert.False(message.ReadyToSend());
    }

    /// <summary>
    /// Verifies that <see cref="Message.ReadyToSend"/> returns <c>false</c>
    /// when <c>SendTo</c> is empty.
    /// </summary>
    [Fact]
    public void ReadyToSend_EmptySendTo_ReturnsFalse()
    {
        var message = new Message { Status = "pending", SendFrom = "from@t.com", Subject = "S", Html = "<p>b</p>", Slug = "s", SendTo = "" };

        Assert.False(message.ReadyToSend());
    }

    /// <summary>
    /// Verifies that <see cref="Message.ReadyToSend"/> returns <c>false</c>
    /// when <c>Html</c> is empty.
    /// </summary>
    [Fact]
    public void ReadyToSend_EmptyHtml_ReturnsFalse()
    {
        var message = new Message { Status = "pending", SendFrom = "from@t.com", Subject = "S", Html = "", Slug = "s", SendTo = "to@t.com" };

        Assert.False(message.ReadyToSend());
    }

    /// <summary>
    /// Verifies that <see cref="Message.ReadyToSend"/> returns <c>false</c>
    /// when <c>Subject</c> is empty.
    /// </summary>
    [Fact]
    public void ReadyToSend_EmptySubject_ReturnsFalse()
    {
        var message = new Message { Status = "pending", SendFrom = "from@t.com", Subject = "", Html = "<p>b</p>", Slug = "s", SendTo = "to@t.com" };

        Assert.False(message.ReadyToSend());
    }

    /// <summary>
    /// Verifies that <see cref="Message.ReadyToSend"/> returns <c>false</c>
    /// when <c>SendFrom</c> is empty.
    /// </summary>
    [Fact]
    public void ReadyToSend_EmptySendFrom_ReturnsFalse()
    {
        var message = new Message { Status = "pending", SendFrom = "", Subject = "S", Html = "<p>b</p>", Slug = "s", SendTo = "to@t.com" };

        Assert.False(message.ReadyToSend());
    }
}
