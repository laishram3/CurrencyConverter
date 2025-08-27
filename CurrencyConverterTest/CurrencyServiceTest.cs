

namespace CurrencyServiceTests;

public class CurrencyServiceTests
{
    private readonly Mock<ICurrencyProviderFactory> _providerFactoryMock;
    private readonly Mock<ICurrencyProvider> _providerMock;
    private readonly CurrencyService _currencyService;

    public CurrencyServiceTests()
    {
        _providerFactoryMock = new Mock<ICurrencyProviderFactory>();
        _providerMock = new Mock<ICurrencyProvider>();

        
        _providerFactoryMock
            .Setup(f => f.GetProvider(It.IsAny<string>()))
            .Returns(_providerMock.Object);

        _currencyService = new CurrencyService(_providerFactoryMock.Object);
    }

    [Fact]
    public async Task GetLatestRatesAsync_ReturnsExpectedResult()
    {
        // Arrange
        var expected = "RatesJson";
        _providerMock
            .Setup(p => p.GetLatestRatesAsync("USD"))
            .ReturnsAsync(expected);

        // Act
        var result = await _currencyService.GetLatestRatesAsync("USD", "SomeProvider");

        // Assert
        Assert.Equal(expected, result);
        _providerFactoryMock.Verify(f => f.GetProvider("SomeProvider"), Times.Once);
        _providerMock.Verify(p => p.GetLatestRatesAsync("USD"), Times.Once);
    }

    [Fact]
    public async Task ConvertAsync_ReturnsExpectedResult()
    {
        // Arrange
        var expected = "ConversionJson";
        _providerMock
            .Setup(p => p.ConvertAsync("USD", "EUR", 100m))
            .ReturnsAsync(expected);

        // Act
        var result = await _currencyService.ConvertAsync("USD", "EUR", 100m, "SomeProvider");

        // Assert
        Assert.Equal(expected, result);
        _providerFactoryMock.Verify(f => f.GetProvider("SomeProvider"), Times.Once);
        _providerMock.Verify(p => p.ConvertAsync("USD", "EUR", 100m), Times.Once);
    }

    [Fact]
    public async Task GetHistoricalRatesAsync_ReturnsExpectedResult()
    {
        // Arrange
        var expected = "HistoricalJson";
        var start = DateTime.UtcNow.AddDays(-5);
        var end = DateTime.UtcNow;
        _providerMock
            .Setup(p => p.GetHistoricalRatesAsync("USD", "EUR", start, end, 1, 10))
            .ReturnsAsync(expected);

        // Act
        var result = await _currencyService.GetHistoricalRatesAsync("USD", "EUR", start, end, 1, 10, "SomeProvider");

        // Assert
        Assert.Equal(expected, result);
        _providerFactoryMock.Verify(f => f.GetProvider("SomeProvider"), Times.Once);
        _providerMock.Verify(p => p.GetHistoricalRatesAsync("USD", "EUR", start, end, 1, 10), Times.Once);
    }
}
