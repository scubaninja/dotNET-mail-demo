using Xunit;

public class UnitTests
{
    [Fact]
    public void TestUnit()
    {
        // Arrange
        int expected = 5;
        int actual = 2 + 3;

        // Act & Assert
        Assert.Equal(expected, actual);
    }
}
