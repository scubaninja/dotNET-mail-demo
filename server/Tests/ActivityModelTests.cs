using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for the <see cref="Activity"/> audit-log model, verifying default
/// property values and constructor behaviour.
/// </summary>
public class ActivityModelTests
{
    // ──────────────────────────────────────────────────────────────
    // Default constructor
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that a new <see cref="Activity"/> instance auto-generates
    /// a non-empty GUID string as its <c>Key</c>.
    /// </summary>
    [Fact]
    public void DefaultConstructor_AutoGeneratesKey()
    {
        var activity = new Activity();

        Assert.False(string.IsNullOrEmpty(activity.Key));
        Assert.True(Guid.TryParse(activity.Key, out _));
    }

    /// <summary>
    /// Verifies that two independent <see cref="Activity"/> instances receive
    /// different auto-generated <c>Key</c> values.
    /// </summary>
    [Fact]
    public void DefaultConstructor_TwoInstances_HaveDifferentKeys()
    {
        var a1 = new Activity();
        var a2 = new Activity();

        Assert.NotEqual(a1.Key, a2.Key);
    }

    /// <summary>
    /// Verifies that the <c>CreatedAt</c> timestamp is automatically set to a
    /// recent UTC value when an <see cref="Activity"/> is constructed.
    /// </summary>
    [Fact]
    public void DefaultConstructor_CreatedAtIsRecentUtc()
    {
        var before = DateTimeOffset.UtcNow;
        var activity = new Activity();

        Assert.True(activity.CreatedAt >= before);
    }

    /// <summary>
    /// Verifies that property values explicitly set on an <see cref="Activity"/>
    /// are correctly stored and retrieved.
    /// </summary>
    [Fact]
    public void SetProperties_ValuesAreRetained()
    {
        var activity = new Activity
        {
            ContactId = 42,
            Key = "signup",
            Description = "New contact signed up"
        };

        Assert.Equal(42, activity.ContactId);
        Assert.Equal("signup", activity.Key);
        Assert.Equal("New contact signed up", activity.Description);
    }
}
