using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tracker.Api.Tests;

/// <summary>
/// Shared JSON serializer options that match the API's configuration
/// (enums serialized as strings).
/// </summary>
public static class TestJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
}
