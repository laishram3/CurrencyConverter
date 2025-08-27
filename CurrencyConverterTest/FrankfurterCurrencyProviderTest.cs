public class FrankfurterCurrencyProviderTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly IMemoryCache _memoryCache;
    private readonly FrankfurterCurrencyProvider _provider;

    public FrankfurterCurrencyProviderTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();

        // Use real MemoryCache
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        // Options mock
        var optionsMock = Options.Create(new FrankfurterSettings { BaseUrl = "https://api.frankfurter.app" });

        _provider = new FrankfurterCurrencyProvider(_httpClientFactoryMock.Object, _memoryCache, optionsMock);
    }

    private HttpClient GetMockHttpClient(string responseContent)
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent),
            });

        var client = new HttpClient(handlerMock.Object);
        return client;
    }

    [Fact]
    public async Task GetLatestRatesAsync_ReturnsCachedValue_WhenPresent()
    {
        // Arrange
        var baseCurrency = "EUR";
        var cachedValue = "{\"USD\":1.1}";

        _memoryCache.Set($"latest_rates_{baseCurrency}", cachedValue);

        // Act
        var result = await _provider.GetLatestRatesAsync(baseCurrency);

        // Assert
        Assert.Equal(cachedValue, result);
    }

    [Fact]
    public async Task GetLatestRatesAsync_FetchesFromHttp_WhenNotCached()
    {
        // Arrange
        var responseContent = "{\"USD\":1.2}";
        var client = GetMockHttpClient(responseContent);

        _httpClientFactoryMock.Setup(f => f.CreateClient("FrankfurterClient"))
                              .Returns(client);

        // Act
        var result = await _provider.GetLatestRatesAsync("EUR");

        // Assert
        Assert.Equal(responseContent, result);
        // Ensure value is cached
        Assert.True(_memoryCache.TryGetValue("latest_rates_EUR", out string cached));
        Assert.Equal(responseContent, cached);
    }

    [Fact]
    public async Task ConvertAsync_ReturnsHttpContent()
    {
        // Arrange
        var responseContent = "11.0";
        var client = GetMockHttpClient(responseContent);

        _httpClientFactoryMock.Setup(f => f.CreateClient("FrankfurterClient"))
                              .Returns(client);

        // Act
        var result = await _provider.ConvertAsync("EUR", "USD", 10M);

        // Assert
        Assert.Equal(responseContent, result);
    }

    [Fact]
    public async Task GetHistoricalRatesAsync_ReturnsHttpContent()
    {
        // Arrange
        var responseContent = "[{\"From\":\"EUR\",\"To\":\"USD\",\"Date\":\"2025-08-27T00:00:00Z\",\"Rate\":1.1}]";
        var client = GetMockHttpClient(responseContent);

        _httpClientFactoryMock.Setup(f => f.CreateClient("FrankfurterClient"))
                              .Returns(client);

        var start = new DateTime(2025, 8, 25);
        var end = new DateTime(2025, 8, 27);

        // Act
        var result = await _provider.GetHistoricalRatesAsync("EUR", "USD", start, end, 1, 10);

        // Assert
        Assert.Equal(responseContent, result);
    }
}
