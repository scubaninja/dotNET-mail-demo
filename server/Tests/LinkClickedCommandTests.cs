using Tailwind.Mail.Commands;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for <see cref="LinkClickedCommand"/>, which records a link-click event
/// and returns a formatted confirmation string.
/// </summary>
public class LinkClickedCommandTests
{
    // ──────────────────────────────────────────────────────────────
    // Execute
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="LinkClickedCommand.Execute"/> returns the expected
    /// "Link clicked: {key}" format string for a standard key value.
    /// </summary>
    [Fact]
    public void Execute_WithKey_ReturnsFormattedString()
    {
        var command = new LinkClickedCommand("abc-123");

        var result = command.Execute();

        Assert.Equal("Link clicked: abc-123", result);
    }

    /// <summary>
    /// Verifies that <see cref="LinkClickedCommand.Execute"/> correctly embeds a
    /// GUID-formatted key in the result string.
    /// </summary>
    [Fact]
    public void Execute_WithGuidKey_ReturnsFormattedString()
    {
        var key = Guid.NewGuid().ToString();
        var command = new LinkClickedCommand(key);

        var result = command.Execute();

        Assert.Equal($"Link clicked: {key}", result);
    }

    /// <summary>
    /// Verifies that the <c>Key</c> property exposed by <see cref="LinkClickedCommand"/>
    /// stores the value that was passed to the constructor.
    /// </summary>
    [Fact]
    public void Constructor_SetsKeyProperty()
    {
        var command = new LinkClickedCommand("my-link-key");

        Assert.Equal("my-link-key", command.Key);
    }

    /// <summary>
    /// Verifies that <see cref="LinkClickedCommand.Execute"/> handles an empty
    /// string key without throwing, producing a result with an empty segment.
    /// </summary>
    [Fact]
    public void Execute_EmptyKey_ReturnsFormattedStringWithEmptySegment()
    {
        var command = new LinkClickedCommand(string.Empty);

        var result = command.Execute();

        Assert.Equal("Link clicked: ", result);
    }
}
