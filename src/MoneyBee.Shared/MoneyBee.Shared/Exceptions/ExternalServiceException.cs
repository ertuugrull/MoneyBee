namespace MoneyBee.Shared.Exceptions;

public class ExternalServiceException : Exception
{
    public string ServiceName { get; }

    public ExternalServiceException(string serviceName, string message) 
        : base($"External service '{serviceName}' error: {message}")
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string message, Exception innerException) 
        : base($"External service '{serviceName}' error: {message}", innerException)
    {
        ServiceName = serviceName;
    }
}
