using System;
using System.Collections.Generic;
using System.Text.Json;
using Data.Models;

namespace Data.Journal
{
    /// <summary>
    /// Minimal journal deserializer: reads the "event" property and deserializes
    /// the JSON string into the mapped CLR type. Falls back to returning the raw
    /// JSON string when no mapping is found.
    /// </summary>
    public static class JournalDeserializer
    {
        static readonly Dictionary<string, Type> EventTypeMap = new()
        {
            ["FSDJump"] = typeof(FSDJump),
            ["FSSAllBodiesFound"] = typeof(FSSAllBodiesFound),
            // Add additional mappings as you add models
        };

        static readonly JsonSerializerOptions DefaultOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Deserialize a single journal JSON line into a CLR instance based on the "event" field.
        /// Returns the typed instance when recognized, otherwise returns the original JSON string.
        /// </summary>
        public static object? DeserializeEvent(string json, JsonSerializerOptions? options = null)
        {
            options ??= DefaultOptions;

            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("event", out var ev))
            {
                return json; // no event property
            }

            var eventName = ev.GetString();
            if (string.IsNullOrEmpty(eventName)) return json;

            if (!EventTypeMap.TryGetValue(eventName, out var targetType))
            {
                return json; // unknown event
            }

            // Special-case mapping for FSDJump where journal contains StarPos array
            if (targetType == typeof(FSDJump) && doc.RootElement.TryGetProperty("StarPos", out var starPos) && starPos.ValueKind == JsonValueKind.Array)
            {
                // Deserialize into a DTO that models the raw shape, then map into FSDJump
                var dto = JsonSerializer.Deserialize<FSDJumpDto>(json, options);
                if (dto == null) return null;

                var model = new FSDJump
                {
                    Timestamp = dto.Timestamp,
                    Event = dto.Event ?? string.Empty,
                    StarSystem = dto.StarSystem ?? string.Empty,
                    SystemAddress = dto.SystemAddress,
                    StarPosX = dto.StarPos?.Length > 0 ? dto.StarPos[0] : (double?)null,
                    StarPosY = dto.StarPos?.Length > 1 ? dto.StarPos[1] : (double?)null,
                    StarPosZ = dto.StarPos?.Length > 2 ? dto.StarPos[2] : (double?)null,
                    JumpDist = dto.JumpDist,
                    FuelUsed = dto.FuelUsed,
                    FuelLevel = dto.FuelLevel,
                    StarClass = dto.StarClass,
                    RawJson = json
                };

                return model;
            }

            // Default: direct deserialize into the target type
            try
            {
                var obj = JsonSerializer.Deserialize(json, targetType, options);
                return obj;
            }
            catch
            {
                // If direct deserialization fails, return the raw JSON so caller can decide
                return json;
            }
        }

        // DTO for FSDJump raw JSON shape
        private class FSDJumpDto
        {
            public DateTime Timestamp { get; set; }
            public string? Event { get; set; }
            public string? StarSystem { get; set; }
            public long? SystemAddress { get; set; }
            public double[]? StarPos { get; set; }
            public double? JumpDist { get; set; }
            public double? FuelUsed { get; set; }
            public double? FuelLevel { get; set; }
            public string? StarClass { get; set; }
        }
    }
}
