using FluentValidation;
using FluentValidation.AspNetCore;
using MoneyBee.Shared.Middleware;
using MoneyBee.Transfer.BackgroundServices;
using MoneyBee.Transfer.Data;
using MoneyBee.Transfer.Mapping;
using MoneyBee.Transfer.Services;
using MoneyBee.Transfer.Services.Interfaces;
using MoneyBee.Transfer.Validators;
using Polly;
using Polly.Extensions.Http;

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

builder.Services.AddHeaderPropagation(options =>
{
    options.Headers.Add("X-Api-Key");
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(TransferMappingProfile));

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTransferRequestValidator>();

builder.Services.AddSingleton<TransferStore>();

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

builder.Services.AddHttpClient("FraudService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:FraudServiceUrl"] ?? "http://localhost:5010");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreakerPolicy)
.AddHeaderPropagation();

builder.Services.AddHttpClient("ExchangeService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ExchangeRateServiceUrl"] ?? "http://localhost:5012");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreakerPolicy)
.AddHeaderPropagation();

builder.Services.AddHttpClient("CustomerService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CustomerServiceUrl"] ?? "http://localhost:5002");
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddPolicyHandler(retryPolicy)
.AddHeaderPropagation();

builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:AuthServiceUrl"] ?? "http://localhost:5001");
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(retryPolicy)
.AddHeaderPropagation();

builder.Services.AddScoped<IFraudDetectionService, FraudDetectionService>();
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();
builder.Services.AddScoped<ICustomerVerificationService, CustomerVerificationService>();
builder.Services.AddScoped<MoneyBee.Shared.Services.IInternalAuthService, MoneyBee.Shared.Services.RemoteAuthService>();

builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddHostedService<PendingTransferProcessor>();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Global Exception Handling (must be first in pipeline)
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHeaderPropagation();

app.MapHealthChecks("/health");

app.UseMiddleware<MoneyBee.Shared.Middleware.RateLimitingMiddleware>();
app.UseMiddleware<MoneyBee.Shared.Middleware.ApiKeyAuthenticationMiddleware>();

app.MapControllers();

app.Run();
