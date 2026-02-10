using System.Text.RegularExpressions;

namespace MoneyBee.Shared.Utilities;

public static class ValidationErrorHelper
{
    public static List<string> CleanMessages(IEnumerable<string> rawMessages)
    {
        return rawMessages.Select(CleanMessage).Distinct().ToList();
    }

    public static string CleanMessage(string rawMessage)
    {
        if (string.IsNullOrWhiteSpace(rawMessage)) return "Geçersiz istek.";

        // JSON Deserialization Errors (System.Text.Json)
        if (rawMessage.Contains("could not be converted to System.Guid"))
        {
            var fieldName = ExtractFieldName(rawMessage);
            return fieldName != null 
                ? $"Geçersiz ID formatı: {fieldName}" 
                : "Geçersiz ID formatı.";
        }

        if (rawMessage.Contains("could not be converted to System.Decimal") || rawMessage.Contains("could not be converted to System.Double"))
        {
            var fieldName = ExtractFieldName(rawMessage);
            return fieldName != null 
                ? $"Geçersiz sayı formatı: {fieldName}" 
                : "Geçersiz sayı formatı.";
        }

        if (rawMessage.Contains("could not be converted to System.DateTime"))
        {
            var fieldName = ExtractFieldName(rawMessage);
            return fieldName != null 
                ? $"Geçersiz tarih formatı: {fieldName}" 
                : "Geçersiz tarih formatı.";
        }

        if (rawMessage.Contains("The request field is required") || rawMessage.Contains("A non-empty request body is required"))
        {
            return "İstek gövdesi (body) eksik veya hatalı formatta.";
        }

        if (rawMessage.Contains("Unexpected end of JSON input") || rawMessage.Contains("The JSON object contains a trailing comma"))
        {
            return "Geçersiz JSON formatı.";
        }

        return rawMessage;
    }

    private static string? ExtractFieldName(string message)
    {
        // System.Text.Json uses Path: $.fieldName
        var match = Regex.Match(message, @"Path: \$\.([a-zA-Z0-9_]+)");
        if (match.Success) return match.Groups[1].Value;

        // Sometimes it might be simple like "The {0} field is required"
        return null;
    }
}
