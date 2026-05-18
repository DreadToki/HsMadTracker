using System.Text.Json.Serialization;
using HsTracker.Models;

namespace HsTracker.Serialization;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
)]
[JsonSerializable(typeof(HsCardsPage))]
[JsonSerializable(typeof(HsCard))]
internal partial class HsTrackerJsonSerializerContext : JsonSerializerContext { }
