using Microsoft.AspNetCore.Http;
using MoneyBee.Shared.Models;
using MoneyBee.Shared.Services;

namespace MoneyBee.Shared.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;

    public RateLimitingMiddleware(RequestDelegate next)
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

        var identifier = context.Request.Headers["X-Api-Key"].ToString();
        if (string.IsNullOrEmpty(identifier))
        {
            identifier = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        var rateLimitResult = await authService.CheckRateLimitAsync(identifier);
        
        context.Response.Headers["X-RateLimit-Remaining"] = rateLimitResult.Remaining.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = rateLimitResult.ResetTime.ToString("O");

        if (!rateLimitResult.IsAllowed)
        {
            var statusCode = 429;
            context.Response.StatusCode = statusCode;
            
            var response = ApiResponse.Fail("Rate limit exceeded. Maximum 100 requests per minute.", statusCode);
            
            await context.Response.WriteAsJsonAsync(response, new System.Text.Json.JsonSerializerOptions 
            { 
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase 
            });
            return;
        }

        await _next(context);
    }
}
