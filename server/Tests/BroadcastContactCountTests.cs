using System.Data;
using Moq;
using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

/// <summary>
/// Tests for <see cref="Broadcast.ContactCount"/>, which queries the database
/// for the number of subscribed contacts matching the broadcast's target criteria.
/// All database interactions are mocked so no live database is required.
/// </summary>
public class BroadcastContactCountTests
{
    // ──────────────────────────────────────────────────────────────
    // ContactCount – null connection guard
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that passing a null <see cref="IDbConnection"/> to
    /// <see cref="Broadcast.ContactCount"/> throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void ContactCount_NullConnection_ThrowsArgumentNullException()
    {
        var broadcast = Broadcast.FromMarkdown(@"---
Subject: Test
Summary: test
---
body
");

        Assert.Throws<ArgumentNullException>(() => broadcast.ContactCount(null!));
    }

    // ──────────────────────────────────────────────────────────────
    // ContactCount – wildcard (send to all)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that when <c>SendToTag</c> is <c>"*"</c>, <see cref="Broadcast.ContactCount"/>
    /// executes a scalar COUNT query that returns the mocked row count.
    /// </summary>
    [Fact]
    public void ContactCount_WildcardTag_ReturnsScalarResult()
    {
        var broadcast = Broadcast.FromMarkdown(@"---
Subject: All Subscribers
Summary: For everyone
---
body
");

        // broadcast.SendToTag defaults to "*"
        var connMock = new Mock<IDbConnection>();
        SetupScalarCommand(connMock, 42L);

        var count = broadcast.ContactCount(connMock.Object);

        Assert.Equal(42L, count);
    }

    // ──────────────────────────────────────────────────────────────
    // ContactCount – specific tag
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that when <c>SendToTag</c> is a specific slug, the query
    /// returns the mocked count of contacts with that tag.
    /// </summary>
    [Fact]
    public void ContactCount_SpecificTag_ReturnsScalarResult()
    {
        var broadcast = Broadcast.FromMarkdown(@"---
Subject: VIP Update
Summary: VIP only
SendToTag: vip
---
body
");

        var connMock = new Mock<IDbConnection>();
        SetupScalarCommand(connMock, 7L);

        var count = broadcast.ContactCount(connMock.Object);

        Assert.Equal(7L, count);
    }

    // ──────────────────────────────────────────────────────────────
    // ContactCount – DB exception
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that when the database query throws, <see cref="Broadcast.ContactCount"/>
    /// wraps the original exception in an <see cref="ApplicationException"/>.
    /// </summary>
    [Fact]
    public void ContactCount_DbThrows_WrapsInApplicationException()
    {
        var broadcast = Broadcast.FromMarkdown(@"---
Subject: Test
Summary: test
---
body
");

        var connMock = new Mock<IDbConnection>();
        connMock.Setup(c => c.CreateCommand()).Throws(new InvalidOperationException("DB error"));

        var ex = Assert.Throws<ApplicationException>(() => broadcast.ContactCount(connMock.Object));

        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    // ──────────────────────────────────────────────────────────────
    // Private helpers
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Sets up the mock <see cref="IDbConnection"/> so that <c>CreateCommand</c>
    /// returns a command whose <c>ExecuteScalar</c> returns the given value.
    /// </summary>
    private static void SetupScalarCommand(Mock<IDbConnection> connMock, object scalarValue)
    {
        var paramsMock = new Mock<IDataParameterCollection>();
        paramsMock.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

        var cmdMock = new Mock<IDbCommand>();
        cmdMock.SetupAllProperties();
        cmdMock.Setup(c => c.Parameters).Returns(paramsMock.Object);
        cmdMock.Setup(c => c.CreateParameter()).Returns(new Mock<IDbDataParameter>().Object);
        cmdMock.Setup(c => c.ExecuteScalar()).Returns(scalarValue);

        connMock.Setup(c => c.CreateCommand()).Returns(cmdMock.Object);
    }
}
