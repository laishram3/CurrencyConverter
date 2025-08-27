
namespace CurrencyConverter.Providers;

public class FrankfurterCurrencyProvider : ICurrencyProvider
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IMemoryCache _cache;
    private readonly string _baseUrl;

    public FrankfurterCurrencyProvider(IHttpClientFactory clientFactory, IMemoryCache cache, IOptions<FrankfurterSettings> options)
    {
        _clientFactory = clientFactory;
        _cache = cache;
        _baseUrl = options.Value.BaseUrl;
    }

    public async Task<string> GetLatestRatesAsync(string baseCurrency)
    {
        string cacheKey = $"latest_rates_{baseCurrency}";  

        if (_cache.TryGetValue(cacheKey, out string cachedRates))
        {
            return cachedRates;  
        }

        var client = _clientFactory.CreateClient("FrankfurterClient");
        var response = await client.GetAsync($"{_baseUrl}/latest?base={baseCurrency}");
        var result = await response.Content.ReadAsStringAsync();

       
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }

   
    public async Task<string> ConvertAsync(string from, string to, decimal amount)
    {
        var client = _clientFactory.CreateClient("FrankfurterClient");
        var response = await client.GetAsync($"{_baseUrl}/latest?amount={amount}&from={from}&to={to}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

   
    public async Task<string> GetHistoricalRatesAsync(string from, string to, DateTime start, DateTime end, int page, int pageSize)
    {
        var client = _clientFactory.CreateClient("FrankfurterClient");
        var response = await client.GetAsync($"{_baseUrl}/{start:yyyy-MM-dd}..{end:yyyy-MM-dd}?from={from}&to={to}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
