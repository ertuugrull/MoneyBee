using System.Text.Json;
using MoneyBee.Transfer.Services.Interfaces;
using MoneyBee.Transfer.Services.Models;

namespace MoneyBee.Transfer.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExchangeRateService> _logger;
    private const string ClientName = "ExchangeService";

    public ExchangeRateService(IHttpClientFactory httpClientFactory, ILogger<ExchangeRateService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private HttpClient GetClient() => _httpClientFactory.CreateClient(ClientName);

    public async Task<ExchangeRateResult> GetRateAsync(string fromCurrency, string toCurrency)
    {
        if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return new ExchangeRateResult 
            { 
                Success = true, 
                Rate = 1m,
                FromCurrency = fromCurrency.ToUpperInvariant(),
                ToCurrency = toCurrency.ToUpperInvariant()
            };
        }

        try
        {
            var client = GetClient();
            var response = await client.GetAsync($"/api/exchange/rate/{fromCurrency.ToUpperInvariant()}/{toCurrency.ToUpperInvariant()}");
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ExchangeRateApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result != null)
                {
                    return new ExchangeRateResult
                    {
                        Success = true,
                        Rate = result.Rate,
                        FromCurrency = result.From,
                        ToCurrency = result.To
                    };
                }
            }

            var errorResult = JsonSerializer.Deserialize<ExchangeRateErrorResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var errorMessage = errorResult?.Message ?? errorResult?.Error ?? $"Exchange rate service error ({response.StatusCode})";
            
            _logger.LogWarning("Exchange rate service returned: {ErrorMessage}", errorMessage);
            
            return new ExchangeRateResult 
            { 
                Success = false, 
                ErrorMessage = errorMessage 
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error calling Exchange Rate service");
            return new ExchangeRateResult 
            { 
                Success = false, 
                ErrorMessage = $"Exchange rate service network error: {ex.Message}" 
            };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout calling Exchange Rate service");
            return new ExchangeRateResult 
            { 
                Success = false, 
                ErrorMessage = "Exchange rate service timeout" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Exchange Rate service");
            return new ExchangeRateResult 
            { 
                Success = false, 
                ErrorMessage = $"Exchange rate service error: {ex.Message}" 
            };
        }
    }
}
