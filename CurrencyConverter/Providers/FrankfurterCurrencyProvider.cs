
using System.Text.Json;

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
        var apiResult = await response.Content.ReadFromJsonAsync<ExchangeRateResult>(); 

        if (apiResult == null || apiResult.Rates == null || !apiResult.Rates.Any()) 
            return string.Empty;
        
        var allRates = apiResult.Rates.Select(r => new KeyValuePair<DateTime, decimal>(DateTime.Parse(r.Key), r.Value.Values.First())).OrderBy(r => r.Key).ToList(); 
        int totalRecords = allRates.Count; 
        int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize); 
        var data = allRates.Skip((page - 1) * pageSize).Take(pageSize).ToList(); 

        var pagedResult = new PagedRates { 
            Page = page, PageSize = pageSize, TotalRecords = totalRecords, TotalPages = totalPages, Data = data
        };

        return JsonSerializer.Serialize(pagedResult); 
    }
}

public class ExchangeRateResult
{
    public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; }
}

public class PagedRates
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public List<KeyValuePair<DateTime, decimal>> Data { get; set; }
}
