using Tailwind.Mail.Api.Admin;
using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for the request/response DTO types used by the Admin API routes.
/// These verify default property values, constructors, and basic object initialisation.
/// </summary>
public class AdminDtoTests
{
    // ──────────────────────────────────────────────────────────────
    // ValidationResponse
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the <see cref="ValidationResponse"/> default constructor sets
    /// <c>Message</c> to a non-null default string.
    /// </summary>
    [Fact]
    public void ValidationResponse_DefaultConstructor_MessageIsNotNull()
    {
        var response = new ValidationResponse();

        Assert.NotNull(response.Message);
        Assert.Equal("The markdown is valid", response.Message);
    }

    /// <summary>
    /// Verifies that all properties on <see cref="ValidationResponse"/> can be set
    /// and retrieved correctly.
    /// </summary>
    [Fact]
    public void ValidationResponse_SetProperties_ValuesAreRetained()
    {
        var doc = MarkdownEmail.FromString(@"---
Subject: Test
Summary: Desc
---
body
");
        var response = new ValidationResponse
        {
            Valid    = true,
            Message  = "All good",
            Contacts = 42,
            Data     = doc
        };

        Assert.True(response.Valid);
        Assert.Equal("All good", response.Message);
        Assert.Equal(42, response.Contacts);
        Assert.Same(doc, response.Data);
    }

    // ──────────────────────────────────────────────────────────────
    // BulkTagResponse
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the <see cref="BulkTagResponse"/> default constructor
    /// initialises <c>Success</c> to <c>false</c> and <c>Message</c> to the
    /// default "No response" string.
    /// </summary>
    [Fact]
    public void BulkTagResponse_DefaultValues_AreCorrect()
    {
        var response = new BulkTagResponse();

        Assert.False(response.Success);
        Assert.Equal("No response", response.Message);
        Assert.Equal(0, response.Created);
        Assert.Equal(0, response.Updated);
    }

    /// <summary>
    /// Verifies that all properties on <see cref="BulkTagResponse"/> can be set.
    /// </summary>
    [Fact]
    public void BulkTagResponse_SetProperties_ValuesAreRetained()
    {
        var response = new BulkTagResponse
        {
            Success = true,
            Message = "Done",
            Created = 3,
            Updated = 7
        };

        Assert.True(response.Success);
        Assert.Equal("Done", response.Message);
        Assert.Equal(3, response.Created);
        Assert.Equal(7, response.Updated);
    }

    // ──────────────────────────────────────────────────────────────
    // ContactSearchResponse
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="ContactSearchResponse"/> initialises with an empty
    /// <c>Contacts</c> collection so that null-checks are unnecessary downstream.
    /// </summary>
    [Fact]
    public void ContactSearchResponse_DefaultContacts_IsEmptyEnumerable()
    {
        var response = new ContactSearchResponse();

        Assert.NotNull(response.Contacts);
        Assert.Empty(response.Contacts);
        Assert.Null(response.Term);
    }

    /// <summary>
    /// Verifies that <c>Term</c> and <c>Contacts</c> can be set on
    /// <see cref="ContactSearchResponse"/>.
    /// </summary>
    [Fact]
    public void ContactSearchResponse_SetProperties_ValuesAreRetained()
    {
        var contacts = new List<Contact> { new Contact("Alice", "alice@test.com") };
        var response = new ContactSearchResponse
        {
            Term     = "alice",
            Contacts = contacts
        };

        Assert.Equal("alice", response.Term);
        Assert.Single(response.Contacts);
    }

    // ──────────────────────────────────────────────────────────────
    // BulkTagRequest
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="BulkTagRequest"/> defaults <c>Success</c>
    /// to <c>false</c>.
    /// </summary>
    [Fact]
    public void BulkTagRequest_DefaultSuccess_IsFalse()
    {
        var request = new BulkTagRequest();

        Assert.False(request.Success);
    }

    // ──────────────────────────────────────────────────────────────
    // QueueBroadcastResponse
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="QueueBroadcastResponse"/> properties can be set.
    /// </summary>
    [Fact]
    public void QueueBroadcastResponse_SetProperties_ValuesAreRetained()
    {
        var response = new QueueBroadcastResponse
        {
            Success = true,
            Message = "Queued successfully"
        };

        Assert.True(response.Success);
        Assert.Equal("Queued successfully", response.Message);
        Assert.Null(response.Result);
    }
}
