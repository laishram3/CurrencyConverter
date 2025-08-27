namespace CurrencyConverterTest;

public class CurrencyControllerTests
{
    private readonly Mock<ICurrencyService> _mockService;
    private readonly CurrencyController _controller;
    private readonly Mock<ILogger<CurrencyController>> _mockLogger;

    public CurrencyControllerTests()
    {
        _mockService = new Mock<ICurrencyService>();
        var settings = Options.Create(new CurrencySettings
        {            
            DisallowedCurrencies = new[] { "TRY", "PLN", "THB", "MXN" }
        });
        _mockLogger = new Mock<ILogger<CurrencyController>>();
        _controller = new CurrencyController(_mockService.Object, _mockLogger.Object, settings);
    }

    [Fact]
    public async Task GetLatestExchangeRates_ReturnsOk()
    {
        // Arrange
        var fakeRates = new Dictionary<string, decimal> { { "USD", 1.9M }, { "GBP", 0.85M } };
        var fakeRatesJson = JsonSerializer.Serialize(fakeRates);
        _mockService.Setup(s => s.GetLatestRatesAsync("EUR", "frankfurter")).ReturnsAsync(fakeRatesJson);


        // Act
        var result = await _controller.GetLatestExchangeRates("EUR", "frankfurter");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
       
        Assert.Contains("USD", okResult.Value.ToString());
    }

   
    [Fact]
    public async Task ConvertCurrency_ReturnsOk_WhenAllowed()
    {
        // Arrange
        var amountToReturn = 11M;      
        var convertedJson = System.Text.Json.JsonSerializer.Serialize(amountToReturn);

        _mockService
            .Setup(s => s.ConvertAsync("EUR", "USD", 10M, "frankfurter"))
            .ReturnsAsync(convertedJson);

        // Act
        var result = await _controller.ConvertCurrency("EUR", "USD", 10M, "frankfurter");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
     
        var converted = JsonSerializer.Deserialize<decimal>(okResult.Value.ToString()!);

        Assert.Equal(11M, converted);
    }


    [Fact]
    public async Task ConvertCurrency_ReturnsBadRequest_WhenDisallowed()
    {
        // Act
        var result = await _controller.ConvertCurrency("TRY", "USD", 10M);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("not allowed", badRequest.Value.ToString()!);
    }

    [Fact]
    public async Task GetHistoricalRates_ReturnsOk()
    {
        // Arrange
        var historical = new List<HistoricalRate>
    {
        new HistoricalRate { From = "EUR", To = "USD", Date = DateTime.UtcNow, Rate = 1.1M }
    };
        var historicalJson = System.Text.Json.JsonSerializer.Serialize(historical);

        _mockService.Setup(s => s.GetHistoricalRatesAsync(
                "EUR", "USD", It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 10, "frankfurter"))
            .ReturnsAsync(historicalJson);

        // Act
        var result = await _controller.GetHistoricalRates("EUR", "USD", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        
        var returnedJson = okResult.Value!.ToString();
        var returned = System.Text.Json.JsonSerializer.Deserialize<List<HistoricalRate>>(returnedJson!);

        Assert.NotNull(returned);
        Assert.Single(returned!);
        Assert.Equal("EUR", returned![0].From);
    }

}

public class HistoricalRate
{
    public DateTime Date { get; set; }
    public string From { get; set; } = null!;
    public string To { get; set; } = null!;
    public decimal Rate { get; set; }
}
