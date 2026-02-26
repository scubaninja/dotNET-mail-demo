using System.Data;
using System.Net;
using System.Net.Http.Json;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Tailwind.Data;
using Tailwind.Mail.Api.Admin;
using Tailwind.Mail.Models;
using Xunit;

namespace Tailwind.Mail.Tests;

public class DeleteContactEndpointTests
{
  [Fact]
  public async Task DeleteContact_ExistingContact_Returns200WithSuccess()
  {
    // Arrange
    var contact = new Contact { ID = 1, Name = "Jane Doe", Email = "jane@example.com" };
    var factory = BuildFactory(contactData: contact);
    var client = factory.CreateClient();

    // Act
    var response = await client.DeleteAsync("/admin/contacts/1");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var body = await response.Content.ReadFromJsonAsync<DeleteContactResponse>();
    Assert.NotNull(body);
    Assert.True(body!.Success);
  }

  [Fact]
  public async Task DeleteContact_NonExistentContact_Returns404()
  {
    // Arrange
    var factory = BuildFactory(contactData: null);
    var client = factory.CreateClient();

    // Act
    var response = await client.DeleteAsync("/admin/contacts/999");

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  // --- helpers ---

  private static WebApplicationFactory<Program> BuildFactory(Contact? contactData)
  {
    var serverRoot = FindProjectRoot();

    return new WebApplicationFactory<Program>()
      .WithWebHostBuilder(host =>
      {
        host.UseContentRoot(serverRoot);
        host.ConfigureServices(services =>
        {
          // Replace the real IDb with a fake that controls what the command sees
          services.AddScoped<IDb>(_ => new FakeDb(contactData));
        });
      });
  }

  // Walk up the directory tree from the test binary to find the project root
  private static string FindProjectRoot()
  {
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null)
    {
      if (dir.GetFiles("Tailwind.Mail.csproj").Any())
        return dir.FullName;
      dir = dir.Parent;
    }
    throw new DirectoryNotFoundException("Could not find project root containing Tailwind.Mail.csproj");
  }

  // A test-double IDb that returns a pre-configured IDbConnection
  private sealed class FakeDb : IDb
  {
    private readonly Contact? _contact;

    public FakeDb(Contact? contact) => _contact = contact;

    public IDbConnection Connect()
    {
      Dapper.SimpleCRUD.SetDialect(Dapper.SimpleCRUD.Dialect.PostgreSQL);
      SimpleCRUD.SetColumnNameResolver(new CustomResolver());

      var mockConn = new Mock<IDbConnection>();
      var mockTx = new Mock<IDbTransaction>();

      mockConn.Setup(c => c.BeginTransaction()).Returns(mockTx.Object);
      mockConn.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(mockTx.Object);
      mockConn.SetupGet(c => c.State).Returns(ConnectionState.Open);

      var selectCmd = BuildReaderCommand(_contact != null
        ? BuildContactTable(_contact)
        : new DataTable());
      var deleteCmd = BuildNonQueryCommand();

      var cmdQueue = new Queue<IDbCommand>(new IDbCommand[]
      {
        selectCmd.Object,
        deleteCmd.Object, deleteCmd.Object,
        deleteCmd.Object, deleteCmd.Object,
      });
      mockConn.Setup(c => c.CreateCommand())
        .Returns(() => cmdQueue.Count > 0 ? cmdQueue.Dequeue() : deleteCmd.Object);

      return mockConn.Object;
    }
  }

  private static Mock<IDbCommand> BuildReaderCommand(DataTable table)
  {
    var mockCmd = new Mock<IDbCommand>();
    var mockParams = new Mock<IDataParameterCollection>();
    var mockParam = new Mock<IDbDataParameter>();

    mockCmd.SetupAllProperties();
    mockCmd.SetupGet(c => c.Parameters).Returns(mockParams.Object);
    mockCmd.Setup(c => c.CreateParameter()).Returns(mockParam.Object);
    mockParam.SetupAllProperties();
    mockParams.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

    mockCmd.Setup(c => c.ExecuteReader()).Returns(() => new DataTableReader(table));
    mockCmd.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
      .Returns(() => new DataTableReader(table));
    mockCmd.Setup(c => c.ExecuteNonQuery()).Returns(0);

    return mockCmd;
  }

  private static Mock<IDbCommand> BuildNonQueryCommand()
  {
    var mockCmd = new Mock<IDbCommand>();
    var mockParams = new Mock<IDataParameterCollection>();
    var mockParam = new Mock<IDbDataParameter>();

    mockCmd.SetupAllProperties();
    mockCmd.SetupGet(c => c.Parameters).Returns(mockParams.Object);
    mockCmd.Setup(c => c.CreateParameter()).Returns(mockParam.Object);
    mockParam.SetupAllProperties();
    mockParams.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

    mockCmd.Setup(c => c.ExecuteNonQuery()).Returns(1);
    var empty = new DataTable();
    mockCmd.Setup(c => c.ExecuteReader()).Returns(() => new DataTableReader(empty));
    mockCmd.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
      .Returns(() => new DataTableReader(empty));

    return mockCmd;
  }

  private static DataTable BuildContactTable(Contact contact)
  {
    var table = new DataTable();
    table.Columns.Add("id", typeof(int));
    table.Columns.Add("email", typeof(string));
    table.Columns.Add("name", typeof(string));
    table.Columns.Add("subscribed", typeof(bool));
    table.Columns.Add("key", typeof(string));
    table.Columns.Add("created_at", typeof(DateTimeOffset));

    table.Rows.Add(
      contact.ID ?? 0,
      contact.Email ?? string.Empty,
      contact.Name ?? string.Empty,
      contact.Subscribed,
      contact.Key,
      contact.CreatedAt == default ? DateTimeOffset.UtcNow : contact.CreatedAt
    );
    return table;
  }
}
