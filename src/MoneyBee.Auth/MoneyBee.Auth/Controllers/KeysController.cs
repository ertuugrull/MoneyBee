using Microsoft.AspNetCore.Mvc;
using MoneyBee.Auth.Models;
using MoneyBee.Auth.Services;
using MoneyBee.Shared.Exceptions;
using MoneyBee.Shared.Models;

namespace MoneyBee.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KeysController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;

    public KeysController(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    [HttpPost]
    public ActionResult<ServiceResult<ApiKeyResponse>> CreateKey([FromBody] CreateApiKeyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Name is required");
        }

        var apiKey = _apiKeyService.CreateKey(request.Name, request.ExpiresInDays);
        var response = new ApiKeyResponse
        {
            Id = apiKey.Id,
            Key = apiKey.Key,
            Name = apiKey.Name,
            CreatedAt = apiKey.CreatedAt,
            ExpiresAt = apiKey.ExpiresAt
        };

        return Ok(ServiceResult<ApiKeyResponse>.Ok(response, "API Key created successfully"));
    }

    [HttpGet]
    public ActionResult<ServiceResult<IEnumerable<ApiKeyResponse>>> GetAllKeys()
    {
        var keys = _apiKeyService.GetAllKeys().Select(k => new ApiKeyResponse
        {
            Id = k.Id,
            Key = MaskApiKey(k.Key),
            Name = k.Name,
            CreatedAt = k.CreatedAt,
            ExpiresAt = k.ExpiresAt
        });

        return Ok(ServiceResult<IEnumerable<ApiKeyResponse>>.Ok(keys));
    }

    [HttpDelete("{id:guid}")]
    public ActionResult<ApiResponse> RevokeKey(Guid id)
    {
        var result = _apiKeyService.RevokeKey(id);
        if (!result)
        {
            throw new NotFoundException("API Key", id);
        }

        return Ok(ApiResponse.Ok("API Key revoked successfully"));
    }

    private static string MaskApiKey(string key)
    {
        if (key.Length <= 8)
            return new string('*', key.Length);
        return key[..4] + new string('*', key.Length - 8) + key[^4..];
    }
}
