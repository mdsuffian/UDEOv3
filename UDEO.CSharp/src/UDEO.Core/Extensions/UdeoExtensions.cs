using System.Text.Json;

namespace UDEO.Core.Extensions;

/// <summary>
/// Extension methods for JSON serialization of UDEO models.
/// </summary>
public static class UdeoExtensions
{
    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly JsonSerializerOptions PrettyOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string ToJson<T>(this T obj, bool indented = false)
        => JsonSerializer.Serialize(obj, indented ? PrettyOptions : CompactOptions);

    public static T? FromJson<T>(this string json)
        => JsonSerializer.Deserialize<T>(json, CompactOptions);

    /// <summary>
    /// Deep-clones an object via JSON round-trip.
    /// </summary>
    public static T DeepClone<T>(this T obj) where T : class
    {
        var json = obj.ToJson();
        return json.FromJson<T>()!;
    }

    /// <summary>
    /// Safely converts an object to a double, returning null on failure.
    /// </summary>
    public static double? ToDoubleSafe(this object? value)
    {
        if (value is null) return null;
        if (value is double d) return d;
        if (value is int i) return i;
        if (value is long l) return l;
        if (value is float f) return f;
        if (double.TryParse(value.ToString(), out var parsed)) return parsed;
        return null;
    }

    /// <summary>
    /// Safely converts an object to an int, returning null on failure.
    /// </summary>
    public static int? ToIntSafe(this object? value)
    {
        if (value is null) return null;
        if (value is int i) return i;
        if (value is double d && d == Math.Truncate(d)) return (int)d;
        if (int.TryParse(value.ToString(), out var parsed)) return parsed;
        return null;
    }
}
