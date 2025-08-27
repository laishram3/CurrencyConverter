namespace CurrencyConverter.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiVersion("1.0")]
public class CurrencyController(ICurrencyService currencyService, ILogger<CurrencyController> logger, IOptions<CurrencySettings> options) : ControllerBase
{
    private readonly ICurrencyService currencyService = currencyService;
    private readonly CurrencySettings _settings = options.Value;
    private readonly ILogger<CurrencyController> _logger = logger;

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestExchangeRates(string baseCurrency = "EUR", string provider = "frankfurter")
    {
        _logger.LogWarning("GetLatestExchangeRates: {@baseCurrency}  {@provider}", baseCurrency, provider);
        var result = await currencyService.GetLatestRatesAsync(baseCurrency, provider);
        return Ok(result);
    }

    [HttpGet("convert")]
    public async Task<IActionResult> ConvertCurrency(string from, string to, decimal amount, string provider = "frankfurter")
    {
        _logger.LogWarning("Disallowed currency conversion attempt: {@From} -> {@To}", from, to);

        if (_settings.DisallowedCurrencies.Contains(from.ToUpper()) ||
            _settings.DisallowedCurrencies.Contains(to.ToUpper()))
        {
            return BadRequest($"Currency conversion involving {string.Join(", ", _settings.DisallowedCurrencies)} is not allowed.");
        }

        var result = await currencyService.ConvertAsync(from, to, amount, provider);
        return Ok(result);
    }

    [HttpGet("historical")]
    public async Task<IActionResult> GetHistoricalRates(
         string from,
         string to,
         DateTime start,
         DateTime end,
         int page = 1,
         int pageSize = 10,
         string provider = "frankfurter")
    {
        var result = await currencyService.GetHistoricalRatesAsync(from, to, start, end, page, pageSize, provider);
        return Ok(result);
    }
}

