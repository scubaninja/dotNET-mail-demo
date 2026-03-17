using System;
using Xunit;
using Tailwind.Data;

namespace Tailwind.Mail.Tests;

public class StringExtensionsTests
{
    [Fact]
    public void ToSnakeCase_WithCamelCase_ReturnsSnakeCase()
    {
        var result = "CreatedAt".ToSnakeCase();

        Assert.Equal("created_at", result);
    }

    [Fact]
    public void ToSnakeCase_WithID_ReturnsId()
    {
        var result = "ID".ToSnakeCase();

        Assert.Equal("id", result);
    }

    [Fact]
    public void ToSnakeCase_WithSingleCharacter_ReturnsSameCharacter()
    {
        var result = "A".ToSnakeCase();

        Assert.Equal("A", result);
    }

    [Fact]
    public void ToSnakeCase_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((string)null!).ToSnakeCase());
    }

    [Fact]
    public void ToSnakeCase_WithLowercaseWord_ReturnsSame()
    {
        var result = "name".ToSnakeCase();

        Assert.Equal("name", result);
    }

    [Fact]
    public void ToSnakeCase_WithPascalCase_ReturnsSnakeCase()
    {
        var result = "EmailId".ToSnakeCase();

        Assert.Equal("email_id", result);
    }

    [Fact]
    public void ToSnakeCase_WithMultipleWords_ReturnsSnakeCase()
    {
        var result = "ContactOptOut".ToSnakeCase();

        Assert.Equal("contact_opt_out", result);
    }

    [Fact]
    public void ToSnakeCase_WithAlreadySnakeCase_ReturnsSame()
    {
        var result = "subject".ToSnakeCase();

        Assert.Equal("subject", result);
    }

    [Fact]
    public void ToSnakeCase_WithSingleUppercaseChar_ReturnsLowercase()
    {
        var result = "S".ToSnakeCase();

        Assert.Equal("S", result);
    }
}
