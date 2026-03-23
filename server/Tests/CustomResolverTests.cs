using Tailwind.Data;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for <see cref="CustomResolver"/>, the Dapper.SimpleCRUD column-name
/// resolver that converts C# PascalCase property names to database snake_case
/// column names.
/// </summary>
public class CustomResolverTests
{
    private readonly CustomResolver _resolver = new();

    // ──────────────────────────────────────────────────────────────
    // ResolveColumnName
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the resolver converts a standard PascalCase property name
    /// to its snake_case equivalent for use in SQL queries.
    /// </summary>
    [Theory]
    [InlineData("ContactId",  "contact_id")]
    [InlineData("SendTo",     "send_to")]
    [InlineData("CreatedAt",  "created_at")]
    [InlineData("SendFrom",   "send_from")]
    [InlineData("EmailId",    "email_id")]
    public void ResolveColumnName_PascalCase_ReturnsSnakeCase(string propertyName, string expected)
    {
        var property = typeof(TestModel).GetProperty(propertyName)!;

        var result = _resolver.ResolveColumnName(property);

        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Verifies that the resolver handles the special "ID" property name by
    /// returning the lowercase "id" column name.
    /// </summary>
    [Fact]
    public void ResolveColumnName_IdProperty_ReturnsLowercaseId()
    {
        var property = typeof(TestModel).GetProperty("ID")!;

        var result = _resolver.ResolveColumnName(property);

        Assert.Equal("id", result);
    }

    // ──────────────────────────────────────────────────────────────
    // Helper model
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Helper class with representative property names used by the resolver tests.
    /// These property names mirror the naming conventions used in the production models.
    /// </summary>
    private class TestModel
    {
        public int? ID { get; set; }
        public int? ContactId { get; set; }
        public int? EmailId { get; set; }
        public string? SendTo { get; set; }
        public string? SendFrom { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
