namespace CurrencyConverter.IntegrationTests;

public class RatesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RatesControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
       new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiMTIzNCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiZXhwIjoxODA0NjQ0MjU4LCJpc3MiOiJ0ZXN0IiwiYXVkIjoiY3VycmVuY3lfYXBpIn0.A_Ft3Ww5UtWPWa2bgy9F60VLX6--9PLfiEIY9uFHBAc");
    }

    [Fact]
    public async Task GetLatestRates_ReturnsOkAndRates()
    {
        var response = await _client.GetAsync("/api/currency/latest?baseCurrency=USD&provider=frankfurter");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var ratesResponse =  await response.Content.ReadFromJsonAsync<LatestRatesResponse>();
        Assert.Equal("USD", ratesResponse.Base);
    }

    [Theory]
    [InlineData("EUR", "USD", 10)]   
    public async Task ConvertCurrency_ReturnsOk(string from, string to, decimal amount)
    {
        var response = await _client.GetAsync($"/api/currency/convert?from={from}&to={to}&amount={amount}&provider=frankfurter");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var ratesResponse = await response.Content.ReadFromJsonAsync<LatestRatesResponse>();
        Assert.Equal("EUR", ratesResponse.Base);

    }

    [Fact]
    public async Task ConvertCurrency_DisallowedCurrency_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/currency/convert?from=TRY&to=USD&amount=10&provider=frankfurter");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var message = await response.Content.ReadAsStringAsync();
        Assert.Contains("not allowed", message);
    }

    [Fact]
    public async Task GetHistoricalRates_ReturnsOkAndData()
    {
        var start = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        var end = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var response = await _client.GetAsync($"/api/currency/historical?from=EUR&to=USD&start={start}&end={end}&page=1&pageSize=5&provider=frankfurter");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
       
    }
}

public class LatestRatesResponse
{
    public string Base { get; set; } = null!;
    public string Date { get; set; } = null!;
    public Dictionary<string, decimal> Rates { get; set; } = new();
}
