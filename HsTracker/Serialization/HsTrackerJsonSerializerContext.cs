using System.Text.Json.Serialization;
using HsTracker.Models.HearthstoneData;

namespace HsTracker.Serialization;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
)]
[JsonSerializable(typeof(HearthstoneDataCardsPage))]
[JsonSerializable(typeof(HearthstoneDataCard))]
internal partial class HsTrackerJsonSerializerContext : JsonSerializerContext { }
