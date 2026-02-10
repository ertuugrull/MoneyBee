using Microsoft.AspNetCore.Mvc;
using MoneyBee.Shared.Models;
using MoneyBee.Shared.Services;

namespace MoneyBee.Auth.Controllers;

[ApiController]
[Route("api/internal/auth")]
[ApiExplorerSettings(IgnoreApi = true)]
public class InternalAuthController : ControllerBase
{
    private readonly IInternalAuthService _authService;

    public InternalAuthController(IInternalAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("validate")]
    public async Task<ActionResult<AuthValidationResponse>> Validate([FromQuery] string apiKey)
    {
        var result = await _authService.ValidateApiKeyAsync(apiKey);
        return Ok(result);
    }

    [HttpGet("rate-limit")]
    public async Task<ActionResult<RateLimitResponse>> CheckRateLimit([FromQuery] string identifier)
    {
        var result = await _authService.CheckRateLimitAsync(identifier);
        return Ok(result);
    }
}
