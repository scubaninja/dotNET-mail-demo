using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for the <see cref="Contact"/> and <see cref="Tag"/> model constructors
/// and default property values.
/// </summary>
public class ContactModelTests
{
    // ──────────────────────────────────────────────────────────────
    // Contact
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the parameterised <see cref="Contact"/> constructor sets
    /// <c>Name</c> and <c>Email</c> from the supplied arguments.
    /// </summary>
    [Fact]
    public void Contact_ParameterisedConstructor_SetsNameAndEmail()
    {
        var contact = new Contact("Alice", "alice@example.com");

        Assert.Equal("Alice", contact.Name);
        Assert.Equal("alice@example.com", contact.Email);
    }

    /// <summary>
    /// Verifies that each new <see cref="Contact"/> instance automatically receives
    /// a non-empty GUID as its <c>Key</c>.
    /// </summary>
    [Fact]
    public void Contact_NewInstance_HasNonEmptyKey()
    {
        var contact = new Contact("Bob", "bob@example.com");

        Assert.False(string.IsNullOrEmpty(contact.Key));
        Assert.True(Guid.TryParse(contact.Key, out _));
    }

    /// <summary>
    /// Verifies that two distinct <see cref="Contact"/> instances each receive a unique
    /// GUID key so that accidental key collisions cannot occur.
    /// </summary>
    [Fact]
    public void Contact_TwoInstances_HaveDifferentKeys()
    {
        var c1 = new Contact("Alice", "alice@example.com");
        var c2 = new Contact("Bob", "bob@example.com");

        Assert.NotEqual(c1.Key, c2.Key);
    }

    /// <summary>
    /// Verifies that the default (parameterless) <see cref="Contact"/> constructor
    /// produces an instance without throwing and with a non-empty key.
    /// </summary>
    [Fact]
    public void Contact_DefaultConstructor_HasNonEmptyKey()
    {
        var contact = new Contact();

        Assert.False(string.IsNullOrEmpty(contact.Key));
    }

    // ──────────────────────────────────────────────────────────────
    // Tag
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the <see cref="Tag"/> constructor auto-generates a
    /// lowercase, hyphenated slug from the supplied name.
    /// </summary>
    [Fact]
    public void Tag_Constructor_GeneratesSlugFromName()
    {
        var tag = new Tag("VIP Customers");

        Assert.Equal("VIP Customers", tag.Name);
        Assert.Equal("vip-customers", tag.Slug);
    }

    /// <summary>
    /// Verifies that the default (parameterless) <see cref="Tag"/> constructor
    /// produces a valid instance with null Name and Slug.
    /// </summary>
    [Fact]
    public void Tag_DefaultConstructor_NullNameAndSlug()
    {
        var tag = new Tag();

        Assert.Null(tag.Name);
        Assert.Null(tag.Slug);
    }
}
