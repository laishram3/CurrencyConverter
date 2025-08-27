namespace CurrencyConverter.Providers;

public interface ICurrencyProvider
{
    Task<string> GetLatestRatesAsync(string baseCurrency);
    Task<string> ConvertAsync(string from, string to, decimal amount);
    Task<string> GetHistoricalRatesAsync(string from, string to, DateTime start, DateTime end, int page, int pageSize);
}
