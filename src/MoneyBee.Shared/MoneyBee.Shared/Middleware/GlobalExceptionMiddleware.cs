using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MoneyBee.Shared.Models;

namespace MoneyBee.Shared.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exceptions.ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (Exceptions.NotFoundException ex)
        {
            await HandleNotFoundExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);
            await HandleGenericExceptionAsync(context);
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, Exceptions.ValidationException ex)
    {
        var statusCode = (int)HttpStatusCode.BadRequest;
        context.Response.StatusCode = statusCode;
        
        var cleanErrors = Utilities.ValidationErrorHelper.CleanMessages(ex.Errors);
        var response = ServiceResult<object?>.Fail(cleanErrors, statusCode, ex.Message);
        
        await WriteResponseAsync(context, response);
    }

    private static async Task HandleNotFoundExceptionAsync(HttpContext context, Exceptions.NotFoundException ex)
    {
        var status = (int)HttpStatusCode.NotFound;
        context.Response.StatusCode = status;
        var response = ServiceResult<object?>.Fail(ex.Message, status);
        await WriteResponseAsync(context, response);
    }

    private static async Task HandleGenericExceptionAsync(HttpContext context)
    {
        var status = (int)HttpStatusCode.InternalServerError;
        context.Response.StatusCode = status;
        var response = ServiceResult<object?>.Fail("Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.", status);
        await WriteResponseAsync(context, response);
    }

    private static async Task WriteResponseAsync(HttpContext context, object response)
    {
        context.Response.ContentType = "application/json";
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
        await context.Response.WriteAsync(json);
    }
}
