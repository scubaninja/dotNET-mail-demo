using Npgsql;
using Tailwind.Data;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for the <see cref="CommandExtensions"/> that build Npgsql SQL command
/// fragments — <c>Where</c>, <c>AddParams</c>, <c>Limit</c>, and <c>Order</c>.
/// These extensions operate on concrete <see cref="NpgsqlCommand"/> objects and
/// do not require a live database connection.
/// </summary>
public class NpgsqlCommandExtensionsTests
{
    // ──────────────────────────────────────────────────────────────
    // AddParams
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="CommandExtensions.AddParams"/> adds the correct
    /// parameters to the command from an anonymous object.
    /// </summary>
    [Fact]
    public void AddParams_AnonymousObject_AddsParameters()
    {
        var cmd = new NpgsqlCommand("SELECT * FROM mail.contacts");

        cmd.AddParams(new { Name = "Alice", Email = "alice@test.com" });

        Assert.Equal(2, cmd.Parameters.Count);
        Assert.NotNull(cmd.Parameters["Name"]);
        Assert.NotNull(cmd.Parameters["Email"]);
    }

    /// <summary>
    /// Verifies that <see cref="CommandExtensions.AddParams"/> returns the same
    /// command instance for fluent chaining.
    /// </summary>
    [Fact]
    public void AddParams_ReturnsSameCommandForChaining()
    {
        var cmd = new NpgsqlCommand("SELECT 1");

        var result = cmd.AddParams(new { Name = "Alice" });

        Assert.Same(cmd, result);
    }

    // ──────────────────────────────────────────────────────────────
    // Where
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="CommandExtensions.Where"/> appends a
    /// <c>WHERE col=@col</c> clause to the command text.
    /// </summary>
    [Fact]
    public void Where_SingleParameter_AppendsWhereClause()
    {
        var cmd = new NpgsqlCommand("SELECT * FROM mail.contacts");

        cmd.Where(new { Email = "alice@test.com" });

        Assert.Contains("where", cmd.CommandText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Email=@Email", cmd.CommandText);
    }

    /// <summary>
    /// Verifies that <see cref="CommandExtensions.Where"/> joins multiple parameters
    /// with AND in the WHERE clause.
    /// </summary>
    [Fact]
    public void Where_MultipleParameters_JoinsWithAnd()
    {
        var cmd = new NpgsqlCommand("SELECT * FROM mail.contacts");

        cmd.Where(new { Name = "Alice", Email = "alice@test.com" });

        Assert.Contains("and", cmd.CommandText, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that <see cref="CommandExtensions.Where"/> returns the same
    /// command instance for fluent chaining.
    /// </summary>
    [Fact]
    public void Where_ReturnsSameCommandForChaining()
    {
        var cmd = new NpgsqlCommand("SELECT 1");

        var result = cmd.Where(new { Name = "test" });

        Assert.Same(cmd, result);
    }

    // ──────────────────────────────────────────────────────────────
    // Limit
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="CommandExtensions.Limit"/> appends the
    /// correct <c>LIMIT n</c> clause to the command text.
    /// </summary>
    [Fact]
    public void Limit_AppendsLimitClause()
    {
        var cmd = new NpgsqlCommand("SELECT * FROM mail.contacts");

        cmd.Limit(10);

        Assert.Equal("SELECT * FROM mail.contacts limit 10", cmd.CommandText);
    }

    /// <summary>
    /// Verifies that <see cref="CommandExtensions.Limit"/> returns the same
    /// command instance for fluent chaining.
    /// </summary>
    [Fact]
    public void Limit_ReturnsSameCommandForChaining()
    {
        var cmd = new NpgsqlCommand("SELECT 1");

        var result = cmd.Limit(5);

        Assert.Same(cmd, result);
    }

    // ──────────────────────────────────────────────────────────────
    // Order
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="CommandExtensions.Order"/> appends the
    /// default <c>ORDER BY id asc</c> clause when no arguments are supplied.
    /// </summary>
    [Fact]
    public void Order_DefaultArgs_AppendsOrderByIdAsc()
    {
        var cmd = new NpgsqlCommand("SELECT * FROM mail.contacts");

        cmd.Order();

        Assert.Equal("SELECT * FROM mail.contacts order by id asc", cmd.CommandText);
    }

    /// <summary>
    /// Verifies that <see cref="CommandExtensions.Order"/> correctly uses the
    /// supplied column name and direction.
    /// </summary>
    [Fact]
    public void Order_CustomColumnAndDirection_AppendsCorrectClause()
    {
        var cmd = new NpgsqlCommand("SELECT * FROM mail.contacts");

        cmd.Order("created_at", "desc");

        Assert.Equal("SELECT * FROM mail.contacts order by created_at desc", cmd.CommandText);
    }

    /// <summary>
    /// Verifies that <see cref="CommandExtensions.Order"/> returns the same
    /// command instance for fluent chaining.
    /// </summary>
    [Fact]
    public void Order_ReturnsSameCommandForChaining()
    {
        var cmd = new NpgsqlCommand("SELECT 1");

        var result = cmd.Order("id", "asc");

        Assert.Same(cmd, result);
    }

    // ──────────────────────────────────────────────────────────────
    // Fluent chaining
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="CommandExtensions.Limit"/> and
    /// <see cref="CommandExtensions.Order"/> can be chained together to produce
    /// the expected combined SQL fragment.
    /// </summary>
    [Fact]
    public void Limit_And_Order_Chained_ProducesCombinedSql()
    {
        var cmd = new NpgsqlCommand("SELECT * FROM mail.contacts");

        cmd.Order("name", "asc").Limit(25);

        Assert.Equal("SELECT * FROM mail.contacts order by name asc limit 25", cmd.CommandText);
    }
}
