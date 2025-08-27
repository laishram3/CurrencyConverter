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
    public async Task GetHistoricalRatesAsync_ReturnsPagedResult_AsString()
    {
        // Arrange
        var from = "EUR";
        var to = "USD";
        var start = new DateTime(2025, 01, 01);
        var end = new DateTime(2025, 01, 03);
        int page = 1, pageSize = 2;

        var exchangeRates = new ExchangeRateResult
        {
            Rates = new Dictionary<string, Dictionary<string, decimal>>
            {
                { "2025-01-01", new Dictionary<string, decimal> { { "USD", 1.1m } } },
                { "2025-01-02", new Dictionary<string, decimal> { { "USD", 1.2m } } },
                { "2025-01-03", new Dictionary<string, decimal> { { "USD", 1.3m } } }
            }
        };

        // Mock HttpMessageHandler to return the fake response
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(exchangeRates)
            });

        var httpClient = new HttpClient(handlerMock.Object);

        var clientFactoryMock = new Mock<IHttpClientFactory>();
        clientFactoryMock.Setup(f => f.CreateClient("FrankfurterClient"))
                         .Returns(httpClient);

        var memoryCacheMock = new Mock<IMemoryCache>();
        var optionsMock = Options.Create(new FrankfurterSettings { BaseUrl = "https://api.frankfurter.app" });

        var provider = new FrankfurterCurrencyProvider(clientFactoryMock.Object, memoryCacheMock.Object, optionsMock);

        // Act
        var result = await provider.GetHistoricalRatesAsync(from, to, start, end, page, pageSize);

        // Assert
        Assert.False(string.IsNullOrEmpty(result));

        var pagedResult = JsonSerializer.Deserialize<PagedRates>(result);
        Assert.NotNull(pagedResult);
        Assert.Equal(page, pagedResult.Page);
        Assert.Equal(pageSize, pagedResult.PageSize);
        Assert.Equal(3, pagedResult.TotalRecords);
        Assert.Equal(2, pagedResult.TotalPages); 
        Assert.Equal(2, pagedResult.Data.Count);
        Assert.Equal(1.1m, pagedResult.Data[0].Value);
        Assert.Equal(1.2m, pagedResult.Data[1].Value);
    }
}
