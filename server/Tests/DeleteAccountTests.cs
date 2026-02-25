using System.Data;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Tailwind.Data;
using Xunit;

namespace Tailwind.Mail.Tests;

// Stub IDb that throws if Connect() is called - used to verify early-exit validation paths
public class NeverConnectDb : IDb
{
  public IDbConnection Connect() => throw new InvalidOperationException("DB should not be reached in this test");
}

public class DeleteAccountTests : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly HttpClient _client;

  public DeleteAccountTests(WebApplicationFactory<Program> factory)
  {
    _client = factory.WithWebHostBuilder(builder =>
    {
      builder.UseContentRoot(Directory.GetCurrentDirectory());
      builder.ConfigureServices(services =>
      {
        // Replace real DB with a stub so these tests never require a running database
        services.AddScoped<IDb, NeverConnectDb>();
      });
    }).CreateClient();
  }

  [Fact]
  public async Task DeleteAccount_MissingEmail_ReturnsBadRequest()
  {
    // Arrange
    var request = new { email = (string?)null, confirm = true };

    // Act
    var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/admin/settings/account")
    {
      Content = JsonContent.Create(request)
    });

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    var body = await response.Content.ReadFromJsonAsync<DeleteAccountResponseDto>();
    Assert.NotNull(body);
    Assert.False(body.Success);
    Assert.Equal("Email is required", body.Message);
  }

  [Fact]
  public async Task DeleteAccount_EmptyEmail_ReturnsBadRequest()
  {
    // Arrange
    var request = new { email = "   ", confirm = true };

    // Act
    var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/admin/settings/account")
    {
      Content = JsonContent.Create(request)
    });

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    var body = await response.Content.ReadFromJsonAsync<DeleteAccountResponseDto>();
    Assert.NotNull(body);
    Assert.False(body.Success);
    Assert.Equal("Email is required", body.Message);
  }

  [Fact]
  public async Task DeleteAccount_ConfirmFalse_ReturnsBadRequest()
  {
    // Arrange
    var request = new { email = "user@example.com", confirm = false };

    // Act
    var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/admin/settings/account")
    {
      Content = JsonContent.Create(request)
    });

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    var body = await response.Content.ReadFromJsonAsync<DeleteAccountResponseDto>();
    Assert.NotNull(body);
    Assert.False(body.Success);
    Assert.Equal("You must confirm the deletion by setting confirm to true", body.Message);
  }

  // DTO for deserialising the response body in tests
  private class DeleteAccountResponseDto
  {
    public bool Success { get; set; }
    public string? Message { get; set; }
  }
}
