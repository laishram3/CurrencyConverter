namespace CurrencyConverter.Providers;

public interface ICurrencyProviderFactory
{
    ICurrencyProvider GetProvider(string providerName);
} 
