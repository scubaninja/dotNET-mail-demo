using Tailwind.Data;
using Xunit;

namespace Tailwind.Tests;

/// <summary>Tests for the <see cref="StringExtensions.ToSnakeCase"/> utility method.</summary>
public class StringExtensionsTests
{
    [Fact]
    public void ToSnakeCase_SingleWord_ReturnsLowercase()
        => Assert.Equal("name", "Name".ToSnakeCase());

    [Fact]
    public void ToSnakeCase_PascalCase_InsertsUnderscores()
        => Assert.Equal("created_at", "CreatedAt".ToSnakeCase());

    [Fact]
    public void ToSnakeCase_ID_ReturnsId()
        => Assert.Equal("id", "ID".ToSnakeCase());

    [Fact]
    public void ToSnakeCase_SingleChar_ReturnsCharUnchanged()
        => Assert.Equal("A", "A".ToSnakeCase()); // single-char strings are returned as-is

    [Fact]
    public void ToSnakeCase_AlreadyLowercase_ReturnsSame()
        => Assert.Equal("email", "email".ToSnakeCase());

    [Theory]
    [InlineData("ContactId", "contact_id")]
    [InlineData("SendAt", "send_at")]
    [InlineData("EmailId", "email_id")]
    [InlineData("ReplyTo", "reply_to")]
    [InlineData("SendToTag", "send_to_tag")]
    public void ToSnakeCase_VariousInputs_ReturnsExpected(string input, string expected)
        => Assert.Equal(expected, input.ToSnakeCase());

    [Fact]
    public void ToSnakeCase_NullInput_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => ((string)null!).ToSnakeCase());
}
