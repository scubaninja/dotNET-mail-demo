using System.Collections.Specialized;
using System.Data;
using System.Dynamic;
using Moq;
using Tailwind.Data;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for <see cref="StringExtensions"/>, <see cref="ObjectExtensions"/>,
/// and <see cref="CommandExtensions"/> helper methods in the Data layer.
/// </summary>
public class ExtensionsTests
{
    // ──────────────────────────────────────────────────────────────
    // StringExtensions.ToSnakeCase
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that a single-character string is returned unchanged
    /// because there are no uppercase transitions to process.
    /// </summary>
    [Fact]
    public void ToSnakeCase_SingleChar_ReturnsUnchanged()
    {
        Assert.Equal("a", "a".ToSnakeCase());
    }

    /// <summary>
    /// Verifies that the special-cased "ID" string is converted to lowercase "id".
    /// </summary>
    [Fact]
    public void ToSnakeCase_ID_ReturnsLowercaseId()
    {
        Assert.Equal("id", "ID".ToSnakeCase());
    }

    /// <summary>
    /// Verifies that PascalCase input is converted to snake_case.
    /// </summary>
    [Theory]
    [InlineData("ContactId", "contact_id")]
    [InlineData("SendTo", "send_to")]
    [InlineData("CreatedAt", "created_at")]
    [InlineData("SendFrom", "send_from")]
    public void ToSnakeCase_PascalCase_ReturnsSnakeCase(string input, string expected)
    {
        Assert.Equal(expected, input.ToSnakeCase());
    }

    /// <summary>
    /// Verifies that an all-lowercase string is returned unchanged.
    /// </summary>
    [Fact]
    public void ToSnakeCase_AllLowercase_ReturnsUnchanged()
    {
        Assert.Equal("hello", "hello".ToSnakeCase());
    }

    /// <summary>
    /// Verifies that passing null throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void ToSnakeCase_Null_ThrowsArgumentNullException()
    {
        string? s = null;
        Assert.Throws<ArgumentNullException>(() => s!.ToSnakeCase());
    }

    // ──────────────────────────────────────────────────────────────
    // ObjectExtensions.ToExpando
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that an anonymous object is correctly converted to an ExpandoObject
    /// with matching property names and values.
    /// </summary>
    [Fact]
    public void ToExpando_AnonymousObject_ReturnsDynamicWithSameProperties()
    {
        var obj = new { Name = "Alice", Age = 30 };
        dynamic result = obj.ToExpando();
        var dict = (IDictionary<string, object>)result;

        Assert.True(dict.ContainsKey("Name"));
        Assert.True(dict.ContainsKey("Age"));
        Assert.Equal("Alice", dict["Name"]);
        Assert.Equal(30, dict["Age"]);
    }

    /// <summary>
    /// Verifies that passing an ExpandoObject returns the same instance
    /// without double-wrapping.
    /// </summary>
    [Fact]
    public void ToExpando_AlreadyExpando_ReturnsSameObject()
    {
        dynamic expando = new ExpandoObject();
        expando.Key = "value";
        object asObject = expando;
        dynamic result = asObject.ToExpando();

        Assert.Equal(expando, result);
    }

    /// <summary>
    /// Verifies that a <see cref="NameValueCollection"/> is correctly converted
    /// to an ExpandoObject with string values.
    /// </summary>
    [Fact]
    public void ToExpando_NameValueCollection_ReturnsDynamicWithSameKeys()
    {
        var nvc = new NameValueCollection { { "foo", "bar" }, { "baz", "qux" } };
        dynamic result = nvc.ToExpando();
        var dict = (IDictionary<string, object>)result;

        Assert.True(dict.ContainsKey("foo"));
        Assert.True(dict.ContainsKey("baz"));
    }

    // ──────────────────────────────────────────────────────────────
    // ObjectExtensions.ToDictionary
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="ObjectExtensions.ToDictionary"/> returns a dictionary
    /// with the expected keys and values from the source object.
    /// </summary>
    [Fact]
    public void ToDictionary_AnonymousObject_ReturnsDictionary()
    {
        var obj = new { Slug = "hello-world", Count = 5 };
        var dict = obj.ToDictionary();

        Assert.Equal("hello-world", dict["Slug"]);
        Assert.Equal(5, dict["Count"]);
    }

    // ──────────────────────────────────────────────────────────────
    // ObjectExtensions.ToValueList / ToSettingList / ToColumnList
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="ObjectExtensions.ToValueList"/> produces
    /// a comma-separated list of "@PropertyName" tokens.
    /// </summary>
    [Fact]
    public void ToValueList_Object_ReturnsAtPrefixedCsv()
    {
        var obj = new { Name = "Alice", Email = "alice@test.com" };
        var result = obj.ToValueList();

        Assert.Contains("@Name", result);
        Assert.Contains("@Email", result);
    }

    /// <summary>
    /// Verifies that <see cref="ObjectExtensions.ToSettingList"/> produces
    /// "Key=@Key" assignment pairs joined by commas.
    /// </summary>
    [Fact]
    public void ToSettingList_Object_ReturnsKeyEqAtKeyCsv()
    {
        var obj = new { Name = "Alice" };
        var result = obj.ToSettingList();

        Assert.Equal("Name=@Name", result);
    }

    /// <summary>
    /// Verifies that <see cref="ObjectExtensions.ToColumnList"/> produces
    /// a plain comma-separated list of property names.
    /// </summary>
    [Fact]
    public void ToColumnList_Object_ReturnsPlainCsv()
    {
        var obj = new { Name = "Alice", Email = "alice@test.com" };
        var result = obj.ToColumnList();

        Assert.Contains("Name", result);
        Assert.Contains("Email", result);
        Assert.DoesNotContain("@", result);
    }

    // ──────────────────────────────────────────────────────────────
    // CommandExtensions.ToExpandoList / RecordToExpando
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="CommandExtensions.ToExpandoList"/> reads all rows
    /// from an <see cref="IDataReader"/> and returns them as a list of ExpandoObjects.
    /// </summary>
    [Fact]
    public void ToExpandoList_ReaderWithRows_ReturnsExpandoList()
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(int));
        table.Columns.Add("name", typeof(string));
        table.Rows.Add(1, "Alice");
        table.Rows.Add(2, "Bob");

        using var reader = table.CreateDataReader();
        var result = reader.ToExpandoList();

        Assert.Equal(2, result.Count);
        var first = (IDictionary<string, object>)result[0];
        Assert.Equal(1, first["id"]);
        Assert.Equal("Alice", first["name"]);
    }

    /// <summary>
    /// Verifies that <see cref="CommandExtensions.RecordToExpando"/> converts a
    /// single data reader row into a dynamic object with DBNull values mapped to null.
    /// </summary>
    [Fact]
    public void RecordToExpando_DBNullField_MapsToNull()
    {
        var table = new DataTable();
        table.Columns.Add("email", typeof(string));
        table.Columns.Add("description", typeof(string));
        table.Rows.Add("test@example.com", DBNull.Value);

        using var reader = table.CreateDataReader();
        reader.Read();
        dynamic result = reader.RecordToExpando();
        var dict = (IDictionary<string, object>)result;

        Assert.Equal("test@example.com", dict["email"]);
        Assert.Null(dict["description"]);
    }
}
