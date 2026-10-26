using Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text;
using System.Text.Json;

namespace Voyager
{
    public class ReportGenerator
    {
        public static int CountBeltClusters(string? bodiesJson)
        {
            if (string.IsNullOrWhiteSpace(bodiesJson))
            {
                return 0;
            }

            try
            {
                using var document = JsonDocument.Parse(bodiesJson);
                return CountBeltClusters(document.RootElement);
            }
            catch (JsonException)
            {
                return 0;
            }
        }

        private static int CountBeltClusters(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.Array)
            {
                var count = 0;
                foreach (var item in root.EnumerateArray())
                {
                    if (IsBeltClusterBody(item))
                    {
                        count++;
                    }
                }

                return count;
            }

            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var propertyName in new[] { "Bodies", "bodies" })
                {
                    if (root.TryGetProperty(propertyName, out var bodies))
                    {
                        return CountBeltClusters(bodies);
                    }
                }
            }

            return 0;
        }

        private static bool IsBeltClusterBody(JsonElement item)
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            foreach (var propertyName in new[] { "bodyType", "BodyType", "type", "Type" })
            {
                if (item.TryGetProperty(propertyName, out var value))
                {
                    var bodyType = value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
                    if (string.Equals(bodyType, "BeltCluster", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(bodyType, "belt", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a report based on the EDSystemName (Orion Nebula systems) and the FSDJump data in the OrionDbContext. The report is saved to the specified data folder.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="dataFolder"></param>
        internal static void GenerateReport(ILogger logger, OrionDbContext dbContext, string dataFolder)
        {
            var orionSystemCount = dbContext.EDSystemName.Count();
            logger.Information("Found {Count} Orion Nebula systems in the database.", orionSystemCount);

            // Select the FSDJump records that match the Orion Nebula systems using a join
            var query = dbContext.FSDJump
                .Where(jump => dbContext.EDSystemName.Any(system => system.Name == jump.StarSystem))
                .GroupBy(jump => jump.StarSystem)
                .Select(group => new
                {
                    StarSystem = group.Key,
                    JumpCount = group.Count(),
                    LatestDate = group.Max(jump => jump.Timestamp),
                    EarliestDate = group.Min(jump => jump.Timestamp)
                })
                .ToList();

            logger.Information("Found {Count} FSD jumps for Orion Nebula systems.", query.Sum(x => x.JumpCount));
            var results = query.ToList();

            var latestFssScans = dbContext.FSSAllBodiesFound
                .Where(scan => dbContext.EDSystemName.Any(system => system.Name == scan.SystemName))
                .GroupBy(scan => scan.SystemName)
                .Select(group => group.OrderByDescending(scan => scan.Timestamp).First())
                .ToList();

            // compute overall stats
            var totalSystems = dbContext.EDSystemName.Count();
            var totalBeltClusters = latestFssScans.Sum(scan => CountBeltClusters(scan.Bodies));
            var visitedSystems = results.Count;
            var totalJumps = results.Sum(r => r.JumpCount);
            var earliestJump = results.Count > 0 ? results.Min(r => r.EarliestDate) : (DateTime?)null;
            var latestJump = results.Count > 0 ? results.Max(r => r.LatestDate) : (DateTime?)null;
            var percentComplete = totalSystems > 0 ? (double)visitedSystems / totalSystems * 100.0 : 0.0;
            var oldestJump = results.OrderBy(r => r.EarliestDate).Take(10).Select(r => $"<tr><td>{r.StarSystem}</td><td>{r.JumpCount}</td><td>{r.EarliestDate:yyyy-MM-dd HH:mm:ss}</td><td>{r.LatestDate:yyyy-MM-dd HH:mm:ss}</td></tr>");

            // Load the template and replace the placeholders with the actual values from the query results.
            var sb = new StringBuilder();
            sb.Append(File.ReadAllText("Template.html"));
            sb.Replace("{{TOTAL_SYSTEMS}}", totalSystems.ToString());
            sb.Replace("{{TOTAL_BELT_CLUSTERS}}", totalBeltClusters.ToString());
            sb.Replace("{{VISITED_SYSTEMS}}", visitedSystems.ToString());
            sb.Replace("{{TOTAL_JUMPS}}", totalJumps.ToString());
            sb.Replace("{{EARLIEST_JUMP}}", earliestJump?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
            sb.Replace("{{LATEST_JUMP}}", latestJump?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
            sb.Replace("{{PERCENT_COMPLETE}}", percentComplete.ToString("F2") + "%");
            sb.Replace("{{PERCENT_VALUE}}", percentComplete.ToString("F2"));
            sb.Replace("{{SYSTEM_ROWS}}", string.Join("", oldestJump));
            sb.Replace("{{GENERATED_AT}}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

            // write file
            var outPath = Path.Combine(dataFolder, "index.html");
            File.WriteAllText(outPath, sb.ToString(), Encoding.UTF8);
            logger.Information("Wrote report to {ReportPath}", outPath);
        }
    }
}
