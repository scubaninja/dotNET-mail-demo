using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Xunit;

public class MockApiTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;

    public MockApiTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
    }

    [Fact]
    public async Task Test_GetEndpoint_ReturnsExpectedResponse()
    {
        // Arrange
        var expectedResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("{\"message\":\"Hello, World!\"}")
        };

        _mockHttpMessageHandler
            .Setup(handler => handler.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _httpClient.GetAsync("https://api.example.com/endpoint");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal("{\"message\":\"Hello, World!\"}", content);
    }

    [Fact]
    public async Task Test_PostEndpoint_ReturnsExpectedResponse()
    {
        // Arrange
        var expectedResponse = new HttpResponseMessage(System.Net.HttpStatusCode.Created)
        {
            Content = new StringContent("{\"id\":1,\"message\":\"Created\"}")
        };

        _mockHttpMessageHandler
            .Setup(handler => handler.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var postData = new StringContent("{\"name\":\"Test\"}");

        // Act
        var response = await _httpClient.PostAsync("https://api.example.com/endpoint", postData);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal("{\"id\":1,\"message\":\"Created\"}", content);
    }
}
