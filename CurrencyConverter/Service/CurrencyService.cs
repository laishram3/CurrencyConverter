namespace CurrencyConverter.Service;

public class CurrencyService(ICurrencyProviderFactory providerFactory) : ICurrencyService
{
    public async Task<string> GetLatestRatesAsync(string baseCurrency, string providerName)
    {
        var currencyProvider = providerFactory.GetProvider(providerName); 
        return await currencyProvider.GetLatestRatesAsync(baseCurrency);
    }

    public async Task<string> ConvertAsync(string from, string to, decimal amount, string providerName)
    {
        var currencyProvider = providerFactory.GetProvider(providerName);
        return await currencyProvider.ConvertAsync(from, to, amount);
    }

    public async Task<string> GetHistoricalRatesAsync(string from, string to, DateTime start, DateTime end, int page, int pageSize, string providerName)
    {
        var currencyProvider = providerFactory.GetProvider(providerName);
        return await currencyProvider.GetHistoricalRatesAsync(from, to, start, end, page, pageSize);
    }
}
