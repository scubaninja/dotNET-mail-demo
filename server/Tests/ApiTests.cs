using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Tailwind.Mail.Api;
using Tailwind.Data;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TestApiEndpoints()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/about");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("Tailwind Traders Mail Services API", responseString);
    }

    [Fact]
    public void TestMockApi()
    {
        // Arrange
        var mockDb = new Mock<IDb>();
        var mockApi = new Mock<PublicRoutes>();

        // Act
        mockApi.Setup(api => api.MapRoutes(It.IsAny<IEndpointRouteBuilder>()));

        // Assert
        mockApi.Verify(api => api.MapRoutes(It.IsAny<IEndpointRouteBuilder>()), Times.Once);
    }
}
