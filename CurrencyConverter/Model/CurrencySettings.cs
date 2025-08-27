namespace CurrencyConverter.Model;

public class CurrencySettings
{       
    public string[] DisallowedCurrencies { get; set; } = new[] { "TRY", "PLN", "THB", "MXN" };
}
