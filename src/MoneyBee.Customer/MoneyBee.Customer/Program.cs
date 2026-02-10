using FluentValidation;
using FluentValidation.AspNetCore;
using MoneyBee.Customer.Data;
using MoneyBee.Customer.Mapping;
using MoneyBee.Customer.Services;
using MoneyBee.Customer.Services.Interfaces;
using MoneyBee.Customer.Validators;
using MoneyBee.Shared.Middleware;
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
builder.Services.AddAutoMapper(typeof(CustomerMappingProfile));

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCustomerRequestValidator>();

builder.Services.AddSingleton<CustomerStore>();

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

builder.Services.AddHttpClient("KycService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:KycServiceUrl"] ?? "http://localhost:5011");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreakerPolicy)
.AddHeaderPropagation();

builder.Services.AddHttpClient("TransferService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:TransferServiceUrl"] ?? "http://localhost:5003");
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

builder.Services.AddScoped<IKycService, KycService>();
builder.Services.AddScoped<ITransferNotificationService, TransferNotificationService>();
builder.Services.AddScoped<MoneyBee.Shared.Services.IInternalAuthService, MoneyBee.Shared.Services.RemoteAuthService>();

builder.Services.AddScoped<ICustomerService, CustomerService>();

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
