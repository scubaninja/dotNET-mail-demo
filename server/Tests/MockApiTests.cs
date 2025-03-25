using Xunit;
using Moq;
using Tailwind.Data;
using Tailwind.Mail.Api;

public class MockApiTests
{
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
