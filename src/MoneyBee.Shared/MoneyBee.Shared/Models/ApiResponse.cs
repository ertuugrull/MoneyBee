using System.Text.Json.Serialization;

namespace MoneyBee.Shared.Models;

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    
    [JsonPropertyName("status")]
    public int Status { get; set; }

    public static ServiceResult<T> Ok(T data, string? message = null, int status = 200)
    {
        return new ServiceResult<T>
        {
            Success = true,
            Data = data,
            Message = message,
            Status = status
        };
    }

    public static ServiceResult<T> Fail(string message, int status)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            Status = status
        };
    }

    public static ServiceResult<T> Fail(List<string> errors, int status, string? message = null)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Errors = errors,
            Status = status,
            Message = message ?? (errors.Any() ? string.Join(", ", errors) : "An error occurred")
        };
    }
}

public class ApiResponse : ServiceResult<object?>
{
    public static ApiResponse Ok(string? message = null, int status = 200)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            Status = status
        };
    }

    public new static ApiResponse Fail(string message, int status)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Status = status
        };
    }
}
