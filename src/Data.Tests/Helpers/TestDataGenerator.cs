using System;
using System.Text.Json;

namespace Data.Tests.Helpers
{
    /// <summary>
    /// Helper class to generate synthetic Elite Dangerous journal JSON lines matching the journal format.
    /// </summary>
    public static class TestDataGenerator
    {
        private static int _systemCounter = 1;
        private static int _bodyCounter = 1;

        /// <summary>
        /// Generate a FSDJump event JSON line.
        /// </summary>
        public static string GenerateFSDJumpJson(int index)
        {
            var timestamp = DateTime.UtcNow.AddSeconds(-1000 + index);
            var systemAddress = 1000000000L + index;
            var systemName = $"Test System {_systemCounter++}";

            var json = new
            {
                @event = "FSDJump",
                Timestamp = timestamp,
                StarSystem = systemName,
                SystemAddress = systemAddress,
                StarPos = new[] { 10.5 + index * 0.1, 20.3 - index * 0.05, 30.1 + index * 0.02 },
                JumpDist = 5.5 + (index % 10) * 0.5,
                FuelUsed = 2.5,
                FuelLevel = 25.0,
                StarClass = "G"
            };

            return JsonSerializer.Serialize(json);
        }

        /// <summary>
        /// Generate a FSSAllBodiesFound event JSON line.
        /// </summary>
        public static string GenerateFSSAllBodiesFoundJson(int index)
        {
            var timestamp = DateTime.UtcNow.AddSeconds(-500 + index);
            var systemAddress = 2000000000L + index;

            var json = new
            {
                @event = "FSSAllBodiesFound",
                Timestamp = timestamp,
                SystemAddress = systemAddress,
                Count = 15 + (index % 5)
            };

            return JsonSerializer.Serialize(json);
        }

        /// <summary>
        /// Generate a SupercruiseExit event JSON line.
        /// </summary>
        public static string GenerateSupercruiseExitJson(int index)
        {
            var timestamp = DateTime.UtcNow.AddSeconds(-300 + index);
            var systemAddress = 2500000000L + index;
            var bodyName = $"Test Body {_bodyCounter++}";

            var json = new
            {
                @event = "SupercruiseExit",
                Timestamp = timestamp,
                SystemAddress = systemAddress,
                StarSystem = $"System {index}",
                Body = bodyName,
                BodyID = 1 + (index % 20),
                BodyType = index % 2 == 0 ? "Planet" : "Star"
            };

            return JsonSerializer.Serialize(json);
        }

        /// <summary>
        /// Generate a ProspectedAsteroid event JSON line.
        /// </summary>
        public static string GenerateProspectedAsteroidJson(int index)
        {
            var timestamp = DateTime.UtcNow.AddSeconds(-200 + index);

            // Materials array needs to be serialized as JSON string within Materials property
            var materialsArray = new[]
            {
                new { Name = "Iron", Percent = 20.5 + (index % 5) },
                new { Name = "Nickel", Percent = 15.2 + (index % 3) },
                new { Name = "Palladium", Percent = 8.3 - (index % 2) }
            };
            var materialsJson = JsonSerializer.Serialize(materialsArray);

            var json = new
            {
                @event = "ProspectedAsteroid",
                Timestamp = timestamp,
                Content = "Common",
                Content_Localised = "Common Materials",
                Materials = materialsJson,
                Remaining = 75.0 + (index % 10)
            };

            return JsonSerializer.Serialize(json);
        }

        /// <summary>
        /// Generate a MiningRefined event JSON line.
        /// </summary>
        public static string GenerateMiningRefinedJson(int index)
        {
            var timestamp = DateTime.UtcNow.AddSeconds(-100 + index);
            var materials = new[] { "Painite", "Platinum", "Palladium", "Gold" };

            var json = new
            {
                @event = "MiningRefined",
                Timestamp = timestamp,
                Category = "metals",
                Type = materials[index % materials.Length],
                Amount = (index % 4) + 1
            };

            return JsonSerializer.Serialize(json);
        }

        /// <summary>
        /// Generate N FSDJump event JSON lines.
        /// </summary>
        public static List<string> GenerateFSDJumpBatch(int count)
        {
            _systemCounter = 1;
            var batch = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                batch.Add(GenerateFSDJumpJson(i));
            }
            return batch;
        }

        /// <summary>
        /// Generate N FSSAllBodiesFound event JSON lines.
        /// </summary>
        public static List<string> GenerateFSSAllBodiesFoundBatch(int count)
        {
            var batch = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                batch.Add(GenerateFSSAllBodiesFoundJson(i));
            }
            return batch;
        }

        /// <summary>
        /// Generate N SupercruiseExit event JSON lines.
        /// </summary>
        public static List<string> GenerateSupercruiseExitBatch(int count)
        {
            _bodyCounter = 1;
            var batch = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                batch.Add(GenerateSupercruiseExitJson(i));
            }
            return batch;
        }

        /// <summary>
        /// Generate N ProspectedAsteroid event JSON lines.
        /// </summary>
        public static List<string> GenerateProspectedAsteroidBatch(int count)
        {
            var batch = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                batch.Add(GenerateProspectedAsteroidJson(i));
            }
            return batch;
        }

        /// <summary>
        /// Generate N MiningRefined event JSON lines.
        /// </summary>
        public static List<string> GenerateMiningRefinedBatch(int count)
        {
            var batch = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                batch.Add(GenerateMiningRefinedJson(i));
            }
            return batch;
        }

        /// <summary>
        /// Create a temporary log file with JSON lines.
        /// </summary>
        public static string CreateTemporaryLogFile(List<string> jsonLines)
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"test_log_{Guid.NewGuid()}.log");
            File.WriteAllLines(filePath, jsonLines);
            return filePath;
        }

        /// <summary>
        /// Clean up temporary log file.
        /// </summary>
        public static void DeleteTemporaryLogFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch { /* Ignore cleanup errors */ }
            }
        }
    }
}
