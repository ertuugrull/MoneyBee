using MoneyBee.Transfer.Services.Models;

namespace MoneyBee.Transfer.Services.Interfaces;

public interface IExchangeRateService
{
    Task<ExchangeRateResult> GetRateAsync(string fromCurrency, string toCurrency);
}
