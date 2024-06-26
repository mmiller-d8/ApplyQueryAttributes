using System;
using System.Text.Json;

namespace D8.Core.Extensions;

public static class JsonSerializerExtensions
{
    private static readonly JsonSerializerOptions _camelCaseJsonSerializer = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    
    public static T? Deserialize<T>(this string json, JsonSerializerOptions? options = null)
    {
        if (options != null)
            return JsonSerializer.Deserialize<T>(json, options);

        return JsonSerializer.Deserialize<T>(json, _camelCaseJsonSerializer);
    }
        
    public static string? Serialize<T>(this T source, JsonSerializerOptions? options = null)
    {
        if (options != null)
            return JsonSerializer.Serialize<T>(source, options);
        
        return JsonSerializer.Serialize<T>(source, _camelCaseJsonSerializer);
    }
}

