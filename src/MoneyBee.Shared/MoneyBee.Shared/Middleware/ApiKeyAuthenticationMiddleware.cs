using Microsoft.AspNetCore.Http;
using MoneyBee.Shared.Models;
using MoneyBee.Shared.Services;

namespace MoneyBee.Shared.Middleware;

public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-Api-Key";

    public ApiKeyAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IInternalAuthService authService)
    {
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/api/internal/auth"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            var statusCode = 401;
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail("API Key is required", statusCode), new System.Text.Json.JsonSerializerOptions 
            { 
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase 
            });
            return;
        }

        var authResult = await authService.ValidateApiKeyAsync(extractedApiKey!);
        if (!authResult.IsValid)
        {
            var statusCode = 401;
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail("Invalid API Key", statusCode), new System.Text.Json.JsonSerializerOptions 
            { 
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase 
            });
            return;
        }

        context.Items["ApiKey"] = authResult;
        await _next(context);
    }
}
