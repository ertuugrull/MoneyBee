using MoneyBee.Auth.Data;
using MoneyBee.Auth.Services;
using MoneyBee.Shared.Middleware;
using MoneyBee.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Custom validation error response
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var rawErrors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => !string.IsNullOrEmpty(e.ErrorMessage) ? e.ErrorMessage : e.Exception?.Message ?? "Geçersiz değer")
            .ToList();

        var cleanErrors = MoneyBee.Shared.Utilities.ValidationErrorHelper.CleanMessages(rawErrors);
        var message = cleanErrors.Count > 1 ? "Birden fazla geçersiz giriş tespit edildi." : cleanErrors.FirstOrDefault() ?? "Doğrulama hatası.";
        
        var response = MoneyBee.Shared.Models.ApiResponse.Fail(cleanErrors, 400, message);
        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ApiKeyStore>();
builder.Services.AddSingleton<RateLimitStore>();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<IInternalAuthService, InternalAuthService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Global Exception Handling (must be first in pipeline)
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/health");

app.UseMiddleware<MoneyBee.Shared.Middleware.RateLimitingMiddleware>();
app.UseMiddleware<MoneyBee.Shared.Middleware.ApiKeyAuthenticationMiddleware>();

app.MapControllers();

app.Run();
