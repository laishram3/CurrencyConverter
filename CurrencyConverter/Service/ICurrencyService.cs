namespace CurrencyConverter.Service;
public interface ICurrencyService
{
    Task<string> GetLatestRatesAsync(string baseCurrency, string providerName);
    Task<string> ConvertAsync(string from, string to, decimal amount, string providerName);
    Task<string> GetHistoricalRatesAsync(string from, string to, DateTime start, DateTime end, int page, int pageSize, string providerName);
}
