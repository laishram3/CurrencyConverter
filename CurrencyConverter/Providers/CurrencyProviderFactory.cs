namespace CurrencyConverter.Providers;

public class CurrencyProviderFactory(IServiceProvider serviceProvider) : ICurrencyProviderFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public ICurrencyProvider GetProvider(string providerName)
    {
        return providerName.Trim().ToLower() switch
        {
            "frankfurter" => _serviceProvider.GetRequiredService<FrankfurterCurrencyProvider>(),          
            _ => throw new NotSupportedException($"Provider {providerName} is not supported.")
        };
    }
}
